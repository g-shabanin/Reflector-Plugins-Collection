namespace Reflector.Graph
{
	using System.Collections;
	using System.Collections.Specialized;
	using System.Drawing;
	using System.IO;
	using System.Reflection;
	using System.Windows.Forms;
	using System;
	using System.ComponentModel;
	using QuickGraph;
	using QuickGraph.Concepts;
	using QuickGraph.Providers;
	using QuickGraph.Representations;
	using Reflector.CodeModel;

	using Reflector.Graph.Graphs;

	internal sealed class StatementGraphControl : GraphControl
    {
        private StatementGraphPopulator populator = null;
        private StatementGraph graph = null;

		private IAssemblyManager assemblyManager;
		private IAssemblyBrowser assemblyBrowser;
		private ILanguageManager languageManager;
		private ITranslatorManager translatorManager;

        public StatementGraphControl(IServiceProvider serviceProvider) : base(serviceProvider)
        {
			this.assemblyManager = (IAssemblyManager)serviceProvider.GetService(typeof(IAssemblyManager));
			this.assemblyBrowser = (IAssemblyBrowser)serviceProvider.GetService(typeof(IAssemblyBrowser));
			this.languageManager = (ILanguageManager)serviceProvider.GetService(typeof(ILanguageManager));
			this.translatorManager = (ITranslatorManager)serviceProvider.GetService(typeof(ITranslatorManager));
		}

        public QuickGraph.Concepts.Traversals.IVertexListGraph Graph
        {
            get { return this.graph; }
        }

		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);

			if (this.Parent != null)
			{
				this.populator = new StatementGraphPopulator(this.translatorManager);
				this.assemblyBrowser.ActiveItemChanged += new EventHandler(assemblyBrowser_ActiveItemChanged);
				this.languageManager.ActiveLanguageChanged += new EventHandler(LanguageManager_ActiveLanguageChanged);
			}
			else
			{
				this.populator = null;
				this.assemblyBrowser.ActiveItemChanged -= new EventHandler(assemblyBrowser_ActiveItemChanged);
				this.languageManager.ActiveLanguageChanged -= new EventHandler(LanguageManager_ActiveLanguageChanged);
			}
		}

        void LanguageManager_ActiveLanguageChanged(object sender, EventArgs e)
        {
            if (this.Parent == null)
                return;

            this.Translate();
        }

        private void assemblyBrowser_ActiveItemChanged(object sender, EventArgs e)
        {
            if (this.Parent == null)
                return;

            this.Translate();
        }

        private void Translate()
        {
			IMethodDeclaration method = this.assemblyBrowser.ActiveItem as IMethodDeclaration;
            if (method == null)
                return;

            this.graph = this.populator.BuildGraphFromMethod(method);
            StatementGraphRenderer render = new StatementGraphRenderer(this.graph, this.languageManager);
            this.Viewer.Graph = render.Render(this.CreateGraph(method.Name));
        }

        //protected override void OnVertexClick(VertexEventArgs e)
        //{
        //    base.OnVertexClick(e);

        //    StatementToActiveItemVisitor vis = new StatementToActiveItemVisitor();
        //    vis.VisitStatement(((StatementVertex)e.Vertex).Statement);
        //    this.Services.AssemblyBrowser.ActiveItem = vis.ActiveItem;
        //}

        private class StatementToActiveItemVisitor : Visitor
        {
            private object activeItem = null;

            public object ActiveItem
            {
                get
                {
                    return this.activeItem;
                }
                set
                {
                    this.activeItem = value;
                }
            }

            public override void VisitMethodInvokeExpression(IMethodInvokeExpression expression)
            {
                IMethodReferenceExpression methodReferenceExpression = expression.Method as IMethodReferenceExpression;
                this.activeItem = methodReferenceExpression.Method;
            }

            public override void VisitCastExpression(ICastExpression expression)
            {
                this.activeItem = expression.TargetType;
            }

            public override void VisitDelegateCreateExpression(IDelegateCreateExpression expression)
            {
                this.activeItem = expression.Method;
            }

            public override void VisitFieldReferenceExpression(IFieldReferenceExpression expression)
            {
                this.activeItem = expression.Field;
            }
        }
    }
}
