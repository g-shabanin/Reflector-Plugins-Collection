namespace LiveSequence.Common.Context
{
  /// <summary>
  /// Implementation of the ContextScope for the SequenceContext.
  /// </summary>
  public sealed class SequenceContextScope : ContextScope<SequenceContext>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="SequenceContextScope"/> class.
    /// </summary>
    /// <param name="contextParameters">The context parameters.</param>
    internal SequenceContextScope(ContextParameters contextParameters)
      : base(new SequenceContext(contextParameters))
    {
    }
  }
}
