namespace Reflector.Sequence
{
  using System;
  using System.Collections.Generic;
  using System.Globalization;
  using LiveSequence.Common;
  using LiveSequence.Common.Domain;
  using Reflector.CodeModel;

  /// <summary>
  /// This object contains the methods that are used to populate the sequence's call stack.
  /// It's quite simular to the AssemblyParser in LiveSequence.
  /// </summary>
  internal sealed class InstructionDataPopulator
  {
    /// <summary>
    /// Contains a reference to Reflector's assembly manager.
    /// </summary>
    private readonly IAssemblyManager assemblyManager;

    /// <summary>
    /// Contains a reference to the DerivedTypeInformation.
    /// </summary>
    private DerivedTypeInformation derivedTypeInformation;

    /// <summary>
    /// Contains a reference to an instance of the CodeConverter to convert an instruction to an opcode name.
    /// </summary>
    private CodeConverter converter = new CodeConverter();

    /// <summary>
    /// Contains a reference to the list of method's that are already in the graph.
    /// </summary>
    /// <remarks>It is used to prevent infinite loops.</remarks>
    private List<string> methodGraph = new List<string>();

    /// <summary>
    /// Contains a reference to the list of types that are already in the graph.
    /// </summary>
    /// <remarks>It is used to prevent infinite loops.</remarks>
    private List<string> typeGraph = new List<string>();

    /// <summary>
    /// Initializes a new instance of the <see cref="InstructionDataPopulator"/> class.
    /// </summary>
    /// <param name="assemblyManager">The assembly manager.</param>
    internal InstructionDataPopulator(IAssemblyManager assemblyManager)
    {
      this.assemblyManager = assemblyManager;
      IVisibilityConfiguration visibility = new OmniVisibilityConfiguration();
      this.derivedTypeInformation = new DerivedTypeInformation(this.assemblyManager, visibility);
    }

    /// <summary>
    /// Gets or sets the data.
    /// </summary>
    /// <value>The method data.</value>
    private MethodData Data
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the class data.
    /// </summary>
    /// <value>The class data.</value>
    private ClassData ClassData
    {
      get;
      set;
    }

    /// <summary>
    /// Performs a cleanup.
    /// </summary>
    internal void CleanUp()
    {
      this.Data = new MethodData();
      this.ClassData = new ClassData();
    }

    /// <summary>
    /// Builds the model from the namespace.
    /// </summary>
    /// <param name="typeNamespace">The type namespace.</param>
    /// <returns>A list of new ClassModelData objects with the relevant data for the model.</returns>
    internal IList<ClassModelData> BuildModelFromNamespace(INamespace typeNamespace)
    {
      if (typeNamespace == null)
      {
        throw new ArgumentNullException("typeNamespace");
      }

      List<ClassModelData> result = new List<ClassModelData>();
      foreach (ITypeDeclaration type in typeNamespace.Types)
      {
        result.Add(this.BuildModelFromType(type));
      }

      return result;
    }

    /// <summary>
    /// Builds the model from the type.
    /// </summary>
    /// <param name="type">The type declaration.</param>
    /// <returns>A new ClassModelData object with the data for the model.</returns>
    internal ClassModelData BuildModelFromType(ITypeDeclaration type)
    {
      if (type == null)
      {
        throw new ArgumentNullException("type");
      }

      // create model by parsing type
      this.ParseType(type, ReflectorHelper.GetNameWithResolutionScope(type), true);

      // also add additional types
      while (this.ClassData.AdditionalLoadList.Count > 0)
      {
        ITypeDeclaration additionalType = this.ClassData.AdditionalLoadList[0];
        this.ParseType(additionalType, ReflectorHelper.GetNameWithResolutionScope(type), false);
      }

      // clear typeGraph
      this.typeGraph.Clear();

      return this.GetClassModelData(ReflectorHelper.GetNameWithResolutionScope(type));
    }

    /// <summary>
    /// Builds the graph from method.
    /// </summary>
    /// <param name="method">The method that is being examined.</param>
    /// <returns>A new instance of SequenceData, with the related sequence data.</returns>
    internal SequenceData BuildGraphFromMethod(IMethodDeclaration method)
    {
      if (method == null)
      {
        throw new ArgumentNullException("method");
      }

      // create graph
      ITypeReference typeReference = method.DeclaringType as ITypeReference;
      NormalizeMethodDefinition normMethod = ReflectorHelper.CreateNormalizeMethodDefinition(method);

      // parse method body
      this.ParseMethodBody(typeReference, method, typeReference.Namespace + "." + typeReference.Name + "." + normMethod.ToString());

      // clear methodGraph
      this.methodGraph.Clear();

      return this.GetSequenceData(normMethod.ToString(), typeReference.Name, typeReference.Namespace);
    }

