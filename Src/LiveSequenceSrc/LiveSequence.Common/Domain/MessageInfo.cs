namespace LiveSequence.Common.Domain
{
  /// <summary>
  /// Container class for the MessageInfo objects.
  /// </summary>
  internal sealed class MessageInfo
  {
    /// <summary>
    /// Contains a reference to the parent MessageInfo object.
    /// </summary>
    private MessageInfo parent;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageInfo"/> class.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    /// <param name="methodCallName">Name of the method call.</param>
    /// <param name="methodCallInfo">The method call info.</param>
    internal MessageInfo(ObjectInfo source, ObjectInfo target, string methodCallName, MethodCallInfo methodCallInfo)
    {
      this.Source = source;
      this.Target = target;
      this.MethodCallName = methodCallName;
      this.MethodCallInfo = methodCallInfo;
    }

    /// <summary>
    /// Gets the source.
    /// </summary>
    /// <value>The source.</value>
    public ObjectInfo Source
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets the target.
    /// </summary>
    /// <value>The target.</value>
    public ObjectInfo Target
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets the name of the method call.
    /// </summary>
    /// <value>The name of the method call.</value>
    public string MethodCallName
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets the method call info.
    /// </summary>
    /// <value>The method call info.</value>
    public MethodCallInfo MethodCallInfo
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets or sets the parent.
    /// </summary>
    /// <value>The parent.</value>
    internal MessageInfo Parent
    {
      get
      {
        return this.parent;
      }

      set
      {
        this.parent = value;
        this.ParentIsSet = true;
      }
    }

    /// <summary>
    /// Gets or sets the nesting level.
    /// </summary>
    /// <value>The nesting level.</value>
    internal int NestingLevel { get; set; }

    /// <summary>
    /// Gets a value indicating whether [parent is set].
    /// </summary>
    /// <value><c>true</c> if [parent is set]; otherwise, <c>false</c>.</value>
    internal bool ParentIsSet { get; private set; }

    /// <summary>
    /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:OpenSequence.Xps.Data.MessageInfo"/>.
    /// </summary>
    /// <remarks>The string representation of the MessageInfo object is it's MethodCallName property's value.</remarks>
    /// <returns>
    /// A <see cref="T:System.String"/> that represents the current <see cref="T:OpenSequence.Xps.Data.MessageInfo"/>.
    /// </returns>
    public override string ToString()
    {
      return this.MethodCallName;
    }
  }
}
