using System;
using System.Collections;
using System.IO;
using Reflector.CodeMetrics;

namespace Reflector.Graph.Graphs
{
	using QuickGraph;
	using QuickGraph.Collections;
	using QuickGraph.Providers;
	using QuickGraph.Concepts;
	using QuickGraph.Representations;
    using QuickGraph.Concepts.Predicates;
	using QuickGraph.Concepts.Traversals;
	using Reflector;
	using Reflector.CodeModel;
	using QuickGraph.Concepts.Collections;
	using QuickGraph.Predicates;
	
	public sealed class MethodRankPopulator
	{
        private ComputationState state = ComputationState.Idle;
        private BidirectionalGraph graph=null;
		private Hashtable methodVertices = new Hashtable();
		private PageRankAlgorithm pageRank = null;
        private int stepCount=0;
        private int currentCount = 0;
        private VertexCollection isolatedVertices = new VertexCollection();

        public MethodRankPopulator()
		{}
		
		public BidirectionalGraph Graph
		{
			get
			{
				return this.graph;
			}
		}
		
		public PageRankAlgorithm PageRank
		{
			get
			{
				return this.pageRank;
			}
		}

        public ComputationState State
        {
            get 
            {
                lock (this)
                {
                    return this.state;
                }
            }
        }

        public event ComputationProgressEventHandler Progress;
        private void OnProgress(ComputationProgressEventArgs args)
        {
            if (this.Progress != null)
                this.Progress(this, args);
        }

        public void Abort()
        {
            lock (this)
            {
                this.state = ComputationState.AbortPending;
            }
        }

        private bool CheckAbortPending()
        {
            if (state == ComputationState.AbortPending)
            {
                lock (this)
                {
                    this.state = ComputationState.Idle;
                    return true;
                }
            }
            return false;
        }
		
		private bool ContainsVertex(IMethodDeclaration method)
		{
			return this.methodVertices.Contains(method);
		}

		private void AddEdge(IMethodDeclaration source, IMethodReference target)
		{
			IVertex vs = (IVertex)this.methodVertices[source];
            if (vs == null)
                return;
			IVertex vt = (IVertex)this.methodVertices[target];
            if (vt == null)
                return;
			if (this.graph.ContainsEdge(vs,vt))
				return;
			else
				this.graph.AddEdge(vs,vt);			
		}
		
		private void Initialize()
		{
			this.methodVertices.Clear();
            this.stepCount = 1;
            this.currentCount = 0;

            this.graph = new BidirectionalGraph(
				new CustomVertexProvider(),
				new EdgeProvider(),
				false);
		}

        public int GetStepCount(IAssemblyCollection assemblies)
        {
            int stepCount = 0;
            foreach (IAssembly assembly in assemblies)
                stepCount += GetStepCount(assembly);
            return stepCount;
        }
        public int GetStepCount(IAssembly assembly)
        {
            int stepCount = 0;
            foreach (IModule module in assembly.Modules)
                stepCount += module.Types.Count;
            return stepCount*2;
        }

        public void PopulateGraph(IAssemblyCollection assemblies, ICodeMetricManager codeMetricManager)
        {
            this.Initialize();

            this.stepCount = GetStepCount(assemblies);

            // load types from module
            foreach (IAssembly assembly in assemblies)
            {
                if (this.CheckAbortPending())
                    return;
                if (codeMetricManager.IsIgnored(assembly))
                    continue;
                this.loadTypesFromAssembly(assembly);
            }

            // create link
            foreach (IAssembly assembly in assemblies)
            {
                if (this.CheckAbortPending())
                    return;
                if (codeMetricManager.IsIgnored(assembly))
                    continue;
                this.createLinksFromAssembly(assembly);
            }
        }

        public void PopulateGraph(IAssembly assembly)
		{
            this.Initialize();

            this.stepCount = GetStepCount(assembly);

            // load types from module
            if (this.CheckAbortPending())
                return;
            this.loadTypesFromAssembly(assembly);

            // create link
            if (this.CheckAbortPending())
                return;
            this.createLinksFromAssembly(assembly);
        }

		private void RemoveIsolatedVertices()
		{
           this.isolatedVertices.Clear();
 			foreach(IVertex v in this.Graph.Vertices)
			{
				if (this.graph.OutEdgesEmpty(v) && this.graph.InEdgesEmpty(v))
					isolatedVertices.Add(v);
			}

            foreach(CustomVertex v in isolatedVertices)
			{
			    this.graph.RemoveVertex(v);
			}
		}

