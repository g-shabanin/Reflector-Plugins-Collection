namespace Reflector.Review
{
	using System;
	using System.Windows.Forms;

	public sealed class ReviewPackage : IPackage
    {
		private IWindowManager windowManager = null;
		private ICommandBarManager commandBarManager = null;
		private ICommandBarSeparator separator = null;
		private ICommandBarButton button = null;

		public void Load(IServiceProvider serviceProvider)
		{
			this.windowManager = (IWindowManager)serviceProvider.GetService(typeof(IWindowManager));
			this.commandBarManager = (ICommandBarManager)serviceProvider.GetService(typeof(ICommandBarManager));

			ReviewControl reviewControl = new ReviewControl(serviceProvider);
			this.windowManager.Windows.Add("ReviewWindow", reviewControl, "Peli's Review");

			this.separator = commandBarManager.CommandBars["Tools"].Items.AddSeparator();
			this.button = commandBarManager.CommandBars["Tools"].Items.AddButton("Peli's Review", new EventHandler(this.Button_Click), Keys.Control | Keys.W);
		}

		public void Unload()
		{
			this.windowManager.Windows.Remove("ReviewWindow");

			this.commandBarManager.CommandBars["Tools"].Items.Remove(this.button);
			this.commandBarManager.CommandBars["Tools"].Items.Remove(this.separator);
		}

		private void Button_Click(object sender, EventArgs e)
		{
			this.windowManager.Windows["ReviewWindow"].Visible = true;

			ReviewControl reviewControl = (ReviewControl) this.windowManager.Windows["ReviewWindow"].Content;
			reviewControl.Activate();
		}
    }
}
