namespace LiveSequence.Common.Graphics
{
  using System.Windows;

  /// <summary>
  /// Container object that provides easy access to the combination of DiagramRow, DiagramGroup and DiagramNode.
  /// </summary>
  internal sealed class DiagramConnectorNode
  {
    /// <summary>Contains a reference to the connector's row.</summary>
    private DiagramRow row;

    /// <summary>Contains a reference to the connector's group.</summary>
    private DiagramGroup group;

    /// <summary>Contains a reference to the connector's node.</summary>
    private DiagramNode node;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiagramConnectorNode"/> class.
    /// </summary>
    /// <param name="node">The diagram node.</param>
    /// <param name="group">The diagram group.</param>
    /// <param name="row">The diagram row.</param>
    internal DiagramConnectorNode(DiagramNode node, DiagramGroup group, DiagramRow row)
    {
      this.node = node;
      this.group = group;
      this.row = row;
    }

    /// <summary>
    /// Gets the node for this connection point.
    /// </summary>
    /// <value>The diagram node.</value>
    internal DiagramNode Node
    {
      get
      {
        return this.node;
      }
    }

    /// <summary>
    /// Gets the center of the node relative to the diagram.
    /// </summary>
    /// <value>The center point.</value>
    internal Point Center
    {
      get
      {
        return this.GetPoint(this.node.Center);
      }
    }

    /// <summary>
    /// Gets the top left of the node relative to the diagram.
    /// </summary>
    /// <value>The top left point.</value>
    internal Point TopLeft
    {
      get
      {
        return this.GetPoint(this.node.TopLeft);
      }
    }

    /// <summary>
    /// Gets the top right of the node relative to the diagram.
    /// </summary>
    /// <value>The top right point.</value>
    internal Point TopRight
    {
      get
      {
        return this.GetPoint(this.node.TopRight);
      }
    }

    /// <summary>
    /// Return the point shifted by the row and group location.
    /// </summary>
    /// <param name="point">The original point.</param>
    /// <returns>
    /// The point shifted by row and group location.
    /// </returns>
    private Point GetPoint(Point point)
    {
      point.Offset(
          this.row.Location.X + this.group.Location.X,
          this.row.Location.Y + this.group.Location.Y);

      return point;
    }
  }
}
