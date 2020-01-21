namespace Reflector.Graph.Graphs
{
	using System;
	using QuickGraph.Concepts;
	using System.IO;
	using System.Text;
	using System.Globalization;
	using Reflector.CodeModel;
    using Microsoft.Glee.Drawing;

	public sealed class StatementGraphRenderer
	{
		private ILanguageManager languageManager;
		private ILanguage language = null;

		private StatementGraph graph;

        public StatementGraphRenderer(StatementGraph graph, ILanguageManager languageManager)
		{
			this.graph = graph;
			this.languageManager = languageManager;
        }

		public StatementGraph Graph
		{
			get
			{
				return this.graph;
			}
		}

        public Microsoft.Glee.Drawing.Graph Render(Microsoft.Glee.Drawing.Graph glee)
		{
            this.language = this.languageManager.ActiveLanguage;

            foreach (StatementVertex v in this.Graph.Vertices)
            {
                Node node = (Node)glee.AddNode(v.ID.ToString());
                formatVertex(v, node);
            }

            foreach (StatementEdge e in this.Graph.Edges)
            {
                Edge edge = (Edge)glee.AddEdge(
                    e.SourceID.ToString(), e.TargetID.ToString());
                edge.Attr.Label = e.Name;
            }
            return glee;
		}
		
		private void formatVertex(StatementVertex v, Node node)
		{
            StatementFormatterVisitor vis = new StatementFormatterVisitor(
                this.graph,
                node,
                this.language
                );
            vis.VisitStatementVertex(v);
		}

        private class StatementFormatterVisitor
        {
            private StatementGraph graph;
            private Node node;
            private ILanguage language;

            private static readonly Color defaultColor = Color.LightYellow;
            private static readonly Color bifurcateColor = Color.LightBlue;
            private static readonly Color returnColor = Color.LightGreen;
            private static readonly Color jumpColor = Color.LightSalmon;
            private static readonly Color assignColor = Color.LightSteelBlue;

            public StatementFormatterVisitor(
                StatementGraph graph,
                Node node, 
                ILanguage language
                )
            {
                this.graph = graph;
                this.node = node;
                this.language = language;
            }

            private void SetLabel(string value)
            {
                string s = value;
                s = s.Replace('"', '\'');

                this.node.Attr.Label = s;
            }
            private void SetLabel(string format, params object[] args)
            {
                string label =String.Format(format,args);
                this.SetLabel(label);
            }
            private void SetFillColor(IStatement statement, Color color)
            {
                this.node.Attr.Fillcolor = color;
            }
            public void VisitStatementVertex(StatementVertex v)
            {
                // if sink
                if (this.graph.OutEdgesEmpty(v))
                {
                    this.node.Attr.Fillcolor = returnColor;
                }

                IBlockStatement blockStatement = v.Statement as IBlockStatement;
                if (blockStatement!=null)
                {
                    this.VisitBlockStatement(blockStatement);
                    return;
                }
                IExpressionStatement expressionStatement = v.Statement as IExpressionStatement;
                if (expressionStatement!=null)
                {
                    this.VisitExpressionStatement(expressionStatement);
                    return;
                }
                IConditionStatement conditionStatement = v.Statement as IConditionStatement;
                if (conditionStatement!=null)
                {
                    this.VisitConditionStatement(conditionStatement);
                    return;
                }    
                IContinueStatement continueStatement = v.Statement as IContinueStatement;
                if (continueStatement!=null)
                {
                    this.VisitContinueStatement(continueStatement);
                    return;
                }
                IBreakStatement breakStatement = v.Statement as IBreakStatement;
                if (breakStatement != null)
                {
                    this.VisitBreakStatement(breakStatement);
                }
                IForStatement forStatement = v.Statement as IForStatement;
                if (forStatement!=null)
                {
                    this.VisitForStatement(forStatement);
                    return;
                }
                IForEachStatement foreachStatement = v.Statement as IForEachStatement;
                if (foreachStatement!=null)
                {
                    this.VisitForEachStatement(foreachStatement);
                    return;
                }
                IWhileStatement whileStatement = v.Statement as IWhileStatement;
                if (whileStatement != null)
                {
                    this.VisitWhileStatement(whileStatement);
                    return;
                }
                IMethodReturnStatement methodReturnStatement = v.Statement as IMethodReturnStatement;
                if (methodReturnStatement != null)
                {
                    this.VisitMethodReturnStatement(methodReturnStatement);
                    return;
                }
                ITryCatchFinallyStatement tryCatchFinallyStatement = v.Statement as ITryCatchFinallyStatement;
                if (tryCatchFinallyStatement != null)
                {
                    this.VisitTryCatchFinallyStatement(tryCatchFinallyStatement);
                    return;
                }
                IUsingStatement usingStatement = v.Statement as IUsingStatement;
                if (usingStatement != null)
                {
                    this.VisitUsingStatement(usingStatement);
                    return;
                }
                VisitStatement(v.Statement);
            }

            public void VisitBlockStatement(IBlockStatement statement)
            {
                this.SetLabel("{...}");
                this.SetFillColor(statement, defaultColor);
            }

            public void VisitExpressionStatement(IExpressionStatement statement)
            {
                this.SetLabel(this.FormatStatement(statement));
                this.SetFillColor(statement, defaultColor);
            }

            public void VisitConditionStatement(IConditionStatement statement)
            {
                string fe = this.FormatExpression(statement.Condition);
                if (fe.StartsWith("("))
                    this.SetLabel("if{0}", fe);
                else
                    this.SetLabel("if({0})", fe);
                this.SetFillColor(statement, bifurcateColor);
            }

            public void VisitContinueStatement(IContinueStatement statement)
            {
                this.SetLabel("continue;");
                this.SetFillColor(statement, jumpColor);
            }
            public void VisitBreakStatement(IBreakStatement statement)
            {
                this.SetLabel("break;");
                this.SetFillColor(statement, jumpColor);
            }

            public void VisitStatement(IStatement statement)
            {
                this.SetLabel(FormatStatement(statement));
                this.SetFillColor(statement, defaultColor);
            }

            public void VisitForStatement(IForStatement statement)
            {
                this.SetLabel("for");
                this.SetFillColor(statement, bifurcateColor);
            }
            public void VisitForEachStatement(IForEachStatement statement)
            {
                this.SetLabel("foreach");
                this.SetFillColor(statement, bifurcateColor);
            }
            public void VisitWhileStatement(IWhileStatement statement)
            {
                string fe = this.FormatExpression(statement.Condition);
                if (fe.StartsWith("("))
                    this.SetLabel("while{0}", fe);
                else
                    this.SetLabel("while({0})", fe);
                this.SetFillColor(statement, bifurcateColor);
            }
            public void VisitUsingStatement(IUsingStatement statement)
            {
                string label = string.Format("using({0})", this.FormatExpression(statement.Expression));
                this.node.Attr.Label = label;

                this.SetFillColor(statement, bifurcateColor);
            }
            public void VisitMethodReturnStatement(IMethodReturnStatement statement)
            {
                this.VisitStatement(statement);
                this.SetFillColor(statement, returnColor);
            }
            public void VisitTryCatchFinallyStatement(ITryCatchFinallyStatement statement)
            {
                this.SetLabel("try");
                this.SetFillColor(statement, bifurcateColor);
            }

            private string FormatExpression(IExpression expression)
            {
                StatementGraphFormatter formatter = new StatementGraphFormatter();
                ILanguageWriter languageWriter = language.GetWriter(formatter, null);
                languageWriter.WriteExpression(expression);
                return formatter.ToString();
            }
            private string FormatStatement(IStatement statement)
            {
                StatementGraphFormatter formatter = new StatementGraphFormatter();
                ILanguageWriter languageWriter = language.GetWriter(formatter,null);
                languageWriter.WriteStatement(statement);
                return formatter.ToString();
            }
        }
    }
}
