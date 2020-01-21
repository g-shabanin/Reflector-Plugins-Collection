namespace Reflector.Graph.Graphs
{
	using System;
	using System.Collections;
	using Reflector;
	using Reflector.CodeModel;
	using Reflector.CodeModel.Memory;
	using QuickGraph.Exceptions;
    using QuickGraph.Algorithms;

    internal sealed class StatementGraphEdgePopulatorVisitor : Visitor
	{
		private StatementGraph graph;
		private Stack bodyStatements = new Stack();

        public StatementGraphEdgePopulatorVisitor(StatementGraph graph)
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

		public override void VisitBlockStatement(IBlockStatement statement)
		{
            if (statement.Statements.Count > 0)
            {
                // current -> childs
                StatementVertex source = this.graph.GetVertex(statement);
                foreach (IStatement targetStatement in statement.Statements)
                {
                    // create inner links
                    this.VisitStatement(targetStatement);

                    StatementVertex target = this.graph.GetVertex(targetStatement);
                    this.graph.AddEdge(source, target);

                    this.AddFlowEdges(source, target);

                    source = target;
                }

            }
        }

        private void AddFlowEdges(StatementVertex source, StatementVertex target)
        {
            IWhileStatement whileStatement = source.Statement as IWhileStatement;
            if (whileStatement != null && whileStatement.Body.Statements.Count>0)
            {
                AddEdgesToSourceSinks(source, target);
                return;
            }
            IForStatement forStatement = source.Statement as IForStatement;
            if (forStatement != null && forStatement.Body.Statements.Count > 0)
            {
                AddEdgesToSourceSinks(source, target);
                return;
            }
            IForEachStatement foreachStatement = source.Statement as IForEachStatement;
            if (foreachStatement != null && foreachStatement.Body.Statements.Count > 0)
            {
                AddEdgesToSourceSinks(source, target);
                return;
            }
            IDoStatement doStatement = source.Statement as IDoStatement;
            if (doStatement != null && doStatement.Body.Statements.Count > 0)
            {
                AddEdgesToSourceSinks(source, target);
                return;
            }
        }

        private void AddIterateEdge(IStatement statement, IBlockStatement body)
        {
            StatementVertex source = this.graph.GetVertex(statement);
            IStatement lastStatement = body.Statements[body.Statements.Count - 1];
            StatementVertex vl = this.graph.GetVertex(lastStatement);
            StatementEdge edge = this.graph.AddEdge(vl,source);
            edge.Name = "iterate";
        }

        private void AddEdgesToSourceSinks(StatementVertex source, StatementVertex target)
        {
            foreach (StatementVertex sink in AlgoUtility.Sinks(this.Graph, source))
            {
                if (sink.Statement == target.Statement)
                    continue;

                IBreakStatement breakStatement = sink.Statement as IBreakStatement;
                if (breakStatement != null)
                {
                    StatementEdge edge = this.graph.AddEdge(sink, target);
                    edge.Name = "break;";
                    continue;
                }
            }
        }

        public override void VisitConditionStatement(IConditionStatement statement)
		{
			// current -> then
			StatementVertex source = this.graph.GetVertex(statement);
            if (statement.Then.Statements.Count > 0)
            {
                StatementVertex then = this.graph.GetVertex(statement.Then);
                StatementEdge edge = this.graph.AddEdge(source, then);
                edge.Name = "then";
            }

            // condition -> Else (if any)
			if (statement.Else.Statements.Count>0)
			{				
				StatementVertex _else = this.graph.GetVertex(statement.Else);
				StatementEdge elseEdge = this.graph.AddEdge(source,_else);
                elseEdge.Name = "else";
            }
			
			base.VisitConditionStatement(statement);
		}

		public override void VisitTryCatchFinallyStatement(ITryCatchFinallyStatement statement)
		{
			// current -> try
			StatementVertex source = this.graph.GetVertex(statement);
            StatementVertex vTry = null;
            if (statement.Try.Statements.Count > 0)
            {
                vTry = this.graph.GetVertex(statement.Try);
                StatementEdge edge = this.graph.AddEdge(source, vTry);
                edge.Name = "try";
            }

            StatementVertex vFinally = null;
			if (statement.Finally!=null && statement.Finally.Statements.Count>0)
				vFinally = this.graph.GetVertex(statement.Finally);

            if (vTry != null)
            {
                // try -> each catch 
                foreach (ICatchClause catchClause in statement.CatchClauses)
                {
                    StatementVertex vCatch = this.graph.GetVertex(catchClause.Body);
                    StatementEdge edge = this.graph.AddEdge(vTry, vCatch);
                    edge.Name = String.Format("catch({0} {1})",
                        catchClause.Variable.VariableType,
                        catchClause.Variable.Name
                        );
                }
            }

            if (vFinally != null)
            {	
				// try -> finally
				this.graph.AddEdge(vTry,vFinally);
				
				// catch -> finally				
                foreach (ICatchClause catchClause in statement.CatchClauses)
                {
					StatementVertex vCatch = this.graph.GetVertex(catchClause.Body);
					StatementEdge edge = this.graph.AddEdge(vCatch,vFinally);
                    edge.Name = "finally";
                }				
			}
			
			// finally -> fault, not yet			
						
			base.VisitTryCatchFinallyStatement(statement);
		}

		public override void VisitForStatement(IForStatement statement)
		{
			// source -> init
			StatementVertex source = this.graph.GetVertex(statement);
			StatementVertex vInit = this.graph.GetVertex(statement.Initializer);			
			this.graph.AddEdge(source,vInit);

			// adding the last statement
            if (statement.Body.Statements.Count > 0)
            {
                StatementVertex body = this.graph.GetVertex(statement.Body);
                this.graph.AddEdge(vInit, body);

                // storing body statement
                this.bodyStatements.Push(statement);
                base.VisitForStatement(statement);
                this.bodyStatements.Pop();

                // add increment expression
                this.AddIterateEdge(statement, statement.Body);
            }
        }

		public override void VisitForEachStatement(IForEachStatement statement)
		{
			StatementVertex source = this.graph.GetVertex(statement);
            if (statement.Body.Statements.Count > 0)
            {
                StatementVertex body = this.graph.GetVertex(statement.Body);
                this.graph.AddEdge(source, body);

                this.bodyStatements.Push(statement);
                base.VisitForEachStatement(statement);
                this.bodyStatements.Pop();

                this.AddIterateEdge(statement, statement.Body);
            }
        }

		public override void VisitUsingStatement(IUsingStatement statement)
		{
			StatementVertex source = this.graph.GetVertex(statement);
            if (statement.Body.Statements.Count > 0)
            {
                StatementVertex body = this.graph.GetVertex(statement.Body);
                StatementEdge edge = this.graph.AddEdge(source, body);
                edge.Name = "then";

                this.bodyStatements.Push(statement);
                base.VisitUsingStatement(statement);
                this.bodyStatements.Pop();
            }
		}

		public override void VisitWhileStatement(IWhileStatement statement)
		{
			StatementVertex source = this.graph.GetVertex(statement);
            if (statement.Body.Statements.Count > 0)
            {
                StatementVertex body = this.graph.GetVertex(statement.Body);
                StatementEdge edge = this.graph.AddEdge(source, body);
                edge.Name = "then";

                this.bodyStatements.Push(statement);
                base.VisitWhileStatement(statement);
                this.bodyStatements.Pop();

                // add 
                AddIterateEdge(statement,statement.Body);
            }
        }

		public override void VisitDoStatement(IDoStatement statement)
		{
			StatementVertex source = this.graph.GetVertex(statement);
            if (statement.Body.Statements.Count > 0)
            {
                StatementVertex body = this.graph.GetVertex(statement.Body);
                this.graph.AddEdge(source, body);

                this.bodyStatements.Push(statement);
                base.VisitDoStatement(statement);
                this.bodyStatements.Pop();

                this.AddIterateEdge(statement, statement.Body);
            }
		}

		public override void VisitBreakStatement(IBreakStatement statement)
		{
			// add edge to last body
			if (this.bodyStatements.Count==0)
				throw new InvalidOperationException("body stack is empty");
            base.VisitBreakStatement(statement);	
		}

		public override void VisitContinueStatement(IContinueStatement statement)
		{
            // add edge to last body
            if (this.bodyStatements.Count == 0)
                throw new InvalidOperationException("body stack is empty");

            IStatement body = this.bodyStatements.Peek() as IStatement;
            StatementEdge edge = this.graph.AddEdge(
                this.graph.GetVertex(statement), 
                this.graph.GetVertex(body)
                );
            edge.Name = "continue;";

            base.VisitContinueStatement(statement);
        }
	}
}
