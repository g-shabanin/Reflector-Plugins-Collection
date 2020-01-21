namespace Reflector.Graph.Graphs
{
	using System;
	using QuickGraph;
	using Reflector.CodeModel;
	
	public sealed class StatementVertex : QuickGraph.Vertex
	{
		private IStatement statement=null;
		
		public StatementVertex(int id)
		:base(id)
		{}
		
		public IStatement Statement
		{
			get
			{
				if (this.statement==null)
					throw new InvalidOperationException();
				return this.statement;
			}
			set
			{
				this.statement = value;
			}
		}
		
		public override string ToString()		
		{
			if (this.statement==null)
				return "Empty StatementVertex";
			else
				return String.Format("{0}: {1}",base.ToString(),this.statement.ToString());
		}

        internal class Provider : QuickGraph.Providers.TypedVertexProvider
        {
            public Provider()
		    :base(typeof(StatementVertex))
            { }
        }
    }
}
