namespace LiveSequence.Common.Context
{
  /// <summary>
  /// Implementation of the ContextScope for the ModelContext.
  /// </summary>
  public sealed class ModelContextScope : ContextScope<ModelContext>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ModelContextScope"/> class.
    /// </summary>
    /// <param name="contextParameters">The context parameters.</param>
    internal ModelContextScope(ContextParameters contextParameters)
      : base(new ModelContext(contextParameters))
    {
    }
  }
}