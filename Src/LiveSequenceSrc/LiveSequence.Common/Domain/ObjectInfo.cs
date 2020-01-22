#define CODE_ANALYSIS
namespace LiveSequence.Common.Domain
{
  using System.Diagnostics.CodeAnalysis;

  /// <summary>
  /// Container for the ObjectInfo object that contains the information of the types that are present in the sequence.
  /// </summary>
  internal class ObjectInfo
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectInfo"/> class.
    /// </summary>
    /// <param name="key">The key of the type.</param>
    /// <param name="typeName">Name of the type.</param>
    internal ObjectInfo(string key, string typeName)
    {
      this.Key = key;
      this.TypeName = typeName;
    }

    /// <summary>
    /// Gets the key.
    /// </summary>
    /// <value>The key of the type.</value>
    public string Key
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets the name of the type.
    /// </summary>
    /// <value>The name of the type.</value>
    public string TypeName
    {
      get;
      private set;
    }

    /// <summary>
    /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:OpenSequence.Xps.Data.ObjectInfo"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"/> that represents the current <see cref="T:OpenSequence.Xps.Data.ObjectInfo"/>.
    /// </returns>
    [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "We're displaying the Key in front of the TypeName. In a sequence this should be in lowercase.")]
    public override string ToString()
    {
      return string.Concat(this.Key.ToLowerInvariant(), ":", this.TypeName);
    }
  }
}
