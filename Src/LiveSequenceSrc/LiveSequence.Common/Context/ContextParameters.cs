namespace LiveSequence.Common.Context
{
  using System.Windows.Controls;
  using LiveSequence.Common.Graphics;

  /// <summary>
  /// Provides access to objects that act as parameter containers.
  /// </summary>
  internal abstract class ContextParameters
  {
    /// <summary>
    /// Gets the empty instance.
    /// </summary>
    /// <value>The empty instance.</value>
    internal static ContextParameters Empty
    {
      get
      {
        return new EmptyContextParameters();
      }
    }

    /// <summary>
    /// Gets the print context parameters.
    /// </summary>
    /// <param name="viewer">The viewer.</param>
    /// <param name="printDialog">The print dialog.</param>
    /// <returns>A new PrintContextParameters object.</returns>
    internal static ContextParameters GetPrintContextParameters(DiagramViewer viewer, PrintDialog printDialog)
    {
      return new PrintContextParameters(viewer, printDialog);
    }

    /// <summary>
    /// Gets the save context parameters.
    /// </summary>
    /// <param name="viewer">The viewer.</param>
    /// <param name="targetFileName">Name of the target file.</param>
    /// <returns>A new SaveContextParameters object.</returns>
    internal static ContextParameters GetSaveContextParameters(DiagramViewer viewer, string targetFileName)
    {
      return new SaveContextParameters(viewer, targetFileName);
    }
  }
}
