namespace LiveSequence.Common.Domain
{
  using System;
  using System.Collections.Generic;
  using System.Text;

  /// <summary>
  /// Container for the ObjectInfo object that contains the information of the types that are present in the model.
  /// </summary>
  internal sealed class ExtendedObjectInfo : ObjectInfo
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ExtendedObjectInfo"/> class.
    /// </summary>
    /// <param name="extendedTypeData">The extended type data.</param>
    /// <param name="classGroup">The class group.</param>
    internal ExtendedObjectInfo(ClassTypeInfo classTypeInfo, string classGroup)
      : base(classTypeInfo.Key, classTypeInfo.TypeName)
    {
      this.Group = classGroup;
      this.Namespace = classTypeInfo.Namespace;
      this.Modifier = (TypeModifier)Enum.Parse(typeof(TypeModifier), classTypeInfo.Modifier);
      if (classTypeInfo.BaseType != null)
      {
        this.BaseType = new ExtendedObjectInfo(classTypeInfo.BaseType, classGroup);
      }

      this.Interfaces = new List<string>();
      if (classTypeInfo.Interfaces != null && classTypeInfo.Interfaces.Count > 0)
      {
        foreach (ClassTypeInfo item in classTypeInfo.Interfaces)
        {
          this.Interfaces.Add(item.ToString());
        }
      }

      this.GenericParameters = new List<string>();
      if (classTypeInfo.GenericParameters != null && classTypeInfo.GenericParameters.Count > 0)
      {
        foreach (ClassTypeInfo item in classTypeInfo.GenericParameters)
        {
          this.GenericParameters.Add(item.ToString());
        }
      }
    }

    /// <summary>
    /// Gets the group.
    /// </summary>
    /// <value>The class model group.</value>
    public string Group { get; private set; }

    /// <summary>
    /// Gets the namespace.
    /// </summary>
    /// <value>The namespace.</value>
    public string Namespace { get; private set; }

    /// <summary>
    /// Gets the modifier.
    /// </summary>
    /// <value>The modifier.</value>
    public TypeModifier Modifier { get; private set; }

    /// <summary>
    /// Gets the type of the base.
    /// </summary>
    /// <value>The type of the base.</value>
    public ExtendedObjectInfo BaseType { get; private set; }

    /// <summary>
    /// Gets the interfaces.
    /// </summary>
    /// <value>The interfaces.</value>
    public IList<string> Interfaces { get; private set; }

    /// <summary>
    /// Gets the generic parameters.
    /// </summary>
    /// <value>The generic parameters.</value>
    public IList<string> GenericParameters { get; private set; }

    /// <summary>
    /// Gets the full name.
    /// </summary>
    /// <value>The full name.</value>
    public string FullName
    {
      get
      {
        return this.Namespace + "." + this.ToString();
      }
    }

    /// <summary>
    /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:OpenSequence.Xps.Data.ExtendedObjectInfo"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"/> that represents the current <see cref="T:OpenSequence.Xps.Data.ExtendedObjectInfo"/>.
    /// </returns>
    public override string ToString()
    {
      StringBuilder result = new StringBuilder(this.TypeName);

      if (this.GenericParameters != null && this.GenericParameters.Count > 0)
      {
        result.Append("<");
        for (int i = 0; i < this.GenericParameters.Count; i++)
        {
          result.Append(this.GenericParameters[i]);
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
