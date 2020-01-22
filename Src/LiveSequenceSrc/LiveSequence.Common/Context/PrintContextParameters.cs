namespace LiveSequence.Common.Context
{
  using System.Windows.Controls;
  using LiveSequence.Common.Graphics;

  /// <summary>
  /// Provides a parameter cont
  /// </summary>
  internal sealed class PrintContextParameters : ContextParameters
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="PrintContextParameters"/> class.
    /// </summary>
    /// <param name="viewer">The viewer.</param>
    /// <param name="printDialog">The print dialog.</param>
    internal PrintContextParameters(DiagramViewer viewer, PrintDialog printDialog)
    {
      this.Viewer = viewer;
      this.PrintDialog = printDialog;
    }

    /// <summary>
    /// Gets the viewer.
    /// </summary>
    /// <value>The viewer.</value>
    public DiagramViewer Viewer { get; private set; }

    /// <summary>
    /// Gets the print dialog.
    /// </summary>
    /// <value>The print dialog.</value>
    public PrintDialog PrintDialog { get; private set; }
  }
}
