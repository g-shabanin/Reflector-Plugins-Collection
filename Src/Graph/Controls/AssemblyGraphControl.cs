namespace Reflector.Graph
{
	using System.Collections;
	using System.Collections.Specialized;
	using System.Drawing;
	using System.IO;
	using System.Windows.Forms;
	using System;
	using System.ComponentModel;
	using Reflector.CodeModel;
	using System.Reflection;
	using Microsoft.Glee.Drawing;

	internal sealed class AssemblyGraphControl : GraphControl
    {
        private StringCollection excludedAssemblies = new StringCollection();
		private IAssemblyBrowser assemblyBrowser;
		private IAssemblyManager assemblyManager;

        public AssemblyGraphControl(IServiceProvider serviceProvider) : base(serviceProvider)
        {
			this.assemblyBrowser = (IAssemblyBrowser)serviceProvider.GetService(typeof(IAssemblyBrowser));
			this.assemblyManager = (IAssemblyManager)serviceProvider.GetService(typeof(IAssemblyManager));

            this.Dock = DockStyle.Fill;
            this.excludedAssemblies.Add("mscorlib");
			this.excludedAssemblies.Add("system");
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
            if (this.Parent == null)
                return;

            this.Translate();
        }

        private void Translate()
        {
            Microsoft.Glee.Drawing.Graph graph = this.CreateGraph("Assembly Dependency Graph");

            // creating assembly vertices
            for (int i = 0; i < this.assemblyManager.Assemblies.Count; ++i)
            {
                IAssembly assembly = this.assemblyManager.Assemblies[i];
                Node vertex = (Node)graph.AddNode(assembly.ToString());
                vertex.UserData = assembly;
                FormatVertex(vertex);
            }

            /*
            // adding referenced assemblies    
            foreach (IAssembly assembly in this.Services.AssemblyManager.Assemblies)
            {
                foreach (IModule module in assembly.Modules)
                {
                    foreach (IAssemblyReference assemblyName in module.AssemblyReferences)
                    {
                        if (this.excludedAssemblies.Contains(assemblyName.Name.ToLower()))
                            continue;
                        if (assemblyVertices.Contains(assemblyName.ToString()))
                            continue;
                        CustomVertex v = (CustomVertex)graph.AddVertex();
                        v.Value = assemblyName;
                        assemblyVertices.Add(assemblyName.ToString(), v);
                    }
                }
            }
            */

            /// creating edges
			for (int i = 0; i < this.assemblyManager.Assemblies.Count; ++i)
            {
                IAssembly assembly = this.assemblyManager.Assemblies[i];
                Node vertex1 = graph.FindNode(assembly.ToString()) as Node;
                if (vertex1 == null)
                    continue;

                foreach (IModule module in assembly.Modules)
                {
                    foreach (IAssemblyReference assemblyName in module.AssemblyReferences)
                    {
                        Node vertex2 = graph.FindNode(assemblyName.ToString()) as Node;
                        if (vertex2 == null)
                            continue;

                        Edge edge = (Edge)graph.AddEdge(
                            vertex1.Id, 
                            vertex2.Id);
                    }
                }
            }

            this.Viewer.Graph = graph;
        }

        private void FormatVertex(
            Microsoft.Glee.Drawing.Node vertex
            )
        {
            IAssemblyReference assembly = vertex.UserData as IAssemblyReference;

            // add coverage if available
            string label = String.Format("{0}\\n{1}", assembly.Name, assembly.Version);

            // assembly shape
            bool isAssembly = assembly is IAssembly;
            if (isAssembly)
                vertex.Attr.Shape = Microsoft.Glee.Drawing.Shape.Box;
            else
                vertex.Attr.Shape = Microsoft.Glee.Drawing.Shape.Ellipse;

            // assembly color
            if (isAssembly)
                vertex.Attr.Fillcolor = Microsoft.Glee.Drawing.Color.LightSkyBlue;
            else
                vertex.Attr.Fillcolor = Microsoft.Glee.Drawing.Color.LightGray;
            if (this.assemblyBrowser.ActiveItem == assembly)
                vertex.Attr.Fillcolor = Microsoft.Glee.Drawing.Color.LightGreen;

            vertex.Attr.Label = label;
            vertex.UserData = assembly;
        }
    }
}