    /// <summary>
    /// Refreshes the derived type information.
    /// </summary>
    internal void RefreshDerivedTypeInformation()
    {
      IVisibilityConfiguration visibility = new OmniVisibilityConfiguration();
      this.derivedTypeInformation = new DerivedTypeInformation(this.assemblyManager, visibility);
    }

    /// <summary>
    /// Gets the sequence data.
    /// </summary>
    /// <param name="methodName">Name of the method.</param>
    /// <param name="typeName">Name of the type.</param>
    /// <param name="namespaceName">Name of the namespace.</param>
    /// <returns>
    /// A new instance of SequenceData, with the related sequence data.
    /// </returns>
    private SequenceData GetSequenceData(string methodName, string typeName, string namespaceName)
    {
      // here match the methodName in the methodcalllist structure
      // and populate the SequenceData field.
      SequenceData data = new SequenceData(typeName + methodName);
      data.AddObject(typeName);

      this.SelectMethod(methodName, typeName, namespaceName, data);

      return data;
    }

    /// <summary>
    /// Selects the method.
    /// </summary>
    /// <param name="methodName">Name of the method.</param>
    /// <param name="typeName">Name of the type.</param>
    /// <param name="namespaceName">Name of the namespace.</param>
    /// <param name="data">The sequence data.</param>
    private void SelectMethod(string methodName, string typeName, string namespaceName, SequenceData data)
    {
      foreach (MethodCallInfo methodInfo in this.Data.MethodCallList)
      {
        if (methodInfo.StartMethod.Equals(namespaceName + "." + typeName + "." + methodName))
        {
          data.AddObject(methodInfo.MethodCallType);
          data.AddMessage(methodInfo);
          Logger.Current.Info(">> Found method:" + methodInfo.ToString());
        }
      }
    }

