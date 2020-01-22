namespace LiveSequence.Common.Domain
{
  /// <summary>
  /// Container for the object relation info. This class is used to store information
  /// on relations between two ObjectInfo instances, either association or inheritance.
  /// </summary>
  internal sealed class ObjectRelationInfo
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectRelationInfo"/> class.
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <param name="child">The child info.</param>
    /// <param name="relationName">Name of the relation.</param>
    internal ObjectRelationInfo(ExtendedObjectInfo parent, ExtendedObjectInfo child, string relationName)
    {
      this.RelationName = relationName;
      this.Parent = parent;
      this.Child = child;
    }

    /// <summary>
    /// Gets the name of the relation.
    /// </summary>
    /// <value>The name of the relation.</value>
    public string RelationName { get; private set; }

    /// <summary>
    /// Gets the parent.
    /// </summary>
    /// <value>The parent.</value>
    public ObjectInfo Parent { get; private set; }

    /// <summary>
    /// Gets the child.
    /// </summary>
    /// <value>The child.</value>
    public ObjectInfo Child { get; private set; }
  }
}
