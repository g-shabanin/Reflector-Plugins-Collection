namespace LiveSequence.Common.Domain
{
  using System;

  /// <summary>
  /// Event arguments implementation to use with the ContentChanged notification
  /// </summary>
  internal sealed class ContentChangedEventArgs : EventArgs
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentChangedEventArgs"/> class.
    /// </summary>
    /// <param name="newInfo">The new info.</param>
    internal ContentChangedEventArgs(ObjectInfo newInfo)
    {
      this.NewInfo = newInfo;
    }

    /// <summary>
    /// Gets the new info.
    /// </summary>
    /// <value>The new info.</value>
    public ObjectInfo NewInfo
    {
      get;
      private set;
    }
  }
}
