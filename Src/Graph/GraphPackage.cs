using System;

namespace Reflector.Graph
{
	using Reflector.Graph;
    using System.Windows.Forms;
    using System.Collections;

	public class GraphPackage : IPackage
	{
		private IWindowManager windowManager;
		private ICommandBarManager commandBarManager;
		private ArrayList commands = new ArrayList();

		public void Load(IServiceProvider serviceProvider)
		{
			this.windowManager = (IWindowManager)serviceProvider.GetService(typeof(IWindowManager));
			this.commandBarManager = (ICommandBarManager)serviceProvider.GetService(typeof(ICommandBarManager));

			UserControl ilGraph = new IlGraphControl(serviceProvider);
			UserControl assemblyGraph = new AssemblyGraphControl(serviceProvider);
			UserControl classDiagram = new ClassDiagramControl(serviceProvider);

			this.windowManager.Windows.Add("Graph.ILGraph", ilGraph, "Peli's IL Graph");
			this.windowManager.Windows.Add("Graph.AssemblyGraph", assemblyGraph, "Peli's Assembly Graph");
			this.windowManager.Windows.Add("Graph.ClassDiagram", classDiagram, "Peli's Class Diagram");

			this.AddCommand("Browser.MethodDeclaration", "Peli's IL Graph", new EventHandler(this.ILGraph_Click));
			this.AddCommand("Tools", "Peli's Assembly Graph", new EventHandler(this.AssemblyGraph_Click));
			this.AddCommand("Browser.Assembly", "Peli's Assembly Graph", new EventHandler(this.AssemblyGraph_Click));
			this.AddCommand("Browser.TypeDeclaration", "Peli's Class Diagram", new EventHandler(this.ClassDiagram_Click));
		}

		public void Unload()
		{
			this.windowManager.Windows.Remove("Graph.ILGraph");
			this.windowManager.Windows.Remove("Graph.AssemblyGraph");
			this.windowManager.Windows.Remove("Graph.ClassDiagram");

			foreach (Command command in this.commands)
			{
				ICommandBar commandBar = this.commandBarManager.CommandBars[command.CommandBar];
				commandBar.Items.Remove(command.Button);
				commandBar.Items.Remove(command.Separator);
			}
		}

		private void AddCommand(string identifier, string text, EventHandler eventHandler)
		{
			ICommandBar commandBar = this.commandBarManager.CommandBars[identifier];

			Command command = new Command();
			command.CommandBar = identifier;
			command.Separator = commandBar.Items.AddSeparator();
			command.Button = commandBar.Items.AddButton(text, eventHandler);
			this.commands.Add(command);
		}

		private void ILGraph_Click(object sender, EventArgs e)
		{
			this.windowManager.Windows["Graph.ILGraph"].Visible = true;
		}

		private void AssemblyGraph_Click(object sender, EventArgs e)
		{
			this.windowManager.Windows["Graph.AssemblyGraph"].Visible = true;
		}

		private void ClassDiagram_Click(object sender, EventArgs e)
		{
			this.windowManager.Windows["Graph.ClassDiagram"].Visible = true;
		}

		private struct Command
		{
			public string CommandBar;
			public ICommandBarSeparator Separator;
			public ICommandBarButton Button;
		}
    }

    public sealed class FlowToCodeConverter
    {
        private Hashtable codeFlows = new Hashtable();
        public FlowToCodeConverter()
        {
            foreach (System.Reflection.FieldInfo fi in
                typeof(System.Reflection.Emit.OpCodes).GetFields(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Static
                )
                )
            {
                System.Reflection.Emit.OpCode code = (System.Reflection.Emit.OpCode)fi.GetValue(null);
                this.codeFlows[(int)code.Value] = code.FlowControl;
            }
        }
        public System.Reflection.Emit.FlowControl Convert(int code)
        {
            Object o = this.codeFlows[code];
            if (o == null)
                return System.Reflection.Emit.FlowControl.Meta;
            //				throw new Exception(String.Format("code.Value {0} not found",code.Value));
            return (System.Reflection.Emit.FlowControl)o;
        }
    }
}

