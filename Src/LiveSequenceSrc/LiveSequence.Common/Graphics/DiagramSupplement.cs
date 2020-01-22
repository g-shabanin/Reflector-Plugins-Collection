namespace LiveSequence.Common.Graphics
{
  using System.Collections.Generic;
  using System.Globalization;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Media;

  /// <summary>
  /// Adds a supplement to the diagram node. Used to display the model node's interfaces.
  /// </summary>
  public sealed class DiagramSupplement : ContentControl
  {
    /// <summary>
    /// Contains a reference to the list of interfaces.
    /// </summary>
    private List<string> interfaceNames = new List<string>();

    /// <summary>
    /// Gets the calculated height.
    /// </summary>
    /// <value>The calculated height.</value>
    public double CalculatedHeight
    {
      get
      {
        return Constants.InitialHeight + (this.interfaceNames.Count * Constants.LineHeight);
      }
    }

    /// <summary>
    /// Adds the interface.
    /// </summary>
    /// <param name="interfaceName">Name of the interface.</param>
    public void AddInterface(string interfaceName)
    {
      this.interfaceNames.Add(interfaceName);
    }

    /// <summary>
    /// When overridden in a derived class, participates in rendering operations that are directed by the layout system. The rendering instructions for this element are not used directly when this method is invoked, and are instead preserved for later asynchronous use by layout and drawing.
    /// </summary>
    /// <param name="drawingContext">The drawing instructions for a specific element. This context is provided to the layout system.</param>
    protected override void OnRender(DrawingContext drawingContext)
    {
      // When no interfaces have been defined, there is nothing to draw.
      if (this.interfaceNames.Count == 0)
      {
        return;
      }

      base.OnRender(drawingContext);

      Pen pen = new Pen(Brushes.Gray, 0.5);
      Point origin = new Point(30, 3);
      double length = Constants.StartHeight + (this.interfaceNames.Count * Constants.LineHeight);

      // draw path at top left
      drawingContext.DrawLine(pen, origin, new Point(origin.X, origin.Y - length));
      drawingContext.DrawEllipse(Brushes.Transparent, pen, new Point(origin.X, origin.Y - length - Constants.Radius), Constants.Radius, Constants.Radius);

      // draw interface names
      origin.Offset(5.0D, -length - Constants.Radius);
      foreach (string name in this.interfaceNames)
      {
        drawingContext.DrawText(new FormattedText(name, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 7, pen.Brush), origin);
        origin.Offset(0, 7.0D);
      }
    }

    /// <summary>
    /// Container for the constant values
    /// </summary>
    private static class Constants
    {
      /// <summary>
      /// Gets the height of the line.
      /// </summary>
      /// <value>The height of the line.</value>
      internal static double LineHeight
      {
        get
        {
          return 7.0D;
        }
      }

      /// <summary>
      /// Gets the initial height.
      /// </summary>
      /// <value>The initial height.</value>
      internal static double InitialHeight
      {
        get
        {
          return 20.0D;
        }
      }

      /// <summary>
      /// Gets the start height.
      /// </summary>
      /// <value>The start height.</value>
      internal static double StartHeight
      {
        get
        {
          return 3.0D;
        }
      }

      /// <summary>
      /// Gets the radius.
      /// </summary>
      /// <value>The radius.</value>
      internal static double Radius
      {
        get
        {
          return 3.5D;
        }
      }
    }
  }
}