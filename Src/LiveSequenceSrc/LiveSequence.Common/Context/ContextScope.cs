namespace LiveSequence.Common.Context
{
  using System;
  using System.Threading;

  /// <summary>
  /// Represents the ContextBaseScope class which should act as a threaded scope container
  /// for a <see cref="ContextBase"/> inherited context object.
  /// </summary>
  /// <typeparam name="T">A <see cref="ContextBase"/> inherited context object.</typeparam>
  public abstract class ContextScope<T> : IDisposable where T : ContextBase
  {
    /// <summary>Contains a reference to the original active LogContext.</summary>
    private readonly ContextBase originalContext;

    /// <summary>Contains a reference to the original active LogContextScope.</summary>
    private readonly ContextScope<T> originalScope;

    /// <summary>Contains a reference to the thread on which the current active LogContext was created.</summary>
    private readonly Thread thread;

    /// <summary>Contains a reference to the current active LogContextScope object.</summary>
    [ThreadStatic]
    private static ContextScope<T> currentScope;

    /// <summary>Contains a reference to the current active LogContext object.</summary>
    private ContextBase currentContext;

    /// <summary>Indicates whether or not the current instance has been disposed.</summary>
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the ContextBaseScope class.
    /// </summary>
    /// <param name="context">The context.</param>
    protected ContextScope(ContextBase context)
    {
      this.originalContext = ContextBase.Current;
      this.originalScope = currentScope;
      this.thread = Thread.CurrentThread;
      this.PushContext(context);
    }

    /// <summary>
    /// Gets the current context
    /// </summary>
    public T CurrentContext
    {
      get
      {
        if (ContextBase.Current != this.currentContext)
        {
          throw new InvalidOperationException(Properties.Resources.ContextModifiedInsideScope);
        }

        return ContextBase.Current as T;
      }
    }

    #region IDisposable Members

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources
    /// </summary>
    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }

    #endregion

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (!this.disposed)
        {
          this.disposed = true;
          this.PopContext();
        }
      }
    }

    /// <summary>
    /// Pops the context.
    /// </summary>
    private void PopContext()
    {
      if (this.thread != Thread.CurrentThread)
      {
        throw new InvalidOperationException(Properties.Resources.InvalidContextScopeThread);
      }

      if (currentScope != this)
      {
        throw new InvalidOperationException(Properties.Resources.InterleavedContextScopes);
      }

      if (ContextBase.Current != this.currentContext)
      {
        throw new InvalidOperationException(Properties.Resources.ContextModifiedInsideScope);
      }

      currentScope = this.originalScope;
      ContextBase.Current = this.originalContext;
      if (this.currentContext != null)
      {
        this.currentContext.ReleaseContext();
        this.currentContext = null;
      }
    }

    /// <summary>
    /// Pushes the context.
    /// </summary>
    /// <param name="context">The context.</param>
    private void PushContext(ContextBase context)
    {
      this.currentContext = context;
      currentScope = this;
      ContextBase.Current = this.currentContext;
    }
  }
}
