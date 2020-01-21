namespace Reflector.Graph
{
	using System.Collections;
	using System.Collections.Specialized;
	using System.Drawing;
	using System.IO;
	using System.Windows.Forms;
	using System;
	using System.ComponentModel;
	using QuickGraph;
	using QuickGraph.Concepts;
	using QuickGraph.Providers;
	using QuickGraph.Representations;
	using Reflector.CodeModel;
	using System.Reflection;
	using Reflector.Graph.Graphs;

	internal sealed class IlGraphControl : GraphControl
    {
        private FlowToCodeConverter flowConverter = new FlowToCodeConverter();
        private InstructionGraphPopulator populator = new InstructionGraphPopulator();
        private InstructionGraph graph = null;

		private IAssemblyBrowser assemblyBrowser;

		public IlGraphControl(IServiceProvider serviceProvider) : base(serviceProvider)
		{
			this.assemblyBrowser = (IAssemblyBrowser)serviceProvider.GetService(typeof(IAssemblyBrowser));
		}

		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);

			if (this.Parent != null)
			{
				this.assemblyBrowser.ActiveItemChanged += new EventHandler(assemblyBrowser_ActiveItemChanged);
				this.Translate();
			}
			else
			{
				this.assemblyBrowser.ActiveItemChanged -= new EventHandler(assemblyBrowser_ActiveItemChanged);
			}
		}

        private void assemblyBrowser_ActiveItemChanged(object sender, EventArgs e)
        {
			if (this.Parent != null)
			{
				this.Translate();
			}
        }

        void CodeCoverageManager_ActiveSourceChanged(object sender, EventArgs e)
        {
			if (this.Parent != null)
			{
				this.Translate();
			}
        }

        private void Translate()
        {
            IMethodDeclaration method = this.assemblyBrowser.ActiveItem as IMethodDeclaration;
			if (method != null)
			{
				IMethodBody body = method.Body as IMethodBody;
				if (body != null)
				{
					this.graph = this.populator.BuildGraphFromMethod(method);
					InstructionGraphRenderer render = new InstructionGraphRenderer(graph);
					this.Viewer.Graph = render.Render();
				}
			}
        }
    }
}
