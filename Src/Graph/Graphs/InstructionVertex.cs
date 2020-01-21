using System;
using QuickGraph;
using System.Reflection;
using System.IO;
using Reflector.CodeModel;

namespace Reflector.Graph.Graphs
{
	
	internal sealed class InstructionVertex : QuickGraph.Vertex
	{
		private IInstruction instruction=null;
		
		public InstructionVertex(int id)
		:base(id)
		{}
		
		public IInstruction Instruction
		{
			get
			{
				if (this.instruction==null)
					throw new InvalidOperationException();
				return this.instruction;
			}
			set
			{
				this.instruction = value;
			}
		}
		
		public override string ToString()
		{
			using (StringWriter writer = new StringWriter())
			{
				writer.Write("L" + this.instruction.Offset.ToString("X4"));
				writer.Write(": ");
				writer.Write(InstructionHelper.GetInstructionName(this.instruction.Code));

				object value = this.instruction.Value;
				if (value != null)
				{
					writer.Write(" ");
					writer.Write(value.ToString());
				}

				return writer.ToString();
			}
		}
	}
	
	public class InstructionVertexProvider : QuickGraph.Providers.TypedVertexProvider
	{
		public InstructionVertexProvider()
		:base(typeof(InstructionVertex))
		{}
	}
}
