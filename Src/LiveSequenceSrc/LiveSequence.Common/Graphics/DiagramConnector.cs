namespace LiveSequence.Common.Graphics
{
  using System.Windows;
  using System.Windows.Media;

  /// <summary>
  /// Abstract base class that can be used to draw connectors between two nodes.
  /// </summary>
  internal abstract class DiagramConnector
  {
    /// <summary>
    /// Contains a reference to the start node.
    /// </summary>
    private DiagramConnectorNode start;

    /// <summary>
    /// Contains a reference to the end node.
    /// </summary>
    private DiagramConnectorNode end;

    /// <summary>
    /// Contains a reference to the Pen to draw connector line.
    /// </summary>
    private Pen resourcePen;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiagramConnector"/> class.
    /// </summary>
    /// <param name="startConnector">The start connector.</param>
    /// <param name="endConnector">The end connector.</param>
    /// <remarks>
    /// Consturctor that specifies the two nodes that are connected.
    /// </remarks>
    protected DiagramConnector(DiagramConnectorNode startConnector, DiagramConnectorNode endConnector)
    {
      this.start = startConnector;
      this.end = endConnector;
    }

    /// <summary>
    /// Gets the starting node.
    /// </summary>
    protected DiagramConnectorNode StartNode
    {
      get
      {
        return this.start;
      }
    }

    /// <summary>
    /// Gets the ending node.
    /// </summary>
    protected DiagramConnectorNode EndNode
    {
      get
      {
        return this.end;
      }
    }

    /// <summary>
    /// Gets or sets the pen that specifies the connector line.
    /// </summary>
    protected Pen ResourcePen
    {
      get
      {
        return this.resourcePen;
      }

      set
      {
        this.resourcePen = value;
      }
    }

    /// <summary>
    /// Gets the Pen to use for drawing.
    /// </summary>
    /// <remarks>
    /// Create the connector line pen. The opacity is set based on
    /// the current filtered state. The pen contains an animation
    /// if the filtered state has changed.
    /// </remarks>
    protected Pen Pen
    {
      get
      {
        // Make a copy of the resource pen so it can 
        // be modified, the resource pen is frozen.
        Pen connectorPen = this.ResourcePen.Clone();

        // Set opacity based on the filtered state.
        connectorPen.Brush.Opacity = 1.0;

        return connectorPen;
      }
    }

    protected Pen DashedPen
    {
      get
      {
        // Make a copy of the resource pen so it can 
        // be modified, the resource pen is frozen.
        Pen dashedConnectorPen = this.ResourcePen.Clone();

        // Set opacity based on the filtered state.
        dashedConnectorPen.Brush.Opacity = 1.0;
        dashedConnectorPen.DashStyle = new DashStyle(new double[] { 2.3, 2.3 }, 2.3);

        return dashedConnectorPen;
      }
    }

    /// <summary>
    /// Return true if should continue drawing, otherwise false.
    /// </summary>
    /// <param name="drawingContext">The drawing context.</param>
    /// <returns>True when the drawing can continu, false otherwise.</returns>
    public virtual bool Draw(DrawingContext drawingContext)
    {
      // Don't draw if either one of the nodes is not set...
      if (this.start == null || this.start.Node == null ||
          this.end == null || this.end.Node == null)
      {
        return false;
      }

      // Don't draw if either of the nodes are filtered.
      if (this.start.Node.Visibility != Visibility.Visible ||
          this.end.Node.Visibility != Visibility.Visible)
      {
        return false;
      }

      return true;
    }
  }
}