    /// <summary>
    /// Parses the method body.
    /// </summary>
    /// <param name="typeReference">The type reference.</param>
    /// <param name="method">The method declaration.</param>
    /// <param name="startMethod">The start method.</param>
    private void ParseMethodBody(ITypeReference typeReference, IMethodDeclaration method, string startMethod)
    {
      if (typeReference == null)
      {
        return;
      }

      IMethodBody methodBody = method.Body as IMethodBody;

      if (methodBody != null)
      {
        Logger.Current.Info(string.Format(CultureInfo.InvariantCulture, ">> ParseMethodBody - Type:{0}, Start Method:{1}", typeReference.Name, startMethod));

        // first add all instructions
        foreach (IInstruction i in methodBody.Instructions)
        {
          string opcodeName = this.Convert(i.Code);

          if (!string.IsNullOrEmpty(opcodeName))
          {
            // avoid certain instructions
            if (Rules.IsValidInstruction(opcodeName))
            {
              IMethodDeclaration childMet = this.Data.AddNewCall(typeReference.Name, method, i.Value, startMethod);

              if (childMet != null)
              {
                string currentMethod = ((ITypeReference)method.DeclaringType).Name + "." + ReflectorHelper.CreateNormalizeMethodDefinition(method).ToString();

                // add current method to the methodGraph...to detect infinite loops
                this.methodGraph.Add(currentMethod);

                string childMethod = ((ITypeReference)childMet.DeclaringType).Name + "." + ReflectorHelper.CreateNormalizeMethodDefinition(childMet).ToString();
                bool methodExists = this.methodGraph.Exists(delegate(string methodName)
                {
                  return methodName.Equals(childMethod);
                });

                // check for infinite loop
                if (methodExists)
                {
                  Logger.Current.Info(string.Format(
                        CultureInfo.InvariantCulture,
                        "Infinite Loop Detected: StartMethod:{0}, CurrentMethod:{1}, ChildMethod:{2}",
                        startMethod,
                        currentMethod,
                        childMethod));
                }
                else
                {
                  this.ParseMethodBody((ITypeReference)childMet.DeclaringType, childMet, startMethod);
                }
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// Converts the specified code.
    /// </summary>
    /// <param name="code">The code value.</param>
    /// <returns>The related opcode's name.</returns>
    private string Convert(int code)
    {
      return this.converter.Convert(code);
    }

    /// <summary>
    /// Gets the class model data.
    /// </summary>
    /// <param name="referenceTypeName">Name of the reference type.</param>
    /// <returns>
    /// A new ClassModelData that contains the model for the type(s).
    /// </returns>
    private ClassModelData GetClassModelData(string referenceTypeName)
    {
      ClassModelData data = new ClassModelData();
      foreach (var typeInfo in this.ClassData.TypeDataList)
      {
        if (string.Compare(typeInfo.StartTypeName, referenceTypeName, StringComparison.Ordinal) == 0 && Rules.IsValidType(typeInfo.FullName))
        {
          // add the type
          data.AddObject(typeInfo);

          // add the base class connection
          if (typeInfo.BaseType != null && Rules.IsValidType(typeInfo.BaseType.FullName) && this.ClassData.TypeDataList.Contains(typeInfo.BaseType))
          {
            data.AddConnector(new ClassConnectorInfo(typeInfo.BaseType, typeInfo, string.Empty));
          }

          // add the property connections
          foreach (var item in typeInfo.PropertyList)
          {
            if (Rules.IsValidType(item.Value.ToString()))
            {
              data.AddConnector(new ClassConnectorInfo(typeInfo, item.Value, item.Key));
            }
          }
        }
      }

      return data;
    }

    /// <summary>
    /// Parses the type.
    /// </summary>
    /// <param name="typeDeclaration">The type declaration.</param>
    /// <param name="startType">The start type.</param>
    /// <param name="includeBaseAndDerivedTypes">if set to <c>true</c> include base and derived types.</param>
    private void ParseType(ITypeDeclaration typeDeclaration, string startType, bool includeBaseAndDerivedTypes)
    {
      if (typeDeclaration == null)
      {
        return;
      }

      // Add currentType and determine base type
      ITypeDeclaration baseTypeDeclaration = this.ClassData.AddNewObject(typeDeclaration, startType, includeBaseAndDerivedTypes);
      if (baseTypeDeclaration != null)
      {
        string currentType = ReflectorHelper.GetNameWithResolutionScope(typeDeclaration);
        this.typeGraph.Add(currentType);

        string baseType = ReflectorHelper.GetNameWithResolutionScope(baseTypeDeclaration);
        bool baseTypeExists = this.typeGraph.Exists(methodName => methodName.Equals(baseType));

        if (baseTypeExists)
        {
          Logger.Current.Info(string.Format(
                CultureInfo.InvariantCulture,
                "Infinite Loop Detected: StartType:{0}, CurrentType:{1}, BaseType:{2}",
                startType,
                currentType,
                baseType));
        }
        else
        {
          this.ParseType(baseTypeDeclaration, startType, includeBaseAndDerivedTypes);
        }
      }

      if (includeBaseAndDerivedTypes)
      {
        // also add derived types....
        foreach (ITypeDeclaration derivedType in this.derivedTypeInformation.FindDerivedTypes(typeDeclaration))
        {
          Logger.Current.Debug(string.Format("Adding derived types: {0}", typeDeclaration.Name));
          this.ClassData.AddNewObject(derivedType, startType, includeBaseAndDerivedTypes);
        }
      }
    }

    /// <summary>
    /// Implementation of the IVisibilityConfiguration to get all accessabilities.
    /// </summary>
    private class OmniVisibilityConfiguration : IVisibilityConfiguration
    {
      /// <summary>
      /// Gets a value indicating whether this <see cref="OmniVisibilityConfiguration"/> is assembly.
      /// </summary>
      /// <value><c>true</c> if assembly; otherwise, <c>false</c>.</value>
      public bool Assembly
      {
        get { return true; }
      }

      /// <summary>
      /// Gets a value indicating whether this <see cref="OmniVisibilityConfiguration"/> is family.
      /// </summary>
      /// <value><c>true</c> if family; otherwise, <c>false</c>.</value>
      public bool Family
      {
        get { return true; }
      }

      /// <summary>
      /// Gets a value indicating whether [family and assembly].
      /// </summary>
      /// <value><c>true</c> if [family and assembly]; otherwise, <c>false</c>.</value>
      public bool FamilyAndAssembly
      {
        get { return true; }
      }

      /// <summary>
      /// Gets a value indicating whether [family or assembly].
      /// </summary>
      /// <value><c>true</c> if [family or assembly]; otherwise, <c>false</c>.</value>
      public bool FamilyOrAssembly
      {
        get { return true; }
      }

      /// <summary>
      /// Gets a value indicating whether this <see cref="OmniVisibilityConfiguration"/> is private.
      /// </summary>
      /// <value><c>true</c> if private; otherwise, <c>false</c>.</value>
      public bool Private
      {
        get { return true; }
      }

      /// <summary>
      /// Gets a value indicating whether this <see cref="OmniVisibilityConfiguration"/> is public.
      /// </summary>
      /// <value><c>true</c> if public; otherwise, <c>false</c>.</value>
      public bool Public
      {
        get { return true; }
      }
    }
  }
}
