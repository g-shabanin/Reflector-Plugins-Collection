namespace Reflector.SilverlightBrowser
{
	using System;
	using System.Collections.Generic;
    using System.IO;
    using System.Windows.Forms;
	using Reflector;
	using Reflector.CodeModel;

	public class SilverlightBrowserPackage : IPackage
    {
        private IWindowManager windowManager;
		private ICommandBarManager commandBarManager;
		private ICommandBarButton button;
		private ICommandBarItem seperator;

		public void Load(IServiceProvider serviceProvider)
        {
			IAssemblyManager assemblyManager = (IAssemblyManager)serviceProvider.GetService(typeof(IAssemblyManager));
			ILanguageManager languageManager = (ILanguageManager)serviceProvider.GetService(typeof(ILanguageManager));
			ITranslatorManager translatorManager = (ITranslatorManager)serviceProvider.GetService(typeof(ITranslatorManager));

			this.windowManager = (IWindowManager)serviceProvider.GetService(typeof(IWindowManager));
			this.commandBarManager = (ICommandBarManager)serviceProvider.GetService(typeof(ICommandBarManager));

			Viewer viewer = new Viewer(assemblyManager, languageManager, translatorManager, this.windowManager);

			this.windowManager.Windows.Add("Reflector.SilverlightBrowser", viewer, "Silverlight Browser");

			this.seperator = this.commandBarManager.CommandBars["Tools"].Items.AddSeparator();
			this.button = this.commandBarManager.CommandBars["Tools"].Items.AddButton("Browse &Silverlight Page", new EventHandler(this.Button_Click), Keys.U | Keys.Control);
        }

		public void Unload()
		{
			this.commandBarManager.CommandBars["Tools"].Items.Remove(this.seperator);
			this.commandBarManager.CommandBars["Tools"].Items.Remove(this.button);
			this.windowManager.Windows.Remove("Reflector.SilverlightBrowser");
		}

        private void Button_Click(object sender, EventArgs e)
        {
            try
            {
				this.windowManager.Windows["Reflector.SilverlightBrowser"].Visible = true;
            }
            catch (Exception exception)
            {
				MessageBox.Show(exception.Message, "Silverlight Browser Add-In", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}