        private void RestoreIsolatedVertices()
        {
            foreach (CustomVertex v in this.isolatedVertices)
            {
                this.Graph.AddVertex(v);
            }
            this.isolatedVertices.Clear();
        }

        private void RemoveSinks()
		{
			int count;
			VertexCollection vs = new VertexCollection();
			do
			{
				count = this.graph.VerticesCount;
				foreach(IVertex v in this.Graph.Vertices)
				{
					if (this.graph.OutDegree(v)==0)
						vs.Add(v);
				}
				foreach(CustomVertex v in vs)
				{
					this.graph.RemoveVertex(v);
					this.methodVertices.Remove(v.Value);
				}
				vs.Clear();
			}while(this.graph.VerticesCount!=count);
		}

        public void ComputePageRank()
        {
            this.OnProgress(
                new ComputationProgressEventArgs(0,4,"Removing isolated vertices"));
            //this.RemoveIsolatedVertices();
            //	this.RemoveSinks();

            this.pageRank = new PageRankAlgorithm(this.Graph);
            this.OnProgress(
                new ComputationProgressEventArgs(1, 4, "Initializing ranks"));
            this.pageRank.InitializeRanks();
            this.OnProgress(
                new ComputationProgressEventArgs(2, 4, "Compute ranks"));
            this.pageRank.Compute();

            this.OnProgress(
                new ComputationProgressEventArgs(3, 4, "Restoring isolated vertices"));
            //this.RestoreIsolatedVertices();
            this.OnProgress(
                new ComputationProgressEventArgs(4, 4, "MethodRank finished"));
        }

        private void loadTypesFromAssembly(
            IAssembly assembly)
        {
            foreach (IModule module in assembly.Modules)
            {
                if (this.CheckAbortPending())
                    return;
                this.loadTypesFromModule(module);
            }
        }

        private void loadTypesFromModule(IModule module)
		{
            foreach (ITypeDeclaration type in module.Types)
            {
                if (this.CheckAbortPending())
                    return;
                this.loadMethodsFromType(type);
            }
        }

        private void createLinksFromAssembly(IAssembly assembly)
        {
            foreach (IModule module in assembly.Modules)
            {
                if (this.CheckAbortPending())
                    return;
                this.createLinksFromModule(module);
            }
        }

        private void createLinksFromModule(IModule module)
		{
			foreach(ITypeDeclaration type in module.Types)
			{
                if (this.CheckAbortPending())
                    return;

                this.currentCount++;
                this.OnProgress(
                    new ComputationProgressEventArgs(
                        currentCount,
                        stepCount, 
                        String.Format("Adding edges for {0}",new TypeInformation(type).NameWithResolutionScope))
                        );
                foreach (IMethodDeclaration method in type.Methods)
                {
                    if (this.CheckAbortPending())
                        return;
                    this.createLinksFromBody(method);
                }
            }
		}
		
		private void loadMethodsFromType(ITypeDeclaration type)
		{
            if (this.CheckAbortPending())
                return;

            string message = String.Format("Adding vertices for {0}",
                new TypeInformation(type).NameWithResolutionScope);
            this.currentCount++;
            this.OnProgress(
                new ComputationProgressEventArgs(
                    currentCount,
                    stepCount,
                    message)
                    );
            foreach (IMethodReference method in type.Methods)
            {
                if (this.CheckAbortPending())
                    return;
                if (this.methodVertices.Contains(method))
                    continue;
                this.AddMethod(method);
			}
		}

		private CustomVertex AddMethod(IMethodReference method)
		{
			CustomVertex v =(CustomVertex)this.graph.AddVertex();
			v.Value = method;
			this.methodVertices.Add(method,v);
			// add rank
			if (this.pageRank!=null)
				this.pageRank.Ranks.Add(v,0);

			return v;
		}
		
		private void createLinksFromBody(IMethodDeclaration method)
		{
			// visitor method
			IMethodBody body = (IMethodBody)method;
			foreach(IInstruction il in body.Instructions)
			{
				IMethodReference methodRef = il.Operand as IMethodReference;
				if (methodRef==null)
					continue;

				this.AddEdge(method,methodRef);
			}
		}

        private sealed class SinkEdgePredicate : IEdgePredicate
        {
            private IIncidenceGraph graph;
            public SinkEdgePredicate(IIncidenceGraph graph)
            {
                this.graph = graph;
            }
            public bool Test(IEdge e)
            {
                return this.graph.OutDegree(e.Target) == 0;
            }
        }

