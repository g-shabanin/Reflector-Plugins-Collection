namespace LiveSequence.Common.Graphics
{
  /// <summary>
  /// Enumeration used for the node types.
  /// </summary>
  internal enum NodeType
  {
    /// <summary>
    /// Identifies the type nodes at the top of the sequence diagram.
    /// </summary>
    TypeInfo,

    /// <summary>
    /// Identifies the message nodes within the sequence diagram.
    /// </summary>
    MessageInfo,

    /// <summary>
    /// Identifies the class nodes within the class model diagram
    /// </summary>
    Class,

    /// <summary>
    /// Identifies the abstract class nodes within the class model diagram
    /// </summary>
    Abstract,

    /// <summary>
    /// Identifies the static class nodes within the class model diagram
    /// </summary>
    Static,

    /// <summary>
    /// Identifies the interface nodes within the class model diagram
    /// </summary>
    Interface,

    /// <summary>
    /// Identifies the sealed class nodes within the class model diagram
    /// </summary>
    Sealed,

    /// <summary>
    /// Identifies the enumeration nodes within the class model diagram
    /// </summary>
    Enumeration,

    /// <summary>
    /// Identifies the struct nodes within the class model diagram
    /// </summary>
    Struct,

    /// <summary>
    /// Identifies the delegate nodes within the class model diagram
    /// </summary>
    Delegate
  }
}