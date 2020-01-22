using System.Collections.Generic;
using System.Text;

namespace LiveSequence.Common.Domain
{
  public sealed class ClassTypeInfo
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ClassTypeInfo"/> class.
    /// </summary>
    public ClassTypeInfo()
    {
      this.Interfaces = new List<ClassTypeInfo>();
      this.GenericParameters = new List<ClassTypeInfo>();
      this.PropertyList = new Dictionary<string, ClassTypeInfo>();
    }

    /// <summary>
    /// Gets or sets the start name of the type.
    /// </summary>
    /// <value>The start name of the type.</value>
    public string StartTypeName
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the name of the type.
    /// </summary>
    /// <value>The name of the type.</value>
    public string TypeName
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the namespace.
    /// </summary>
    /// <value>The namespace.</value>
    public string Namespace
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the modifier.
    /// </summary>
    /// <value>The modifier.</value>
    public string Modifier
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the type of the base.
    /// </summary>
    /// <value>The type of the base.</value>
    public ClassTypeInfo BaseType
    {
      get;
      set;
    }

    /// <summary>
    /// Gets the interfaces.
    /// </summary>
    /// <value>The interfaces.</value>
    public List<ClassTypeInfo> Interfaces
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets the generic parameters.
    /// </summary>
    /// <value>The generic parameters.</value>
    public List<ClassTypeInfo> GenericParameters
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets the property list.
    /// </summary>
    /// <value>The property list.</value>
    public Dictionary<string, ClassTypeInfo> PropertyList
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets the full name.
    /// </summary>
    /// <value>The full name.</value>
    public string FullName
    {
      get
      {
        if (string.IsNullOrEmpty(this.Namespace))
        {
          return this.ToString();
        }

        return string.Concat(this.Namespace, ".", this.ToString());
      }
    }

    /// <summary>
    /// Gets the key.
    /// </summary>
    /// <value>The key for this instance.</value>
    internal string Key
    {
      get
      {
        return this.FullName;
      }
    }

    /// <summary>
    /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Reflector.Sequence.ClassTypeInfo"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"/> that represents the current <see cref="T:Reflector.Sequence.ClassTypeInfo"/>.
    /// </returns>
    public override string ToString()
    {
      StringBuilder result = new StringBuilder(this.TypeName);

      if (this.GenericParameters != null && this.GenericParameters.Count > 0)
      {
        result.Append("<");
        for (int i = 0; i < this.GenericParameters.Count; i++)
        {
          result.Append(this.GenericParameters[i].ToString());
          if (i < this.GenericParameters.Count - 1)
          {
            result.Append(", ");
          }
        }

        result.Append(">");
      }

      return result.ToString();
    }
  }
}
