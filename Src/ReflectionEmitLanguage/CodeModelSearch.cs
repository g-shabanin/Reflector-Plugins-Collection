// ---------------------------------------------------------
// Jonathan de Halleux Reflection Emit Language for Reflector
// Copyright (c) 2007 Jonathan de Halleux. All rights reserved.
// ---------------------------------------------------------
using System;
using System.Collections;
using System.Reflection;

namespace Reflector.CodeModel
{
    internal sealed class CodeModelSearch
    {
        private readonly IAssemblyManager assemblyManager;
        private readonly Hashtable cachedTypes = new Hashtable();
        private readonly Hashtable methodTokens = new Hashtable();
        private FieldInfo tokenField = null;

        public CodeModelSearch(IServiceProvider serviceProvider)
        {
            this.assemblyManager = (IAssemblyManager)serviceProvider.GetService(typeof(IAssemblyManager));
        }

        public ITypeDeclaration FindType(Type type)
        {
            return FindType(type.FullName);
        }

        public ITypeDeclaration FindType(string typeFullName)
        {
            ITypeDeclaration cachedType = (ITypeDeclaration)this.cachedTypes[typeFullName];
            if (cachedType != null)
                return cachedType;

            foreach (IAssembly assembly in this.assemblyManager.Assemblies)
            {
                foreach (IModule module in assembly.Modules)
                {
                    foreach (ITypeDeclaration type in module.Types)
                    {
                        if (Helper.GetNameWithResolutionScope(type) == typeFullName)
                        {
                            cachedTypes.Add(typeFullName, type);
                            return type;
                        }
                    }
                }
            }

            return null;
        }

        public IPropertyDeclaration FindProperty(ITypeDeclaration type, string propertyName)
        {
            propertyName = propertyName.ToLower();

			if (type != null)
			{
				foreach (IPropertyDeclaration property in type.Properties)
				{
					if (property.Name.ToLower() == propertyName)
					{
						return property;
					}
				}
			}

            return null;
        }

        public IFieldDeclaration FindField(ITypeDeclaration type, string fieldName)
        {
            fieldName = fieldName.ToLower();

			if (type != null)
			{
				foreach (IFieldDeclaration field in type.Fields)
				{
					if (field.Name.ToLower() == fieldName)
					{
						return field;
					}
				}
			}

            return null;
        }

        public int FindToken(IMethodDeclaration method)
        {
            object otoken = this.methodTokens[method];
            int token;

            if (otoken != null)
                token = (int)otoken;
            else
            {
                if (tokenField == null)
                {
                    foreach (FieldInfo field in method.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                    {
                        if (field.FieldType == typeof(int))
                        {
                            tokenField = field;
                            break;
                        }
                    }
                }

                token = (int)tokenField.GetValue(method);
                this.methodTokens.Add(method, token);
            }
            return token;
        }

        private bool AreParameterMatching(ParameterInfo[] parameters, IParameterDeclarationCollection iparameters)
        {
            if (parameters.Length != iparameters.Count)
                return false;

            for (int i = 0; i < parameters.Length; ++i)
            {
                ParameterInfo parameter = parameters[i];
                IParameterDeclaration iparameter = iparameters[i].Resolve();

                if (parameter.Name != iparameters[i].Name)
                    return false;
                if (!AreMatching(parameter.ParameterType, iparameter.ParameterType))
                    return false;
            }

            return true;
        }

        private bool AreMatching(Type type, IType itype)
        {
            IArrayType arrayType = itype as IArrayType;
            if (arrayType != null)
            {
                if (!type.IsArray)
                    return false;

                return AreMatching(type.GetElementType(), arrayType.ElementType);
            }
            IReferenceType referenceType = itype as IReferenceType;
            if (referenceType != null)
            {
                if (!type.IsByRef)
                    return false;
                return AreMatching(type, referenceType.ElementType);
            }
            else
            {
                return type.Name == Helper.GetName(itype as ITypeReference);
            }
        }
    }
}