        private sealed class NonIsolatedVertexPredicate : IVertexPredicate
        {
            private IBidirectionalVertexListGraph graph;
            public NonIsolatedVertexPredicate(IBidirectionalVertexListGraph graph)
            {
                this.graph = graph;
            }
            public bool Test(IVertex v)
            {
                return this.graph.InDegree(v) != 0 || this.graph.OutDegree(v) != 0;
            }
        }

        private sealed class FilteredBidirectionalVertexListGraph : IBidirectionalVertexListGraph
        {
            private IBidirectionalVertexListGraph graph;
            private IVertexPredicate vertexPredicate;

            public FilteredBidirectionalVertexListGraph(
                IBidirectionalVertexListGraph graph,
                IVertexPredicate vertexPredicate
                )
            {
                this.graph = graph;
                this.vertexPredicate = vertexPredicate;
            }

            #region IVertexListGraph Members

            public bool ContainsVertex(IVertex v)
            {
                if (!this.vertexPredicate.Test(v))
                    return false;
                return this.graph.ContainsVertex(v);
            }
            public IVertexEnumerable Vertices
            {
                get 
                { 
                    return new FilteredVertexEnumerable(this.graph.Vertices,this.vertexPredicate);
                }
            }

            public int VerticesCount
            {
                get 
                { 
                    int i = 0;
                    foreach (IVertex v in this.Vertices)
                        i++;
                    return i;
                }
            }

            public bool VerticesEmpty
            {
                get 
                {
                    foreach (IVertex v in this.Vertices)
                        return false;
                    return true;
                }
            }


            #endregion

            #region IIncidenceGraph Members

            public bool ContainsEdge(IVertex u, IVertex v)
            {
                if (!this.vertexPredicate.Test(u))
                    return false;
                if (!this.vertexPredicate.Test(v))
                    return false;
                return this.graph.ContainsEdge(u, v);
            }

            #endregion

            #region IAdjacencyGraph Members

            public IVertexEnumerable AdjacentVertices(IVertex v)
            {
                return new FilteredVertexEnumerable(
                    this.graph.AdjacentVertices(v),
                    this.vertexPredicate
                    );
            }

            #endregion

            #region IImplicitGraph Members

            public int OutDegree(IVertex v)
            {
                int count = 0;
                foreach (IEdge edge in this.OutEdges(v))
                    count++;
                return count;
            }

            public IEdgeEnumerable OutEdges(IVertex v)
            {
                return new FilteredEdgeEnumerable(
                    this.graph.OutEdges(v),
                    new EdgeVertexPredicate(this.vertexPredicate)
                    );
            }

            public bool OutEdgesEmpty(IVertex v)
            {
                foreach (IEdge edge in this.OutEdges(v))
                    return false;
                return true;
            }

            #endregion

            #region IGraph Members
            public bool AllowParallelEdges
            {
                get 
                {
                    return this.graph.AllowParallelEdges;
                }
            }

            public bool IsDirected
            {
                get 
                {
                    return this.graph.IsDirected;
                }
            }


            #endregion

            #region IBidirectionalGraph Members

            public bool AdjacentEdgesEmpty(IVertex v)
            {
                throw new NotImplementedException();
            }

            public int Degree(IVertex v)
            {
                return - this.InDegree(v) + this.OutDegree(v);
            }

            public int InDegree(IVertex v)
            {
                int count = 0;
                foreach (IEdge e in this.InEdges(v))
                    count++;
                return count;
            }

            public IEdgeEnumerable InEdges(IVertex v)
            {
                return new FilteredEdgeEnumerable(
                    this.graph.InEdges(v),
                    new EdgeVertexPredicate(this.vertexPredicate)
                    );
            }

            public bool InEdgesEmpty(IVertex v)
            {
                if (!this.vertexPredicate.Test(v))
                    return true;
                return this.graph.InEdgesEmpty(v);
            }

            #endregion

            private sealed class EdgeVertexPredicate : IEdgePredicate
            {
                private IVertexPredicate vertexPredicate;
                public EdgeVertexPredicate(IVertexPredicate vertexPredicate)
                {
                    this.vertexPredicate = vertexPredicate;
                }
                public bool Test(IEdge edge)
                {
                    return
                        this.vertexPredicate.Test(edge.Source) &&
                        this.vertexPredicate.Test(edge.Target);
                }
            }
        }
    }
}
