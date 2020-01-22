namespace LiveSequence.Common.Domain
{
  /// <summary>
  /// Container class for the connector info for the Class model diagram.
  /// </summary>
  public sealed class ClassConnectorInfo
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ClassConnectorInfo"/> class.
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <param name="child">The child.</param>
    /// <param name="connectorName">Name of the connector.</param>
    public ClassConnectorInfo(ClassTypeInfo parent, ClassTypeInfo child, string connectorName)
    {
      this.Name = connectorName;
      this.Parent = parent;
      this.Child = child;
    }

    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <value>The connector name.</value>
    public string Name { get; private set; }

    /// <summary>
    /// Gets the parent.
    /// </summary>
    /// <value>The parent.</value>
    public ClassTypeInfo Parent { get; private set; }

    /// <summary>
    /// Gets the child.
    /// </summary>
    /// <value>The child.</value>
    public ClassTypeInfo Child { get; private set; }
  }
}
