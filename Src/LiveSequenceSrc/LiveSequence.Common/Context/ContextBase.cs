namespace LiveSequence.Common.Context
{
  /// <summary>
  /// Represents the Context base class.
  /// </summary>
  public abstract class ContextBase
  {
    /// <summary>Contains a reference to the current context.</summary>
    private static ContextBase currentContext;

    /// <summary>
    /// Gets or sets the current context.
    /// </summary>
    /// <value>The current context.</value>
    public static ContextBase Current
    {
      get
      {
        return currentContext;
      }

      set
      {
        currentContext = value;
      }
    }

    /// <summary>
    /// Releases the context.
    /// </summary>
    internal abstract void ReleaseContext();
  }
}
