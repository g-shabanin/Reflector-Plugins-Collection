namespace LiveSequence.Common.Graphics
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Media;
  using LiveSequence.Common.Context;

  /// <summary>
  /// This implementation of the abstract DiagramConnector class draws a connector with the method name.
  /// </summary>
  internal sealed class DiagramCallConnector : DiagramConnector
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="DiagramCallConnector"/> class.
    /// </summary>
    /// <param name="startConnector">The start connector.</param>
    /// <param name="endConnector">The end connector.</param>
    /// <remarks>
    /// Consturctor that specifies the two nodes that are connected.
    /// </remarks>
    public DiagramCallConnector(DiagramConnectorNode startConnector, DiagramConnectorNode endConnector)
      : base(startConnector, endConnector)
    {
      this.ResourcePen = new Pen(Brushes.Black, 1);
    }

    /// <summary>
    /// Gets the ultimate right.
    /// </summary>
    /// <value>The ultimate right.</value>
    internal double UltimateRight
    {
      get
      {
        double width = this.MeasureConnector();
        return (4 * width) / 5;
      }
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
        if (this.StartNode.Node.ObjectInfo == this.EndNode.Node.ObjectInfo)
        {
          this.DrawCurvedConnector(drawingContext);
        }
        else if (this.StartNode.Center.X < this.EndNode.Center.X)
        {
          this.DrawStraightConnector(drawingContext);
        }
        else
        {
          this.DrawReversedConnector(drawingContext);
        }
      }

      return validBaseDraw;
    }

    /// <summary>
    /// Draws the curved connector.
    /// </summary>
    /// <param name="drawingContext">The drawing context.</param>
    private void DrawCurvedConnector(DrawingContext drawingContext)
    {
      Point start = this.StartNode.Center;
      Point end = this.EndNode.Center;

      double offsetX = (double)(DiagramContext.DetermineNestedOffset(this.StartNode.Node.MessageInfo) * 15.0D);
      start.Offset(offsetX, 0);
      end.Offset(offsetX, 0);

      start.Offset(0, -12);
      Point cornerOne = new Point(start.X + 15, start.Y);

      double nestedCallLength = (double)(DiagramContext.NestedCallCount(this.StartNode.Node.MessageInfo) * 40.0D);

      Point intermediateOne = new Point(cornerOne.X, cornerOne.Y + 24);
      Point intermediateTwo = new Point(cornerOne.X, cornerOne.Y + nestedCallLength);
      Point cornerTwo = new Point(intermediateTwo.X, intermediateTwo.Y + 24);
      end.Offset(0, 12 + nestedCallLength);
      drawingContext.DrawLine(this.Pen, start, cornerOne);
      drawingContext.DrawLine(this.Pen, cornerOne, intermediateOne);
      drawingContext.DrawLine(this.DashedPen, intermediateOne, intermediateTwo);
      drawingContext.DrawLine(this.Pen, intermediateTwo, cornerTwo);
      drawingContext.DrawLine(this.Pen, cornerTwo, end);
      string path = string.Format(CultureInfo.InvariantCulture, "M{0}L{1},{2}L{1},{3}Z", end, end.X + 3.464, end.Y - 2, end.Y + 2);
      drawingContext.DrawGeometry(this.Pen.Brush, this.Pen, Geometry.Parse(path));

      Point textStart = new Point(cornerOne.X + 4, cornerOne.Y + 5);
      Brush brush = Brushes.Black;
      drawingContext.DrawText(new FormattedText(this.StartNode.Node.MessageInfo.ToString(), CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 9, brush), textStart);
    }

    /// <summary>
    /// Draws the straight connector.
    /// </summary>
    /// <param name="drawingContext">The drawing context.</param>
    private void DrawStraightConnector(DrawingContext drawingContext)
    {
      Point start = this.StartNode.Center;
      Point end = this.EndNode.Center;

      double offsetX = (double)(DiagramContext.DetermineNestedOffset(this.StartNode.Node.MessageInfo) * 15.0D);
      start.Offset(offsetX, 0);

      start.Offset(0, 1);
      end.Offset(-6, 1);
      drawingContext.DrawLine(this.Pen, start, end);
      string path = string.Format(CultureInfo.InvariantCulture, "M{0}L{1},{2}L{1},{3}Z", end, end.X - 3.464, end.Y - 2, end.Y + 2);
      drawingContext.DrawGeometry(this.Pen.Brush, this.Pen, Geometry.Parse(path));

      Point textStart = start;
      textStart.Offset(15, -14);
      Brush brush = Brushes.Black;
      drawingContext.DrawText(new FormattedText(this.StartNode.Node.MessageInfo.ToString(), CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 9, brush), textStart);
    }

    /// <summary>
    /// Draws the reversed connector.
    /// </summary>
    /// <param name="drawingContext">The drawing context.</param>
    private void DrawReversedConnector(DrawingContext drawingContext)
    {
      Point start = this.StartNode.Center;
      Point end = this.EndNode.Center;

      double offsetX = (double)(DiagramContext.DetermineNestedOffset(this.StartNode.Node.MessageInfo) * 15.0D);
      start.Offset(offsetX, 0);

      start.Offset(-6, 1);
      end.Offset(0, 1);
      drawingContext.DrawLine(this.Pen, start, end);
      string path = string.Format(CultureInfo.InvariantCulture, "M{0}L{1},{2}L{1},{3}Z", end, end.X + 3.464, end.Y - 2, end.Y + 2);
      drawingContext.DrawGeometry(this.Pen.Brush, this.Pen, Geometry.Parse(path));

      Point textStart = end;
      textStart.Offset(15, -14);
      Brush brush = Brushes.Black;
      drawingContext.DrawText(new FormattedText(this.StartNode.Node.MessageInfo.ToString(), CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 9, brush), textStart);
    }

    /// <summary>
    /// Measures the connector.
    /// </summary>
    /// <returns>The ultimate right position for the connector and nodes combination.</returns>
    private double MeasureConnector()
    {
        double result = 0;
        double textRight = 0;

        if (this.StartNode.Node.ObjectInfo == this.EndNode.Node.ObjectInfo)
        {
            // Curved connector
            result = this.StartNode.TopRight.X;
            int textWidth = DiagramUtility.DetermineLength(this.StartNode.Node.MessageInfo.ToString(), "Segoe UI", 9F, false);
            textRight = this.StartNode.Center.X + 34 + textWidth;
        }
        else if (this.StartNode.Center.X < this.EndNode.Center.X)
        {
            result = this.EndNode.TopRight.X;
            int textWidth = DiagramUtility.DetermineLength(this.StartNode.Node.MessageInfo.ToString(), "Segoe UI", 9F, false);
            textRight = this.StartNode.Center.X + 15 + textWidth;
        }
        else
        {
            result = this.StartNode.TopRight.X;
            int textWidth = DiagramUtility.DetermineLength(this.StartNode.Node.MessageInfo.ToString(), "Segoe UI", 9F, false);
            textRight = this.EndNode.Center.X + 15 + textWidth;
        }

        result = Math.Max(result, textRight);
        return result;
    }
  }
}
