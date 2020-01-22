namespace Reflector.Sequence
{
  using System;
  using System.Globalization;
  using System.IO;
  using System.Text;
  using LiveSequence.Common.Domain;
  using Reflector.CodeModel;

  /// <summary>
  /// Static Helper class for Reflector based methods.
  /// </summary>
  internal static class ReflectorHelper
  {
    /// <summary>
    /// Creates the normalize method definition.
    /// </summary>
    /// <param name="methodDefinition">The method definition.</param>
    /// <returns>A new NormalizeMethodDefinition instance, based on the input.</returns>
    internal static NormalizeMethodDefinition CreateNormalizeMethodDefinition(IMethodDeclaration methodDefinition)
    {
      if (methodDefinition == null)
      {
        throw new ArgumentNullException("methodDefinition");
      }

      ITypeReference typeReference = methodDefinition.DeclaringType as ITypeReference;

      return new NormalizeMethodDefinition(
        typeReference.Name,
        typeReference.Namespace,
        methodDefinition.Name,
        BuildParameterList(methodDefinition.Parameters),
        methodDefinition.ReturnType.Type.ToString(),
        string.Empty);
    }

    /// <summary>
    /// Creates the normalize method definition.
    /// </summary>
    /// <param name="methodReference">The method reference.</param>
    /// <returns>A new NormalizeMethodDefinition instance, based on the input.</returns>
    internal static NormalizeMethodDefinition CreateNormalizeMethodDefinition(IMethodReference methodReference)
    {
      if (methodReference == null)
      {
        throw new ArgumentNullException("methodReference");
      }

      ITypeReference typeReference = methodReference.DeclaringType as ITypeReference;

      return new NormalizeMethodDefinition(
        typeReference.Name,
        typeReference.Namespace,
        methodReference.Name,
        BuildParameterList(methodReference.Parameters),
        methodReference.ReturnType.Type.ToString(),
        string.Empty);
    }

    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <param name="value">The type reference value.</param>
    /// <returns>The string representation of the type name.</returns>
    internal static string GetName(ITypeReference value)
    {
      if (value == null)
      {
        throw new ArgumentNullException("value");
      }

      ITypeCollection genericParameters = value.GenericArguments;
      if (genericParameters.Count > 0)
      {
        using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
        {
          for (int i = 0; i < genericParameters.Count; i++)
          {
            if (i != 0)
            {
              writer.Write(",");
            }

            IType genericParameter = genericParameters[i];
            if (genericParameter != null)
            {
              writer.Write(genericParameter.ToString());
            }
          }

          return value.Name + "<" + writer.ToString() + ">";
        }
      }

      return value.Name;
    }

    /// <summary>
    /// Gets the name with resolution scope.
    /// </summary>
    /// <param name="value">The type reference value.</param>
    /// <returns>A string containing the full name of the type.</returns>
    internal static string GetNameWithResolutionScope(ITypeReference value)
    {
      if (value == null)
      {
        throw new ArgumentNullException("value");
      }

      ITypeReference declaringType = value.Owner as ITypeReference;
      if (declaringType != null)
      {
        return ReflectorHelper.GetNameWithResolutionScope(declaringType) + "+" + ReflectorHelper.GetName(value);
      }

      string namespaceName = value.Namespace;
      if (namespaceName.Length == 0)
      {
        return ReflectorHelper.GetName(value);
      }

      return namespaceName + "." + ReflectorHelper.GetName(value);
    }

    /// <summary>
    /// Determines the modifier.
    /// </summary>
    /// <param name="typeDeclaration">The type declaration.</param>
    /// <returns>The type modifier for the type declaration.</returns>
    internal static string DetermineModifier(ITypeDeclaration typeDeclaration)
    {
      string result = "None";

      if (typeDeclaration == null)
      {
        return result;
      }

      if (typeDeclaration.Abstract && typeDeclaration.Sealed)
      {
        result = "Static";
      }
      else if (typeDeclaration.Interface)
      {
        result = "Interface";
      }
      else if (IsDelegate(typeDeclaration))
      {
        result = "Delegate";
      }
      else if (IsEnumeration(typeDeclaration))
      {
        result = "Enumeration";
      }
      else if (IsValueType(typeDeclaration))
      {
        result = "Struct";
      }
      else if (typeDeclaration.Abstract)
      {
        result = "Abstract";
      }
      else if (typeDeclaration.Sealed)
      {
        result = "Sealed";
      }

      return result;
    }

    /// <summary>
    /// Determines whether the specified value is delegate.
    /// </summary>
    /// <param name="value">The type reference value.</param>
    /// <returns>
    ///  <c>true</c> if the specified value is delegate; otherwise, <c>false</c>.
    /// </returns>
    internal static bool IsDelegate(ITypeReference value)
    {
      if (value != null)
      {
        if ((value.Name == "MulticastDelegate") && (value.Namespace == "System"))
        {
          return false;
        }

        ITypeDeclaration typeDeclaration = value.Resolve();
        if (typeDeclaration == null)
        {
          return false;
        }

        ITypeReference baseType = typeDeclaration.BaseType;
        return baseType != null && baseType.Namespace == "System" && (baseType.Name == "MulticastDelegate" || baseType.Name == "Delegate");
      }

      return false;
    }

    /// <summary>
    /// Determines whether the specified value is enumeration.
    /// </summary>
    /// <param name="value">The type reference value.</param>
    /// <returns>
    ///   <c>true</c> if the specified value is enumeration; otherwise, <c>false</c>.
    /// </returns>
    internal static bool IsEnumeration(ITypeReference value)
    {
      if (value != null)
      {
        ITypeDeclaration typeDeclaration = value.Resolve();
        if (typeDeclaration == null)
        {
          return false;
        }

        ITypeReference baseType = typeDeclaration.BaseType;
        return baseType != null && baseType.Name == "Enum" && baseType.Namespace == "System";
      }

      return false;
    }

    /// <summary>
    /// Determines whether the specified value is a value type.
    /// </summary>
    /// <param name="value">The type reference value.</param>
    /// <returns>
    ///   <c>true</c> if the specified value is a value type; otherwise, <c>false</c>.
    /// </returns>
    internal static bool IsValueType(ITypeReference value)
    {
      if (value != null)
      {
        ITypeDeclaration typeDeclaration = value.Resolve();
        if (typeDeclaration == null)
        {
          return false;
        }

        return typeDeclaration.ValueType;
      }

      return false;
    }

    /// <summary>
    /// Gets the visibility.
    /// </summary>
    /// <param name="value">The event reference value.</param>
    /// <returns>A method visibility for the event.</returns>
    internal static MethodVisibility GetVisibility(IEventReference value)
    {
      IMethodDeclaration addMethod = ReflectorHelper.GetAddMethod(value);
      IMethodDeclaration removeMethod = ReflectorHelper.GetRemoveMethod(value);
      IMethodDeclaration invokeMethod = ReflectorHelper.GetInvokeMethod(value);

      if ((addMethod != null) && (removeMethod != null) && (invokeMethod != null))
      {
        if ((addMethod.Visibility == removeMethod.Visibility) && (addMethod.Visibility == invokeMethod.Visibility))
        {
          return addMethod.Visibility;
        }
      }
      else if ((addMethod != null) && (removeMethod != null))
      {
        if (addMethod.Visibility == removeMethod.Visibility)
        {
          return addMethod.Visibility;
        }
      }
      else if ((addMethod != null) && (invokeMethod != null))
      {
        if (addMethod.Visibility == invokeMethod.Visibility)
        {
          return addMethod.Visibility;
        }
      }
      else if ((removeMethod != null) && (invokeMethod != null))
      {
        if (removeMethod.Visibility == invokeMethod.Visibility)
        {
          return removeMethod.Visibility;
        }
      }
      else if (addMethod != null)
      {
        return addMethod.Visibility;
      }
      else if (removeMethod != null)
      {
        return removeMethod.Visibility;
      }
      else if (invokeMethod != null)
      {
        return invokeMethod.Visibility;
      }

      return MethodVisibility.Public;
    }

    /// <summary>
    /// Gets the visibility.
    /// </summary>
    /// <param name="value">The property reference value.</param>
    /// <returns>The method visibilty of the property.</returns>
    internal static MethodVisibility GetVisibility(IPropertyReference value)
    {
      IMethodDeclaration getMethod = ReflectorHelper.GetGetMethod(value);
      IMethodDeclaration setMethod = ReflectorHelper.GetSetMethod(value);

      MethodVisibility visibility = MethodVisibility.Public;

      if ((setMethod != null) && (getMethod != null))
      {
        if (getMethod.Visibility == setMethod.Visibility)
        {
          visibility = getMethod.Visibility;
        }
      }
      else if (setMethod != null)
      {
        visibility = setMethod.Visibility;
      }
      else if (getMethod != null)
      {
        visibility = getMethod.Visibility;
      }

      return visibility;
    }

    /// <summary>
    /// Determines whether the specified value is visible.
    /// </summary>
    /// <param name="value">The field reference value.</param>
    /// <param name="visibility">The visibility.</param>
    /// <returns>
    ///   <c>true</c> if the specified value is visible; otherwise, <c>false</c>.
    /// </returns>
    internal static bool IsVisible(IFieldReference value, IVisibilityConfiguration visibility)
    {
      if (ReflectorHelper.IsVisible(value.DeclaringType, visibility))
      {
        IFieldDeclaration fieldDeclaration = value.Resolve();
        if (fieldDeclaration == null)
        {
          return true;
        }

        switch (fieldDeclaration.Visibility)
        {
          case FieldVisibility.Public:
            return visibility.Public;

          case FieldVisibility.Assembly:
            return visibility.Assembly;

          case FieldVisibility.FamilyOrAssembly:
            return visibility.FamilyOrAssembly;

          case FieldVisibility.Family:
            return visibility.Family;

          case FieldVisibility.Private:
            return visibility.Private;

          case FieldVisibility.FamilyAndAssembly:
            return visibility.FamilyAndAssembly;

          case FieldVisibility.PrivateScope:
            return visibility.Private;
        }

        throw new InvalidOperationException();
      }

      return false;
    }

    /// <summary>
    /// Determines whether the specified value is visible.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="visibility">The visibility.</param>
    /// <returns>
    ///   <c>true</c> if the specified value is visible; otherwise, <c>false</c>.
    /// </returns>
    internal static bool IsVisible(IEventReference value, IVisibilityConfiguration visibility)
    {
      if (ReflectorHelper.IsVisible(value.DeclaringType, visibility))
      {
        switch (ReflectorHelper.GetVisibility(value))
        {
          case MethodVisibility.Public:
            return visibility.Public;

          case MethodVisibility.Assembly:
            return visibility.Assembly;

          case MethodVisibility.FamilyOrAssembly:
            return visibility.FamilyOrAssembly;

          case MethodVisibility.Family:
            return visibility.Family;

          case MethodVisibility.Private:
          case MethodVisibility.PrivateScope:
            return visibility.Private;

          case MethodVisibility.FamilyAndAssembly:
            return visibility.FamilyAndAssembly;
        }

        throw new InvalidOperationException();
      }

      return false;
    }

    /// <summary>
    /// Determines whether the specified value is visible.
    /// </summary>
    /// <param name="value">The property reference value.</param>
    /// <param name="visibility">The visibility.</param>
    /// <returns>
    ///   <c>true</c> if the specified value is visible; otherwise, <c>false</c>.
    /// </returns>
    internal static bool IsVisible(IPropertyReference value, IVisibilityConfiguration visibility)
    {
      if (ReflectorHelper.IsVisible(value.DeclaringType, visibility))
      {
        switch (ReflectorHelper.GetVisibility(value))
        {
          case MethodVisibility.Public:
            return visibility.Public;

          case MethodVisibility.Assembly:
            return visibility.Assembly;

          case MethodVisibility.FamilyOrAssembly:
            return visibility.FamilyOrAssembly;

          case MethodVisibility.Family:
            return visibility.Family;

          case MethodVisibility.Private:
          case MethodVisibility.PrivateScope:
            return visibility.Private;

          case MethodVisibility.FamilyAndAssembly:
            return visibility.FamilyAndAssembly;
        }

        throw new InvalidOperationException();
      }

      return false;
    }

    /// <summary>
    /// Determines whether the specified value is visible.
    /// </summary>
    /// <param name="value">The method reference value.</param>
    /// <param name="visibility">The visibility.</param>
    /// <returns>
    ///   <c>true</c> if the specified value is visible; otherwise, <c>false</c>.
    /// </returns>
    internal static bool IsVisible(IMethodReference value, IVisibilityConfiguration visibility)
    {
      if (ReflectorHelper.IsVisible(value.DeclaringType, visibility))
      {
        IMethodDeclaration methodDeclaration = value.Resolve();
        switch (methodDeclaration.Visibility)
        {
          case MethodVisibility.Public:
            return visibility.Public;

          case MethodVisibility.Assembly:
            return visibility.Assembly;

          case MethodVisibility.FamilyOrAssembly:
            return visibility.FamilyOrAssembly;

          case MethodVisibility.Family:
            return visibility.Family;

          case MethodVisibility.Private:
          case MethodVisibility.PrivateScope:
            return visibility.Private;

          case MethodVisibility.FamilyAndAssembly:
            return visibility.FamilyAndAssembly;
        }

        throw new InvalidOperationException();
      }

      return false;
    }

    /// <summary>
    /// Determines whether the specified value is visible.
    /// </summary>
    /// <param name="value">The type reference value.</param>
    /// <param name="visibility">The visibility.</param>
    /// <returns>
    ///   <c>true</c> if the specified value is visible; otherwise, <c>false</c>.
    /// </returns>
    internal static bool IsVisible(IType value, IVisibilityConfiguration visibility)
    {
      ITypeReference typeReference = value as ITypeReference;
      if (typeReference != null)
      {
        ITypeReference declaringType = typeReference.Owner as ITypeReference;
        if (declaringType != null)
        {
          if (!ReflectorHelper.IsVisible(declaringType, visibility))
          {
            return false;
          }
        }

        ITypeDeclaration typeDeclaration = typeReference.Resolve();
        if (typeDeclaration == null)
        {
          return true;
        }

        switch (typeDeclaration.Visibility)
        {
          case TypeVisibility.Public:
          case TypeVisibility.NestedPublic:
            return visibility.Public;

          case TypeVisibility.Private:
          case TypeVisibility.NestedPrivate:
            return visibility.Private;

          case TypeVisibility.NestedFamilyOrAssembly:
            return visibility.FamilyOrAssembly;

          case TypeVisibility.NestedFamily:
            return visibility.Family;

          case TypeVisibility.NestedFamilyAndAssembly:
            return visibility.FamilyAndAssembly;

          case TypeVisibility.NestedAssembly:
            return visibility.Assembly;

          default:
            throw new NotImplementedException();
        }
      }

      throw new InvalidOperationException();
    }

    /// <summary>
    /// Gets the add method.
    /// </summary>
    /// <param name="value">The event reference value.</param>
    /// <returns>A method declaration for the event's add method.</returns>
    internal static IMethodDeclaration GetAddMethod(IEventReference value)
    {
      IEventDeclaration eventDeclaration = value.Resolve();
      if (eventDeclaration.AddMethod != null)
      {
        return eventDeclaration.AddMethod.Resolve();
      }

      return null;
    }

    /// <summary>
    /// Gets the remove method.
    /// </summary>
    /// <param name="value">The event reference value.</param>
    /// <returns>A method declaration for the event's remove method.</returns>
    internal static IMethodDeclaration GetRemoveMethod(IEventReference value)
    {
      IEventDeclaration eventDeclaration = value.Resolve();
      if (eventDeclaration.RemoveMethod != null)
      {
        return eventDeclaration.RemoveMethod.Resolve();
      }

      return null;
    }

    /// <summary>
    /// Gets the invoke method.
    /// </summary>
    /// <param name="value">The event reference value.</param>
    /// <returns>A method declaration for the event's invoke method.</returns>
    internal static IMethodDeclaration GetInvokeMethod(IEventReference value)
    {
      IEventDeclaration eventDeclaration = value.Resolve();
      if (eventDeclaration.InvokeMethod != null)
      {
        return eventDeclaration.InvokeMethod.Resolve();
      }

      return null;
    }

    /// <summary>
    /// Gets the set method.
    /// </summary>
    /// <param name="value">The property reference value.</param>
    /// <returns>A method declaration containing the set method.</returns>
    internal static IMethodDeclaration GetSetMethod(IPropertyReference value)
    {
      IPropertyDeclaration propertyDeclaration = value.Resolve();
      if (propertyDeclaration.SetMethod != null)
      {
        return propertyDeclaration.SetMethod.Resolve();
      }

      return null;
    }

    /// <summary>
    /// Gets the get method.
    /// </summary>
    /// <param name="value">The property reference value.</param>
    /// <returns>A method declaration containing the get method.</returns>
    internal static IMethodDeclaration GetGetMethod(IPropertyReference value)
    {
      IPropertyDeclaration propertyDeclaration = value.Resolve();
      if (propertyDeclaration.GetMethod != null)
      {
        return propertyDeclaration.GetMethod.Resolve();
      }

      return null;
    }

    /// <summary>
    /// Builds the parameter list.
    /// </summary>
    /// <param name="parameterDefinitionCollection">The parameter definition collection.</param>
    /// <returns>A string representation of the parameter definitions.</returns>
    private static string BuildParameterList(IParameterDeclarationCollection parameterDefinitionCollection)
    {
      if (parameterDefinitionCollection == null)
      {
        return string.Empty;
      }

      var sb = new StringBuilder();
      int paramCount = 1;
      foreach (IParameterDeclaration paramDefinition in parameterDefinitionCollection)
      {
        sb.Append(paramDefinition.ParameterType.ToString());

        if (paramCount < parameterDefinitionCollection.Count)
        {
          sb.Append(",");
          paramCount++;
        }
      }

      return sb.ToString();
    }
  }
}
