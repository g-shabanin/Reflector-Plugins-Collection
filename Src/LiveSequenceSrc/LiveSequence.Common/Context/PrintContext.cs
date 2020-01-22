namespace LiveSequence.Common.Context
{
  using System;
  using System.Printing;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Media;
  using System.Windows.Xps;
  using LiveSequence.Common.Graphics;

  /// <summary>
  /// Context class for the Print functionality of the diagram viewer.
  /// </summary>
  public sealed class PrintContext : ContextBase
  {
    /// <summary>
    /// Contains a reference to the DiagramViewer whos content should be sent to the printer.
    /// </summary>
    private DiagramViewer viewer;

    /// <summary>
    /// Contains a reference to the PrintDialog that is used for printing.
    /// </summary>
    private PrintDialog printDialog;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrintContext"/> class.
    /// </summary>
    /// <param name="contextParameters">The context parameters.</param>
    internal PrintContext(ContextParameters contextParameters)
    {
      PrintContextParameters pcp = contextParameters as PrintContextParameters;
      if (pcp == null)
      {
        throw new ArgumentException(Properties.Resources.ArgumentHasInvalidType, "contextParameters");
      }

      this.viewer = pcp.Viewer;
      this.printDialog = pcp.PrintDialog;
    }

    /// <summary>
    /// Gets the diagram control.
    /// </summary>
    /// <value>The diagram control.</value>
    public DiagramViewer DiagramControl
    {
      get
      {
        return this.viewer;
      }
    }

    /// <summary>
    /// Releases the context.
    /// </summary>
    internal override void ReleaseContext()
    {
      // save current diagram transform
      Transform transform = this.DiagramControl.Diagram.LayoutTransform;

      // reset current transform (in case it is scaled or rotated)
      this.DiagramControl.Diagram.LayoutTransform = null;

      // Get the desired size of the diagram
      Size size = new Size(this.DiagramControl.Diagram.DesiredSize.Width + 50, this.DiagramControl.Diagram.DesiredSize.Height + 50);

      // Measure and arrange elements
      this.DiagramControl.Diagram.Measure(size);
      this.DiagramControl.Diagram.Arrange(new Rect(size));

      // Print the content of DiagramControl.Diagram...
      XpsDocumentWriter xpsWriter = PrintQueue.CreateXpsDocumentWriter(this.printDialog.PrintQueue);

      PrintTicket xpsTicket = this.printDialog.PrintTicket;
      if (this.DiagramControl.Diagram.DesiredSize.Width > this.DiagramControl.Diagram.DesiredSize.Height)
      {
        xpsTicket.PageOrientation = PageOrientation.Landscape;
      }
      else
      {
        xpsTicket.PageOrientation = PageOrientation.Portrait;
      }

      // It is not feasible to have one portion of the diagram on the front and another at the back.
      xpsTicket.Duplexing = Duplexing.OneSided;

      // Set the print size, based on the printable area.
      Size printSize = new Size(this.printDialog.PrintableAreaWidth, this.printDialog.PrintableAreaHeight);

      try
      {
        DiagramDocumentPaginator paginator = new DiagramDocumentPaginator(
          DiagramUtility.DrawingVisualFromFrameworkElement(this.DiagramControl.Diagram, size),
          printSize);

        // print the document straight forward...
        xpsWriter.Write(paginator, xpsTicket);
      }
      catch (PrintSystemException ex)
      {
        // log exception....
        Console.WriteLine(ex);
      }

      // Restore previously saved layout
      this.DiagramControl.Diagram.LayoutTransform = transform;

      // Reset the scroll
      this.DiagramControl.ResetScrollPosition();
    }
  }
}
