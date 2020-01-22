namespace Reflector.Sequence
{
  using System;
  using System.Collections.Generic;
  using System.Windows.Forms;
  using LiveSequence.Common;
  using Reflector;

  /// <summary>
  /// Implements the IPackage interface from Reflector to make this a plugable add-in.
  /// </summary>
  public class OpenSequencePackage : IPackage
  {
    /// <summary>
    /// Contains a reference to Reflector's Window Manager.
    /// </summary>
    private IWindowManager windowManager;

    /// <summary>
    /// Contains a reference to Reflector's command bar manager.
    /// </summary>
    private ICommandBarManager commandBarManager;

    /// <summary>
    /// Contains a reference to the list of commands, supported by this add-in.
    /// </summary>
    private List<Command> commands = new List<Command>();

    #region IPackage Members

    /// <summary>
    /// Loads the specified service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public void Load(IServiceProvider serviceProvider)
    {
      this.windowManager = serviceProvider.GetService(typeof(IWindowManager)) as IWindowManager;
      this.commandBarManager = serviceProvider.GetService(typeof(ICommandBarManager)) as ICommandBarManager;

      UserControl graphControl = new DiagramViewerContainer(serviceProvider);

      this.windowManager.Windows.Add("OpenSequence", graphControl, "Sequence Diagram");

      this.AddCommand("Browser.MethodDeclaration", "Sequence Diagram", new EventHandler(this.OnOpenSequenceClick));

      Logger.Current.Info("Package loaded.");
    }

    /// <summary>
    /// Unloads this instance.
    /// </summary>
    public void Unload()
    {
        this.windowManager.Windows.Remove("OpenSequence");

      foreach (Command command in this.commands)
      {
        ICommandBar commandBar = this.commandBarManager.CommandBars[command.CommandBar];
        commandBar.Items.Remove(command.Button);
        commandBar.Items.Remove(command.Separator);
      }
    }

    #endregion

    /// <summary>
    /// Handles the click event of the added command for the Sequence diagram.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void OnOpenSequenceClick(object sender, EventArgs e)
    {
      this.windowManager.Windows["OpenSequence"].Visible = true;
    }

    /// <summary>
    /// Adds the command.
    /// </summary>
    /// <param name="identifier">The identifier.</param>
    /// <param name="text">The text to appear as the command's text.</param>
    /// <param name="eventHandler">The event handler.</param>
    private void AddCommand(string identifier, string text, EventHandler eventHandler)
    {
      ICommandBar commandBar = this.commandBarManager.CommandBars[identifier];

      Command command = new Command();
      command.CommandBar = identifier;
      command.Separator = commandBar.Items.AddSeparator();
      command.Button = commandBar.Items.AddButton(text, eventHandler);
      this.commands.Add(command);
    }

    /// <summary>
    /// Command structure to be used to add commands to Reflector's command bar.
    /// </summary>
    private struct Command
    {
      /// <summary>
      /// Contains the name of the command bar.
      /// </summary>
      public string CommandBar;

      /// <summary>
      /// Contains a reference to the separator that may have been added
      /// </summary>
      public ICommandBarSeparator Separator;

      /// <summary>
      /// Contains a reference to the command bar button.
      /// </summary>
      public ICommandBarButton Button;
    }
  }
}
