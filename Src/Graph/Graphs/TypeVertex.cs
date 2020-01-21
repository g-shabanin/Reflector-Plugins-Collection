using System;

namespace Reflector.Graph.Graphs
{
	using QuickGraph;
	using QuickGraph.Concepts.Providers;
	using Reflector.CodeModel;

	public class TypeVertex : Vertex
	{
		private ITypeDeclaration type;

		public TypeVertex(int id)
			:base(id)
		{}

		public ITypeDeclaration Type
		{
			get
			{
				if (this.type==null)
					throw new ArgumentNullException("type");
				return this.type;
			}
			set
			{
				if (value==null)
					throw new ArgumentNullException("type");
				this.type=value;
			}
		}

		public class Provider : IVertexProvider
		{
			private int nextID=0;
			#region IVertexProvider Members
			public Type VertexType
			{
				get
				{
					return typeof(TypeVertex);
				}
			}

			public void UpdateVertex(QuickGraph.Concepts.IVertex v)
			{
				TypeVertex tv = (TypeVertex)v;
				tv.ID=nextID++;
			}

			public QuickGraph.Concepts.IVertex ProvideVertex()
			{
				return new TypeVertex(nextID++);
			}

			#endregion

		}

	}
}
