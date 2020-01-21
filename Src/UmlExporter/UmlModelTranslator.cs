namespace Reflector.UmlExporter
{
	using System;
	using System.Collections;
	using System.Text;
	using Reflector.CodeModel;

	/// <summary>
	/// The UmlModelTranslator is responsible for creating a Uml/Xmi model of an
    /// assembly selected in Reflector. The reflector classes are used to
    /// inspect the assembly and construct appropriate model elements. The 
    /// details can be retrieved from the Model property once the load is
    /// complete.
    /// </summary>
	internal class UmlModelTranslator
    {
        private readonly ElementCache _Cache;
        private readonly UmlModel _Model;

		public static void Translate(IAssembly source, UmlModel target)
		{
			UmlModelTranslator translator = new UmlModelTranslator(target);
			translator.Load(source);
		}

        internal UmlModelTranslator(UmlModel target)
        {
            _Model = target;
            _Cache = new ElementCache(Model, this);

            // Initialise with elements I will always want
            _Model.Add(UmlStereotype.REALIZE);
        }

        private void Load(IAssembly assembly)
        {
            Cache.FullyLoad(assembly);
            UmlOwner owner = Model;

			foreach (IModule module in assembly.Modules)
            {
                foreach (ITypeDeclaration type in module.Types)
                {
                    // Fetch the containing package...
                    UmlOwner package = LoadPackage(type.Namespace);
                    if (package == null) owner = Model;
                    else owner = package;

                    Load(type);
                }
            }
        }

        /// <summary>
        /// Loads a .NET namespace as a UML Package.
        /// </summary>
        /// <param name="fullname"></param>
        /// <returns></returns>
        private UmlOwner LoadPackage(string fullname)
        {
            UmlPackage package = (UmlPackage) this.Cache.Locate(fullname, false);
			if (package != null)
			{
				return package;
			}

            UmlOwner owner = Model;
            string[] parts = fullname.Split(new char[] { '.' });
            string currentPackage = "";            

            foreach (string ending in parts)
            {
				if (ending.Length > 0)
				{
					currentPackage += ending; // Build the full name of each package.

					if (currentPackage.Length > 0) // If this requires a package.
					{
						// Try to see if we already have this package.
						package = (UmlPackage)this.Cache.Locate(currentPackage, false);
						if (package == null) // No package there...
						{
							package = new UmlPackage();
							package.Name = ending;
							owner.Add(package); // Add to package structure in UML.
							owner = package;
							Cache.Elements.Add(currentPackage, package);
						}
						else
						{
							owner = package;
						}

						currentPackage += ".";
					}
				}
            }

            if (currentPackage.Length == 0) return Model;
            return package;
        }

		/// <summary>The Cache that the Loader uses to store ModelElements it has loaded.</summary>
		/// <remarks>
		/// The cache stores all of the elements loaded to allow
		/// references to be setup. When an element is created it
		/// is added to the Cache. When a reference is required the 
		/// type is loaded either directly or from this cache.
		/// </remarks>
		private ElementCache Cache
		{
			get { return _Cache; }
		}

		/// <summary>This is the model object. The loader will populate this with elements.</summary>
		private UmlModel Model
		{
			get { return _Model; }
		} 

        /// <summary>Load the details for the declared type. This will search the ElementCache for the container in which this type belongs.</summary>
        /// <param name="type">The type to be loaded.</param>
        private UmlClassifier Load(ITypeDeclaration type)
        {
            // Ignore "<Module>".
			if (type.Name != "<Module>")
			{
				UmlOwner owner;
				if (type.Owner is ITypeDeclaration)
				{
					owner = this.Load(type.Owner as ITypeDeclaration) as UmlClassifier;
				}
				else
				{
					owner = this.LoadPackage(type.Namespace);
				}

				return Load(type, owner);
			}

			return null;
        }

        private UmlClassifier Load(ITypeDeclaration type, UmlOwner owner)
        {
            UmlClassifier data;
            
			// Check if it is already there.
			data = (UmlClassifier) this.Cache.Locate(type, false);
			if (data != null)
			{
				return data;
			}
   
            // Get a new UmlClassifier.
            data = CreateClassifierFor(type);

            // Load common features.
            data.Name = type.Name;
            if (type.GenericArguments != null)
            {
                for (int i = 0; i < type.GenericArguments.Count; i++)
                {
                    if (i == 0) data.Name += "<";
                    data.Name += type.GenericArguments[i];
                    if (i < type.GenericArguments.Count - 1) data.Name += ", ";
                    if (i == type.GenericArguments.Count - 1) data.Name += ">";
                }
            }
            data.IsAbstract = type.Abstract;

            // NOTE: Add to cache as early as possible to correct circular references
            this.Cache.Elements.Add(type, data);

			LoadVisibility(type.Visibility, data);
			this.LoadClassifierFeatures(type, data);
			this.LoadInheritance(type, data);

            // Load into model and Cache
            owner.Add(data);

            return data;
        }

        private void LoadInheritance(ITypeDeclaration typeDeclaration, UmlClassifier data)
        {
			foreach (ITypeReference interfaceTypeReference in typeDeclaration.Interfaces)
            {
                UmlAbstraction abstraction = new UmlAbstraction();
				abstraction.ModelElementStereotype = UmlTypedElementType.Create(UmlStereotype.REALIZE);
				abstraction.DependencyClient = UmlTypedElementType.Create(data);
				abstraction.DependencySupplier = UmlTypedElementType.Create(Load(interfaceTypeReference.Resolve()));
				this.Model.Add(abstraction);
            }

			if (typeDeclaration.BaseType != null)
            {
                UmlGeneralization generalization = new UmlGeneralization();
				generalization.GeneralizationChild = UmlTypedElementType.Create(data);
				generalization.GeneralizationParent = UmlTypedElementType.Create(Load(typeDeclaration.BaseType.Resolve()));
				this.Model.Add(generalization);
            }
        }

        /// <summary>Create the correct UmlClassifier for the indicated type. This is a factory method used to construct UmlClassifiers.</summary>
        /// <param name="type">The type to create a UmlClassifier for.</param>
        /// <returns>A new UmlClassifier object.</returns>
        private static UmlClassifier CreateClassifierFor(ITypeDeclaration type)
        {
            UmlClassifier data;
            if (type.Interface)
            {
                data = new UmlInterface();
            }
            else if ((type.BaseType == null) && (type.Name == "Object"))
            {
                data = new UmlClassifier();
                data.IsRoot = true;
            }
            else if (type.BaseType != null && (type.BaseType.Name == "ValueType" || type.BaseType.Name == "Enum"))
            {
                data = new UmlDataType();
                data.IsLeaf = true;
            }
            else
            {
                data = new UmlClassifier();
            }
            return data;
        }

        private static void LoadVisibility(TypeVisibility visibility, UmlVisibleModelElement data)
        {
            switch (visibility)
            {
                case TypeVisibility.Public: data.Visibility = UmlVisibleModelElement.PUBLIC; break;
                case TypeVisibility.Private: data.Visibility = UmlVisibleModelElement.PRIVATE; break;
                case TypeVisibility.NestedFamily: data.Visibility = UmlVisibleModelElement.PROTECTED; break;
                default: data.Visibility = UmlVisibleModelElement.PACKAGE; break;
            }
        }

        private static void LoadVisibility(MethodVisibility visibility, UmlVisibleModelElement data)
        {
            switch (visibility)
            {
                case MethodVisibility.Public: data.Visibility = UmlVisibleModelElement.PUBLIC; break;
                case MethodVisibility.Private: data.Visibility = UmlVisibleModelElement.PRIVATE; break;
                case MethodVisibility.Family:
                case MethodVisibility.FamilyAndAssembly:
                case MethodVisibility.FamilyOrAssembly: data.Visibility = UmlVisibleModelElement.PROTECTED; break;
                default: data.Visibility = UmlVisibleModelElement.PACKAGE; break;
            }
        }

        private static void LoadVisibility(FieldVisibility visibility, UmlVisibleModelElement data)
        {
            switch (visibility)
            {
                case FieldVisibility.Public: data.Visibility = UmlVisibleModelElement.PUBLIC; break;
                case FieldVisibility.Private: data.Visibility = UmlVisibleModelElement.PRIVATE; break;
                case FieldVisibility.Family:
                case FieldVisibility.FamilyAndAssembly:
                case FieldVisibility.FamilyOrAssembly: data.Visibility = UmlVisibleModelElement.PROTECTED; break;
                default: data.Visibility = UmlVisibleModelElement.PACKAGE; break;
            }
        }

        private void LoadClassifierFeatures(ITypeDeclaration type, UmlClassifier data)
        {
            bool loadAll = Cache.IsFullyLoad(GetAssembly(type));

            foreach (IMethodDeclaration method in type.Methods)
            {
                UmlOperation op = new UmlOperation();
                op.Name = method.Name;
                if(false == loadAll && (method.Visibility != MethodVisibility.Public && method.Visibility != MethodVisibility.Family))
                {
                    continue; // Dont process this method
                }

                LoadVisibility(method.Visibility, data);

                op.IsAbstract = method.Abstract;
                if (method.Static) op.OwnerScope = UmlClassifierFeature.STATIC;

                LoadOperation(op, method, loadAll);

                data.ClassifierFeature.Add(op);
            }

            foreach (IFieldDeclaration field in type.Fields)
            {
                UmlAttribute attr = new UmlAttribute();
                attr.Name = field.Name;
                LoadVisibility(field.Visibility, attr);
                if (field.Static)
                    attr.OwnerScope = UmlClassifierFeature.STATIC;

                if (false == loadAll &&
                    (field.Visibility != FieldVisibility.Public && field.Visibility != FieldVisibility.Family))
                {
                    continue; // Dont process this method
                }

                if (loadAll)
                {
                    // Only load types for the current assembly.
                    TryLoadType(attr, field.FieldType);
                }

                data.ClassifierFeature.Add(attr);
            }
        }

        private IAssembly GetAssembly(ITypeDeclaration type)
        {
            object owner = type.Owner;
            while (false == owner is IAssembly)
            {
                if (owner is ITypeDeclaration)
                {
                    owner = (owner as ITypeDeclaration).Owner;
                }
                else if (owner is IModule)
                {
                    owner = (owner as IModule).Assembly;
                }
                else
                {
                    throw new ApplicationException("I dont know how to handle this owner type");
                }
            }

            return owner as IAssembly;
        }

        private void LoadOperation(UmlOperation op, IMethodDeclaration method, bool loadAll)
        {
            if (method.ReturnType != null)
            {
                UmlParameter returnValue;
                returnValue = new UmlParameter();
                returnValue.Name = "return";
                returnValue.Kind = UmlParameter.RETURN;

                if(loadAll)
                    TryLoadType(returnValue, method.ReturnType.Type);

                op.Parameters.Add(returnValue);
            }

            foreach (IParameterDeclaration parameter in method.Parameters)
            {
                UmlParameter umlParam = new UmlParameter();
                umlParam.Name = parameter.Name;

                if (parameter.ParameterType is IReferenceType)
                {
                    umlParam.Kind = UmlParameter.INOUT;
                }

                if(loadAll)
                    TryLoadType(umlParam, parameter.ParameterType);

                op.Parameters.Add(umlParam);
            }
        }

        private void TryLoadType(IUmlTyped typedValue, IType type)
        {
			ITypeReference typeReference = type as ITypeReference;
			if (typeReference != null)
            {
                ITypeDeclaration typeDeclaration = typeReference.Resolve();
				if (typeDeclaration != null)
				{
					UmlModelElement element = this.Cache.Locate(typeDeclaration, true);
					typedValue.TypedElementType = UmlTypeReference.Create(element);
				}
				else
				{
					UmlModelElement element = this.Cache.Locate(typeReference, true);
					typedValue.TypedElementType = UmlTypeReference.Create(element);
				}
            }
        }

		private class ElementCache
		{
			private readonly ArrayList fullyLoad = new ArrayList(); // new List<IAssembly>
			private readonly UmlModel model;
			private readonly UmlModelTranslator loader;
			public readonly Hashtable Elements = new Hashtable(); // new Dictionary<object, UmlModelElement>();

			public ElementCache(UmlModel model, UmlModelTranslator loader)
			{
				this.model = model;
				this.loader = loader;
			}

			public UmlModelElement Locate(object identifier, bool create)
			{
				if (this.Elements.ContainsKey(identifier))
				{
					UmlModelElement value = (UmlModelElement) Elements[identifier];
					return value;
				}

				if (create)
				{
					ITypeDeclaration typeDeclaration = identifier as ITypeDeclaration;
					if (typeDeclaration != null)
					{
						return this.loader.Load(typeDeclaration) as UmlModelElement;
					}
				}

				return null;
			}

			public void FullyLoad(IAssembly assembly)
			{
				this.fullyLoad.Add(assembly);
			}

			public bool IsFullyLoad(IAssembly assembly)
			{
				return this.fullyLoad.Contains(assembly);
			}
		}
	}
}
