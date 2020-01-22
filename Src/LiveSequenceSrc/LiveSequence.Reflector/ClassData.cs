namespace Reflector.Sequence
{
  using System;
  using System.Collections.Generic;
  using LiveSequence.Common;
  using LiveSequence.Common.Domain;
  using Reflector.CodeModel;

  /// <summary>
  /// Container class to contain the current class data.
  /// </summary>
  internal sealed class ClassData
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ClassData"/> class.
    /// </summary>
    internal ClassData()
    {
      this.TypeDataList = new List<ClassTypeInfo>();
      this.AdditionalLoadList = new List<ITypeDeclaration>();
    }

    /// <summary>
    /// Gets the type data list.
    /// </summary>
    /// <value>The type data list.</value>
    internal List<ClassTypeInfo> TypeDataList
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets the additional load list.
    /// </summary>
    /// <value>The additional load list.</value>
    internal List<ITypeDeclaration> AdditionalLoadList
    {
      get;
      private set;
    }

    /// <summary>
    /// Adds the new object.
    /// </summary>
    /// <param name="typeDeclaration">The type declaration.</param>
    /// <param name="startType">The start type.</param>
    /// <param name="includeBase">if set to <c>true</c> include base type info.</param>
    /// <returns>
    /// The base type declaration of the given type declaration; null if not within the same namespace or not found.
    /// </returns>
    internal ITypeDeclaration AddNewObject(ITypeDeclaration typeDeclaration, string startType, bool includeBase)
    {
      // Remove the current type from the additional load list.
      if (this.AdditionalLoadList.Find(typeInfo => typeInfo == typeDeclaration) != null)
      {
        this.AdditionalLoadList.Remove(typeDeclaration);
      }

      if (this.TypeDataList.Find(typeInfo => string.Compare(typeInfo.TypeName, typeDeclaration.Name, StringComparison.Ordinal) == 0) != null)
      {
        // Type already added
        Logger.Current.Info("The type has already been added to the list..." + typeDeclaration.ToString());
        return null;
      }

      ITypeDeclaration baseType = null;
      ClassTypeInfo typeData = new ClassTypeInfo
      {
        StartTypeName = startType,
        Namespace = typeDeclaration.Namespace,
        TypeName = typeDeclaration.Name,
        Modifier = ReflectorHelper.DetermineModifier(typeDeclaration)
      };

      // base type
      if (typeDeclaration.BaseType != null && string.Compare(ReflectorHelper.GetNameWithResolutionScope(typeDeclaration.BaseType), "System.Object", StringComparison.Ordinal) != 0)
      {
        baseType = typeDeclaration.BaseType.Resolve();
        ClassTypeInfo baseTypeData = new ClassTypeInfo
        {
          StartTypeName = startType,
          Namespace = baseType.Namespace,
          TypeName = baseType.Name,
          Modifier = ReflectorHelper.DetermineModifier(baseType),
        };

        if (baseType.GenericType != null)
        {
          ITypeReference genericBaseType = baseType.GenericType;
          if (baseType.GenericArguments != null)
          {
            foreach (IType item in baseType.GenericArguments)
            {
              baseTypeData.GenericParameters.Add(new ClassTypeInfo
              {
                StartTypeName = startType,
                Namespace = typeDeclaration.Namespace,
                TypeName = item.ToString(),
                Modifier = ReflectorHelper.DetermineModifier(null)
              });
            }
          }

          baseType = genericBaseType.Resolve();
        }

        typeData.BaseType = baseTypeData;
      }

      // interfaces
      foreach (ITypeReference item in typeDeclaration.Interfaces)
      {
        ITypeDeclaration interfaceDeclaration = item.Resolve();
        typeData.Interfaces.Add(new ClassTypeInfo
        {
          StartTypeName = startType,
          Namespace = interfaceDeclaration.Namespace,
          TypeName = ReflectorHelper.GetName(interfaceDeclaration),
          Modifier = ReflectorHelper.DetermineModifier(interfaceDeclaration)
        });
      }

      // generic parameters
      foreach (var item in typeDeclaration.GenericArguments)
      {
        ITypeReference reference = item as ITypeReference;
        if (reference != null)
        {
          typeData.GenericParameters.Add(new ClassTypeInfo
          {
            StartTypeName = startType,
            Namespace = reference.Namespace,
            TypeName = reference.Name,
            Modifier = ReflectorHelper.DetermineModifier(reference.Resolve())
          });
        }
        else
        {
          typeData.GenericParameters.Add(new ClassTypeInfo
          {
            StartTypeName = startType,
            Namespace = typeDeclaration.Namespace,
            TypeName = item.ToString(),
            Modifier = ReflectorHelper.DetermineModifier(null)
          });
        }
      }

      //if (Settings.ShowAssociations)
      //{
      //  // properties
      //  foreach (IPropertyDeclaration item in typeDeclaration.Properties)
      //  {
      //    ITypeReference typeReference = item.PropertyType as ITypeReference;
      //    if (typeReference != null && string.Compare(typeDeclaration.Namespace, typeReference.Namespace, StringComparison.Ordinal) == 0)
      //    {
      //      ClassTypeInfo typeInfo = new ClassTypeInfo
      //      {
      //        StartTypeName = startType,
      //        Namespace = typeReference.Namespace,
      //        TypeName = typeReference.Name,
      //        Modifier = ReflectorHelper.DetermineModifier(typeReference.Resolve())
      //      };
      //      typeData.PropertyList.Add(ReflectorHelper.GetName(item), typeInfo);

      //      // Add type to the Additional load list, if it has not been loaded already.
      //      this.AddAdditionalType(typeReference);
      //    }
      //  }
      //}

      this.TypeDataList.Add(typeData);

      if (includeBase && baseType != null && string.Compare(baseType.Namespace, typeDeclaration.Namespace, StringComparison.Ordinal) == 0)
      {
        // also parse base type...
        return baseType;
      }

      return null;
    }

    /// <summary>
    /// Adds the type of the additional.
    /// </summary>
    /// <param name="typeReference">The type reference.</param>
    private void AddAdditionalType(ITypeReference typeReference)
    {
      if (this.TypeDataList.Find(typeInfo => string.Compare(typeInfo.TypeName, typeReference.Name, StringComparison.Ordinal) == 0) != null)
      {
        // Type already added
        Logger.Current.Info("The type is already in the additional list..." + typeReference.ToString());
        return;
      }

      this.AdditionalLoadList.Add(typeReference.Resolve());
    }
  }
}
