namespace LiveSequence.Common.Context
{
  /// <summary>
  /// Implementation of the ContextScope for the PrintContext.
  /// </summary>
  public sealed class PrintContextScope : ContextScope<PrintContext>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="PrintContextScope"/> class.
    /// </summary>
    /// <param name="contextParameters">The context parameters.</param>
    internal PrintContextScope(ContextParameters contextParameters)
      : base(new PrintContext(contextParameters))
    {
    }
  }
}
