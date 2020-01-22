namespace LiveSequence.Common.Graphics
{
  using System;
  using System.Globalization;
  using System.Windows.Controls;
  using LiveSequence.Common.Context;

  /// <summary>
  /// The DiagramSequenceNode is the base entity of the sequence diagram, and can represent a ObjectInfo, or a MessageInfo node.
  /// </summary>
  public sealed class DiagramSequenceNode : DiagramNode
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="DiagramSequenceNode"/> class.
    /// </summary>
    public DiagramSequenceNode()
    {
      this.InitializeContextMenu();
    }

    /// <summary>
    /// Update the node template based on the node type.
    /// </summary>
    /// <param name="nodeType">Type of the node.</param>
    internal override void UpdateTemplate(NodeType nodeType)
    {
      if (nodeType != NodeType.TypeInfo && nodeType != NodeType.MessageInfo)
      {
        throw new ArgumentException(Properties.Resources.SequenceNodeAllowsOnlyTypeOrMessageInfo, "nodeType");
      }

      // Determine the node template based on node properties.
      string templateName = nodeType == NodeType.TypeInfo ? "Primary" : "Message";

      string template = string.Format(
          CultureInfo.InvariantCulture,
          "{0}NodeTemplate",
          templateName);

      // Assign the node template.
      this.Template = FindResource(template) as ControlTemplate;
    }

    /// <summary>
    /// Invoked when an unhandled <see cref="E:System.Windows.UIElement.MouseRightButtonUp"/> routed event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
    /// </summary>
    /// <param name="e">The <see cref="T:System.Windows.Input.MouseButtonEventArgs"/> that contains the event data. The event data reports that the right mouse button was released.</param>
    protected override void OnMouseRightButtonUp(System.Windows.Input.MouseButtonEventArgs e)
    {
      base.OnMouseRightButtonUp(e);

      if (this.NodeType == NodeType.TypeInfo && this.ObjectInfo.Key != DiagramContext.DiagramObjects[0].Key)
      {
        // handle right click by showing context menu
        this.ContextMenu.IsOpen = true;

        e.Handled = true;
      }
      else
      {
        this.ContextMenu.IsOpen = false;
        e.Handled = true;
      }
    }

    /// <summary>
    /// Initializes the context menu.
    /// </summary>
    private void InitializeContextMenu()
    {
      this.ContextMenu = new ContextMenu();
      this.ContextMenu.IsEnabled = true;
      this.ContextMenu.Opacity = 0.85;
      MenuItem mi = new MenuItem();
      mi.Command = DiagramCommands.FilterOutActor;
      mi.IsEnabled = true;
      mi.CommandParameter = this;
      this.ContextMenu.Items.Add(mi);
      mi = new MenuItem();
      mi.Command = DiagramCommands.FilterOutCalls;
      mi.IsEnabled = true;
      mi.CommandParameter = this;
      this.ContextMenu.Items.Add(mi);
    }
  }
}