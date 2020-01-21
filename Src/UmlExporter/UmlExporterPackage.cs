namespace Reflector.UmlExporter
{
	using System;
	using System.Text;
	using System.Windows.Forms;
	using System.Xml.Serialization;
	using System.Xml;
	using Reflector;
	using Reflector.CodeModel;

	public class UmlExporterPackage : IPackage
	{
		private IAssemblyBrowser assemblyBrowser;
		private ICommandBarManager commandBarManager = null;
		private ICommandBarSeparator separator = null;
		private ICommandBarButton button = null;
		
		public void Load(IServiceProvider serviceProvider)
		{
			this.assemblyBrowser = (IAssemblyBrowser)serviceProvider.GetService(typeof(IAssemblyBrowser));
			this.commandBarManager = (ICommandBarManager)serviceProvider.GetService(typeof(ICommandBarManager));            
			
			this.separator = commandBarManager.CommandBars["Browser.Assembly"].Items.AddSeparator();
			this.button = commandBarManager.CommandBars["Browser.Assembly"].Items.AddButton("Expor&t to XMI...", new EventHandler(this.Button_Click));
		}
		
		public void Unload()
		{
			this.commandBarManager.CommandBars["Browser.Assembly"].Items.Remove(this.button);
			this.commandBarManager.CommandBars["Browser.Assembly"].Items.Remove(this.separator);
		}
		
		private void Button_Click(object sender, EventArgs e)
		{
			using (SaveFileDialog dialog = new SaveFileDialog())
			{
				dialog.AddExtension = true;
				dialog.Filter = "XMI File (*.xmi)|*.xmi|All Files (*.*)|*.*";
				dialog.DefaultExt = ".xmi";
				if (dialog.ShowDialog() == DialogResult.OK)
				{
					SaveTo(dialog.FileName);
				}
			}
		}
		
		private void SaveTo(string fileName)
		{
			IAssembly assembly = assemblyBrowser.ActiveItem as IAssembly;
			if (assembly != null)
			{
				XmiDocument document = new XmiDocument();
				document.Content.Model = new UmlModel();
				document.Content.Model.Name = "UML Model";
				UmlModelTranslator.Translate(assembly, document.Content.Model);

				XmlTextWriter writer = new XmlTextWriter(fileName, Encoding.UTF8);
				writer.IndentChar = '\t';
				writer.Formatting = Formatting.Indented;

				XmlSerializer serializer = new XmlSerializer(typeof(XmiDocument));
				serializer.Serialize(writer, document);

				writer.Close();
			}
		}
	}
}
