namespace LiveSequence.Common.Graphics
{
  using System.Windows;
  using System.Windows.Controls;
  using LiveSequence.Common.Domain;

  /// <summary>
  /// The DiagramNode is the abstract base entity of the diagrams.
  /// </summary>
  /// <remarks>
  /// Currently it supports representing an ObjectInfo or a MessageInfo node through the DiagramSequenceNode;
  /// and through the DiagramModelNode it supports representing a class diagram.
  /// </remarks>
  public abstract class DiagramNode : ContentControl
  {
    /// <summary>
    /// Dependency property for the NodeLabel property.
    /// </summary>
    public static readonly DependencyProperty NodeLabelProperty = DependencyProperty.Register("NodeLabel", typeof(string), typeof(DiagramSequenceNode));

    /// <summary>Contains a reference to the object info instance.</summary>
    private ObjectInfo objectInfo;

    /// <summary>Contains a reference to the message info instance.</summary>
    private MessageInfo messageInfo;

    /// <summary>Contains a reference to the location object.</summary>
    private Point location = new Point();

    /// <summary>Contains a reference to the node type</summary>
    private NodeType nodeType = NodeType.MessageInfo;

    /// <summary>
    /// Gets or sets the text displayed within the node.
    /// </summary>
    /// <value>The node label.</value>
    public string NodeLabel
    {
      get
      {
        return (string)this.GetValue(DiagramSequenceNode.NodeLabelProperty);
      }

      set
      {
        SetValue(DiagramSequenceNode.NodeLabelProperty, value);
      }
    }

    /// <summary>
    /// Gets or sets the location of the node relative to the parent group.
    /// </summary>
    /// <value>The location point.</value>
    internal Point Location
    {
      get
      {
        return this.location;
      }

      set
      {
        this.location = value;
      }
    }

    /// <summary>
    /// Gets the center of the node.
    /// </summary>
    /// <value>The center point.</value>
    internal Point Center
    {
      get
      {
        return new Point(
            this.location.X + (DesiredSize.Width / 2),
            this.location.Y + (DesiredSize.Height / 2));
      }
    }

    /// <summary>
    /// Gets the top left of the node.
    /// </summary>
    internal Point TopLeft
    {
      get
      {
        return new Point(this.location.X, this.location.Y);
      }
    }

    /// <summary>
    /// Gets the top right of the node.
    /// </summary>
    internal Point TopRight
    {
      get
      {
        return new Point(this.location.X + DesiredSize.Width, this.location.Y);
      }
    }

    /// <summary>
    /// Gets or sets the type of node.
    /// </summary>
    /// <value>The type of the node.</value>
    internal NodeType NodeType
    {
      get
      {
        return this.nodeType;
      }

      set
      {
        this.nodeType = value;
        this.UpdateTemplate(this.nodeType);
        this.UpdateNodeLayout(this.nodeType);
      }
    }

    /// <summary>
    /// Gets or sets the object info.
    /// </summary>
    /// <value>The object info.</value>
    internal ObjectInfo ObjectInfo
    {
      get
      {
        return this.objectInfo;
      }

      set
      {
        this.objectInfo = value;
        this.DataContext = this;

        this.UpdateNodeLabel();
      }
    }

    /// <summary>
    /// Gets or sets the message info.
    /// </summary>
    /// <value>The message info.</value>
    internal MessageInfo MessageInfo
    {
      get
      {
        return this.messageInfo;
      }

      set
      {
        this.messageInfo = value;
        this.DataContext = this;

        this.UpdateNodeLabel();
      }
    }

    /// <summary>
    /// Update the node template based on the node type.
    /// </summary>
    /// <param name="nodeType">Type of the node.</param>
    internal abstract void UpdateTemplate(NodeType nodeType);

    /// <summary>
    /// Updates the node layout.
    /// </summary>
    /// <param name="nodeType">Type of the node.</param>
    internal virtual void UpdateNodeLayout(NodeType nodeType)
    {
      // no default implementation...
    }

    /// <summary>
    /// Updates the node label.
    /// </summary>
    /// <param name="info">The info that is to be displayed.</param>
    protected virtual void UpdateNodeLabel(string info)
    {
      this.NodeLabel = info;
    }

    /// <summary>
    /// Updates the node label.
    /// </summary>
    private void UpdateNodeLabel()
    {
      if (this.messageInfo != null)
      {
        this.UpdateNodeLabel(this.messageInfo.ToString());
      }
      else if (this.objectInfo != null)
      {
        this.UpdateNodeLabel(this.objectInfo.ToString());
      }
    }
  }
}