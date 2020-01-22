namespace LiveSequence.Common.Graphics
{
  using System;
  using System.Globalization;

  /// <summary>
  /// Static class with general utility methods to support the Diagram rendering.
  /// </summary>
  internal static class DiagramUtility
  {
    /// <summary>
    /// Determines the length of a rendered string.
    /// </summary>
    /// <param name="value">The string value.</param>
    /// <param name="fontName">Name of the font.</param>
    /// <param name="fontSize">Size of the font.</param>
    /// <param name="bold">if set to <c>true</c> [bold].</param>
    /// <returns>The length of the string as int.</returns>
    internal static int DetermineLength(string value, string fontName, float fontSize, bool bold)
    {
      using (System.Drawing.Font font = new System.Drawing.Font(fontName, fontSize, bold ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular))
      {
        using (System.Drawing.Bitmap b = new System.Drawing.Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
        {
          using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(b))
          {
            System.Drawing.SizeF size = g.MeasureString(value, font);
            return Convert.ToInt32(size.Width, CultureInfo.InvariantCulture);
          }
        }
      }
    }

    /// <summary>
    /// Determines the length.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="fontName">Name of the font.</param>
    /// <param name="fontSize">Size of the font.</param>
    /// <param name="bold">if set to <c>true</c> [bold].</param>
    /// <param name="currentWidth">Width of the current.</param>
    /// <returns>The length of the string as int.</returns>
    internal static int DetermineLength(string value, string fontName, float fontSize, bool bold, string currentWidth)
    {
      int current = Convert.ToInt32(currentWidth, CultureInfo.InvariantCulture);
      return Math.Max(current, DetermineLength(value, fontName, fontSize, bold));
    }

    /// <summary>
    /// Converts the framework element to drawing visual.
    /// </summary>
    /// <param name="element">The framework element that must be converted.</param>
    /// <param name="size">The size of the diagram.</param>
    /// <returns>A new DrawingVisual with the Diagram.</returns>
    internal static System.Windows.Media.DrawingVisual DrawingVisualFromFrameworkElement(System.Windows.FrameworkElement element, System.Windows.Size size)
    {
      System.Windows.Media.DrawingVisual dv = new System.Windows.Media.DrawingVisual();

      System.Windows.Media.Imaging.RenderTargetBitmap renderBitmap = new System.Windows.Media.Imaging.RenderTargetBitmap(
        (int)size.Width,
        (int)size.Height,
        96D,
        96D,
        System.Windows.Media.PixelFormats.Default);
      renderBitmap.Render(element);

      using (System.Windows.Media.DrawingContext context = dv.RenderOpen())
      {
        context.DrawImage(renderBitmap, new System.Windows.Rect(size));
      }

      return dv;
    }
  }
}
