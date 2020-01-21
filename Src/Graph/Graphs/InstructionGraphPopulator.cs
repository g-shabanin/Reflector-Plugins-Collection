namespace Reflector.Graph.Graphs
{
	using System;
	using System.Collections;
	using QuickGraph.Collections;
	using QuickGraph.Algorithms;
	using QuickGraph.Algorithms.Search;
	using QuickGraph.Algorithms.Visitors;
	using QuickGraph.Concepts.Traversals;
	using Reflector;
	using Reflector.CodeModel;

	internal sealed class InstructionGraphPopulator
	{
		private FlowToCodeConverter flowConverter = new FlowToCodeConverter();
		private Hashtable instructionVertices = null;
		private InstructionGraph graph=null;

		public InstructionGraphPopulator()
		{}

		public InstructionGraph BuildGraphFromMethod(IMethodDeclaration method)
		{			
			if (method==null)
				throw new ArgumentNullException("method");
			// create graph			
			this.graph = new InstructionGraph(method);
			this.instructionVertices = new Hashtable();
			
			// first add all instructions			
			foreach(IInstruction i in this.graph.Body.Instructions)
			{
				// avoid certain instructions
				if (flow(i.Code) == ((System.Reflection.Emit.FlowControl) 6) /* Phi */
					|| flow(i.Code) == System.Reflection.Emit.FlowControl.Meta)
					continue;
				
				// add vertex
				InstructionVertex iv = graph.AddVertex();
				iv.Instruction = i;
				
				this.instructionVertices.Add(i.Offset,iv);
			}
				
			// iterating over the instructions
            search(null, this.graph.Body.Instructions.GetEnumerator());
			
			// iterating of the the try/catch handler
            searchExceptions(this.graph.Body.ExceptionHandlers);
			
			return this.graph;
		}

		public InstructionGraph Graph
		{
			get
			{
				return this.graph;
			}
		}

		public EdgeCollectionCollection GetAllEdgePaths()
		{
			if (this.graph.VerticesCount==0)
				return new EdgeCollectionCollection();

			DepthFirstSearchAlgorithm efs = new DepthFirstSearchAlgorithm(this.graph);
			PredecessorRecorderVisitor vis =new PredecessorRecorderVisitor();

			efs.RegisterPredecessorRecorderHandlers(vis);

			// get root vertex
			efs.Compute(this.Graph.Root);

			return vis.AllPaths();
		}
		
		private System.Reflection.Emit.FlowControl flow(int code)
		{
			return this.flowConverter.Convert(code);
		}

		private void searchExceptions(IExceptionHandlerCollection exceptions)
		{
			if (exceptions==null)
				return;
			
			// handle all catch			
			foreach(IExceptionHandler handler in exceptions)
			{
				if (handler.TryOffset == handler.HandlerOffset)
					continue;
				InstructionVertex tv = vertexFromOffset(handler.TryOffset);
				
				if (handler.Type == ExceptionHandlerType.Catch)
				{
					InstructionVertex cv = vertexFromOffset(handler.HandlerOffset);
					graph.AddEdge(tv,cv);
				}
				if (handler.Type == ExceptionHandlerType.Filter)
				{
					InstructionVertex cv = vertexFromOffset(handler.FilterOffset);
					graph.AddEdge(tv,cv);
				}				
				if (handler.Type == ExceptionHandlerType.Finally)
				{
					InstructionVertex fv = vertexFromOffset(handler.HandlerOffset);
					graph.AddEdge(tv,fv);
					foreach(IExceptionHandler catchHandler in exceptions)
					{
						if (catchHandler.TryOffset == catchHandler.HandlerOffset)
							continue;
						if (handler.TryOffset != catchHandler.TryOffset)
							continue;
						if (handler.HandlerOffset == catchHandler.HandlerOffset)
							continue;
						InstructionVertex cv = vertexFromOffset(catchHandler.HandlerOffset);
						graph.AddEdge(cv,fv);
					}
				}
			}							
		}
		
		private void search(
			InstructionVertex parentVertex, 
			IEnumerator instructions)
		{
			InstructionVertex jv = null;
			InstructionVertex cv = parentVertex;
			while(instructions.MoveNext())
			{
				// add vertex to graph
				IInstruction i = (IInstruction)instructions.Current;
				System.Reflection.Emit.FlowControl f = flow(i.Code);
				// avoid certain instructions
				if (f == ((System.Reflection.Emit.FlowControl)6) /* Phi */ 
					|| f == System.Reflection.Emit.FlowControl.Meta)
					continue;

				InstructionVertex iv = vertexFromOffset(i.Offset);
				
				if (cv!=null)
				{
					graph.AddEdge(cv, iv);
				}				
				// find how to handle the rest
				switch(f)
				{
					case System.Reflection.Emit.FlowControl.Next:
						cv = iv;
						break;
					case System.Reflection.Emit.FlowControl.Call:
						cv=iv;
						break;
					case System.Reflection.Emit.FlowControl.Return:
						cv = null;
						break;
					case System.Reflection.Emit.FlowControl.Throw:
						cv = null;
						break;
					case System.Reflection.Emit.FlowControl.Cond_Branch:
						if (InstructionHelper.GetOperandType(i.Code) == OperandType.Switch)
						{
							foreach(int target in (int[])i.Value)
							{
								jv = vertexFromOffset(target);
								graph.AddEdge(iv,jv);
								search(iv,instructions);
							}
							cv=iv;						
						}
						else
						{
							jv = vertexFromOffset((int)iv.Instruction.Value);
							graph.AddEdge(iv,jv);
							cv = iv;
						}
						break;
					case System.Reflection.Emit.FlowControl.Branch:
						// add jump to offset
						jv = vertexFromOffset((int)iv.Instruction.Value);
						if (jv==null)
							throw new Exception("Could not find vertex");
						graph.AddEdge(iv,jv);
						cv=null;
						break;
					case System.Reflection.Emit.FlowControl.Break:
						// add jump to offset
						jv = vertexFromOffset((int)iv.Instruction.Value);
						graph.AddEdge(iv,jv);
						cv = null;
						break;
				}
			}
		}
		
		private InstructionVertex vertexFromOffset(int offset)
		{	
			InstructionVertex iv =  (InstructionVertex)this.instructionVertices[offset];
			if (iv==null)
				throw new InvalidOperationException("Could not find vertex at offset " + offset.ToString());
			return iv;
		}
	}
}
