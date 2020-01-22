namespace LiveSequence.Common.Context
{
  /// <summary>
  /// Implementation of the ContextScope for the SaveContext.
  /// </summary>
  public sealed class SaveContextScope : ContextScope<SaveContext>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="SaveContextScope"/> class.
    /// </summary>
    /// <param name="contextParameters">The context parameters.</param>
    internal SaveContextScope(ContextParameters contextParameters)
      : base(new SaveContext(contextParameters))
    {
    }
  }
}
