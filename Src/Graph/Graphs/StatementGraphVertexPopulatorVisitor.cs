namespace Reflector.Graph.Graphs
{
	using System;
	using System.Collections;
	using Reflector;
	using Reflector.CodeModel;
	using Reflector.CodeModel.Memory;
	using QuickGraph.Exceptions;

    public sealed class StatementGraphVertexPopulatorVisitor : Visitor
	{
		private StatementGraph graph;
		public StatementGraphVertexPopulatorVisitor(StatementGraph graph)
		{
			if (graph==null)
				throw new ArgumentNullException("graph");
			this.graph = graph;
		}
		
		public StatementGraph Graph		
		{
			get
			{
				return this.graph;
			}			
		}

        public override void VisitStatement(IStatement statement)
		{
            IBlockStatement blockStatement = statement as IBlockStatement;
            if (blockStatement != null)
            {
                if (blockStatement.Statements.Count > 0)
                    this.graph.AddVertex(statement);
                base.VisitStatement(statement);
            }
            else
            {
                // add vertex in the graph
                if (statement != null)
                    this.graph.AddVertex(statement);
                base.VisitStatement(statement);
            }
		}
	}
}
