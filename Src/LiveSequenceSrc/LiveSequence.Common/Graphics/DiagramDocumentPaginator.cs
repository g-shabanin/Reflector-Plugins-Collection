namespace LiveSequence.Common.Graphics
{
  using System;
  using System.Windows;
  using System.Windows.Documents;
  using System.Windows.Media;

  /// <summary>
  /// Provides an implementation of the DocumentPaginator class to wrap the Diagram visual into multiple-page elements.
  /// </summary>
  internal sealed class DiagramDocumentPaginator : DocumentPaginator
  {
    /// <summary>Contains a reference to the content that will be printed.</summary>
    private Drawing diagram;

    /// <summary>Contains the size of the area that will be printed with Diagram content.</summary>
    private Size contentSize;

    /// <summary>Contains the rectangle that will be sent to the printer.</summary>
    private Rect frameRect;

    /// <summary>Contains a reference to the Pen that will be used.</summary>
    private Pen framePen;

    /// <summary>Contains the page count in the X direction.</summary>
    private int pageCountX;

    /// <summary>Contains the page count in the Y direction.</summary>
    private int pageCountY;

    /// <summary>
    /// Initializes a new instance of the <see cref="DiagramDocumentPaginator"/> class.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="printSize">Size of the print.</param>
    internal DiagramDocumentPaginator(DrawingVisual source, Size printSize)
    {
      this.PageSize = printSize;
      this.contentSize = new Size(this.PageSize.Width - (2 * Constants.Margin), this.PageSize.Height - (2 * Constants.Margin));
      this.frameRect = new Rect(new Point(Constants.Margin, Constants.Margin), this.contentSize);
      this.frameRect.Inflate(1, 1);
      this.framePen = new Pen(Brushes.Black, 0.1);

      // Transformation to borderless print size
      Rect bounds = source.DescendantBounds;
      bounds.Union(source.ContentBounds);
      Matrix m = new Matrix();
      m.Translate(-bounds.Left, -bounds.Top);
      double scale = 1; // hardcoded zoom for printing
      this.pageCountX = (int)((bounds.Width * scale) / this.contentSize.Width) + 1;
      this.pageCountY = (int)((bounds.Height * scale) / this.contentSize.Height) + 1;
      m.Scale(scale, scale);

      // Center on available pages
      m.Translate(((this.pageCountX * this.contentSize.Width) - (bounds.Width * scale)) / 2, ((this.pageCountY * this.contentSize.Height) - (bounds.Height * scale)) / 2);

      // Create a new Visual
      DrawingVisual v = new DrawingVisual();
      using (DrawingContext dc = v.RenderOpen())
      {
        dc.PushTransform(new MatrixTransform(m));
        dc.DrawDrawing(source.Drawing);
        foreach (DrawingVisual dv in source.Children)
        {
          dc.DrawDrawing(dv.Drawing);
        }
      }

      this.diagram = v.Drawing;
    }

    /// <summary>
    /// Gets a value indicating whether <see cref="P:System.Windows.Documents.DocumentPaginator.PageCount"/> is the total number of pages.
    /// </summary>
    /// <returns>true if pagination is complete and <see cref="P:System.Windows.Documents.DocumentPaginator.PageCount"/> is the total number of pages; otherwise, false, if pagination is in process and <see cref="P:System.Windows.Documents.DocumentPaginator.PageCount"/> is the number of pages currently formatted (not the total).This value may revert to false, after being true, if <see cref="P:System.Windows.Documents.DocumentPaginator.PageSize"/> or content changes; because those events would force a repagination.</returns>
    public override bool IsPageCountValid
    {
      get
      {
        return true;
      }
    }

    /// <summary>
    /// Gets a count of the number of pages currently formatted
    /// </summary>
    /// <value></value>
    /// <returns>A count of the number of pages that have been formatted.</returns>
    public override int PageCount
    {
      get
      {
        return this.pageCountX * this.pageCountY;
      }
    }

    /// <summary>
    /// Gets or sets the suggested width and height of each page.
    /// </summary>
    /// <value>The page size.</value>
    /// <returns>A <see cref="T:System.Windows.Size"/> representing the width and height of each page.</returns>
    public override Size PageSize { get; set; }

    /// <summary>
    /// Gets the element being paginated.
    /// </summary>
    /// <value>Always returns null from this implementation.</value>
    /// <returns>An <see cref="T:System.Windows.Documents.IDocumentPaginatorSource"/> representing the element being paginated.</returns>
    public override IDocumentPaginatorSource Source
    {
      get
      {
        return null;
      }
    }

    /// <summary>
    /// Gets the <see cref="T:System.Windows.Documents.DocumentPage"/> for the specified page number.
    /// </summary>
    /// <param name="pageNumber">The zero-based page number of the document page that is needed.</param>
    /// <returns>
    /// The <see cref="T:System.Windows.Documents.DocumentPage"/> for the specified <paramref name="pageNumber"/>, or <see cref="F:System.Windows.Documents.DocumentPage.Missing"/> if the page does not exist.
    /// </returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="pageNumber"/> is negative.</exception>
    public override DocumentPage GetPage(int pageNumber)
    {
      if (pageNumber < 0)
      {
        throw new ArgumentOutOfRangeException("pageNumber", Properties.Resources.PageNumberIsNegative);
      }

      int x = pageNumber % this.pageCountX;
      int y = pageNumber / this.pageCountX;

      Rect view = new Rect();
      view.X = x * this.contentSize.Width;
      view.Y = y * this.contentSize.Height;
      view.Size = this.contentSize;

      DrawingVisual v = new DrawingVisual();
      using (DrawingContext dc = v.RenderOpen())
      {
        dc.DrawRectangle(null, this.framePen, this.frameRect);
        dc.PushTransform(new TranslateTransform(Constants.Margin - view.X, Constants.Margin - view.Y));
        dc.PushClip(new RectangleGeometry(view));
        dc.DrawDrawing(this.diagram);
      }

      return new DocumentPage(v, this.PageSize, this.frameRect, this.frameRect);
    }

    /// <summary>
    /// Provides a nice way of getting to the constant values used within this class.
    /// </summary>
    private static class Constants
    {
      /// <summary>
      /// Gets the margin.
      /// </summary>
      /// <value>The margin.</value>
      public static double Margin
      {
        get
        {
          return 1.5 * 96 / 2.54;
        }
      }
    }
  }
}
