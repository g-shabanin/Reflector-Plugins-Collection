namespace LiveSequence.Common.Context
{
  using LiveSequence.Common.Graphics;

  /// <summary>
  /// Provides a parameter container for the SaveContext.
  /// </summary>
  internal sealed class SaveContextParameters : ContextParameters
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="SaveContextParameters"/> class.
    /// </summary>
    /// <param name="viewer">The viewer.</param>
    /// <param name="targetFileName">Name of the target file.</param>
    internal SaveContextParameters(DiagramViewer viewer, string targetFileName)
    {
      this.Viewer = viewer;
      this.TargetFileName = targetFileName;
    }

    /// <summary>
    /// Gets the viewer.
    /// </summary>
    /// <value>The viewer.</value>
    public DiagramViewer Viewer { get; private set; }

    /// <summary>
    /// Gets the name of the target file.
    /// </summary>
    /// <value>The name of the target file.</value>
    public string TargetFileName { get; private set; }
  }
}
