namespace LiveSequence.Common.Domain
{
  /// <summary>
  /// Enumeration for the class modifiers.
  /// </summary>
  public enum TypeModifier
  {
    /// <summary>
    /// No additional modifier exists.
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates the type is an interface.
    /// </summary>
    Interface,

    /// <summary>
    /// Indicates the type is an abstract class.
    /// </summary>
    Abstract,

    /// <summary>
    /// Indicates the type is a sealed class.
    /// </summary>
    Sealed,

    /// <summary>
    /// Indicates the type is a static class.
    /// </summary>
    Static,

    /// <summary>
    /// Indicates the type is an enumeration
    /// </summary>
    Enumeration,

    /// <summary>
    /// Indicates the type is a struct.
    /// </summary>
    Struct,

    /// <summary>
    /// Indicates the type is a delegate.
    /// </summary>
    Delegate
  }
}
