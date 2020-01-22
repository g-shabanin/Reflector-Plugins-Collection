namespace LiveSequence.Common.Graphics
{
  using System.Windows.Media;

  /// <summary>
  /// This implementation of the abstract DiagramConnector class draws a connector for the inheritance.
  /// </summary>
  internal sealed class DiagramInheritanceConnector : DiagramConnector
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="DiagramInheritanceConnector"/> class.
    /// </summary>
    /// <param name="startConnector">The start connector.</param>
    /// <param name="endConnector">The end connector.</param>
    /// <remarks>
    /// Consturctor that specifies the two nodes that are connected.
    /// </remarks>
    internal DiagramInheritanceConnector(DiagramConnectorNode startConnector, DiagramConnectorNode endConnector)
      : base(startConnector, endConnector)
    {
      BrushConverter bc = new BrushConverter();
      Brush brush = bc.ConvertFromString("#716F64") as Brush;
      this.ResourcePen = new Pen(brush != null ? brush : Brushes.DimGray, 1);
    }

    /// <summary>
    /// Return true if should continue drawing, otherwise false.
    /// </summary>
    /// <param name="drawingContext">The drawing context.</param>
    /// <returns>
    /// True when the drawing can continu, false otherwise.
    /// </returns>
    public override bool Draw(DrawingContext drawingContext)
    {
      bool validBaseDraw = base.Draw(drawingContext);
      if (validBaseDraw)
      {
        ////if (this.StartNode.Node.ObjectInfo == this.EndNode.Node.ObjectInfo)
        ////{
        ////  this.DrawCurvedConnector(drawingContext);
        ////}
        ////else if (this.StartNode.Center.X < this.EndNode.Center.X)
        ////{
        ////  this.DrawStraightConnector(drawingContext);
        ////}
        ////else
        ////{
        ////  this.DrawReversedConnector(drawingContext);
        ////}
      }

      return validBaseDraw;
    }
  }
}
