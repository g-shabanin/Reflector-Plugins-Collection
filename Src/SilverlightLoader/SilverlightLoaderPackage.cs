namespace Reflector.SilverlightLoader
{
	using System;
	using System.Drawing;
	using System.Windows.Forms;
	using Reflector;
	using Reflector.CodeModel;

	public class SilverlightLoaderPackage : IPackage
	{
		private IConfigurationManager configurationManager;
		private IAssemblyManager assemblyManager;
		private IWindowManager windowManager;
		private ICommandBarManager commandBarManager;
		private ICommandBarButton button;

		public void Load(IServiceProvider serviceProvider)
		{
			this.configurationManager = (IConfigurationManager)serviceProvider.GetService(typeof(IConfigurationManager));
			this.assemblyManager = (IAssemblyManager)serviceProvider.GetService(typeof(IAssemblyManager));
			this.windowManager = (IWindowManager)serviceProvider.GetService(typeof(IWindowManager));
			this.commandBarManager = (ICommandBarManager) serviceProvider.GetService(typeof(ICommandBarManager));

			this.button = this.commandBarManager.CommandBars["File"].Items.InsertButton(2, "Open &Silverlight...", new EventHandler(this.Button_Click), Keys.Q | Keys.Control);
		}

		public void Unload()
		{
			this.commandBarManager.CommandBars["File"].Items.Remove(this.button);
		}

		private void Button_Click(object sender, EventArgs e)
		{
			SilverlightLoaderDialog dialog = new SilverlightLoaderDialog(this.configurationManager, this.assemblyManager);
			dialog.ShowDialog(this.windowManager as IWin32Window);
		}
	}
}
