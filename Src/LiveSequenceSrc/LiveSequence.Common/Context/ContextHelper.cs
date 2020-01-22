namespace LiveSequence.Common.Context
{
  using System.Windows.Controls;
  using LiveSequence.Common.Graphics;

  /// <summary>
  /// Provides means to create a scope in generic sense.
  /// </summary>
  public static class ContextHelper
  {
    /// <summary>
    /// Creates the print scope.
    /// </summary>
    /// <param name="viewer">The viewer.</param>
    /// <param name="printDialog">The print dialog.</param>
    /// <returns>A new PrintContextScope object.</returns>
    public static PrintContextScope CreatePrintScope(DiagramViewer viewer, PrintDialog printDialog)
    {
      return new PrintContextScope(ContextParameters.GetPrintContextParameters(viewer, printDialog));
    }

    /// <summary>
    /// Creates the save scope.
    /// </summary>
    /// <param name="viewer">The viewer.</param>
    /// <param name="targetFileName">Name of the target file.</param>
    /// <returns>A new SaveContextScope object.</returns>
    public static SaveContextScope CreateSaveScope(DiagramViewer viewer, string targetFileName)
    {
      return new SaveContextScope(ContextParameters.GetSaveContextParameters(viewer, targetFileName));
    }

    /// <summary>
    /// Creates the sequence scope.
    /// </summary>
    /// <returns>A new SequenceContextScope object.</returns>
    public static SequenceContextScope CreateSequenceScope()
    {
      return new SequenceContextScope(ContextParameters.Empty);
    }

    /// <summary>
    /// Creates the model scope.
    /// </summary>
    /// <returns>A new ModelContextScope object.</returns>
    public static ModelContextScope CreateModelScope()
    {
      return new ModelContextScope(ContextParameters.Empty);
    }
  }
}
