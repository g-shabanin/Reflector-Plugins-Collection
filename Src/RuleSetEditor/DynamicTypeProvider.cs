namespace Reflector.RuleSetEditor
{
	using System;
	using System.Collections.Generic;
	using System.Text;          
	using System.Workflow.ComponentModel.Compiler;
	using System.Reflection;          

    /// <summary>
    /// Provides access to types from the current AppDomain.
    /// </summary>
    internal class DynamicTypeProvider : ITypeProvider
    {
        /// <summary>
        /// Gets the <see cref="T:System.Type"/> of the named entity.
        /// </summary>
        /// <param name="name">A string that contains the name of the entity.</param>
        /// <param name="throwOnError">A value that indicates whether to throw an exception if <paramref name="name"/> is not resolvable.</param>
        /// <returns>
        /// The <see cref="T:System.Type"/> of the named entity.
        /// </returns>
        public Type GetType(string name, bool throwOnError)
        {
            foreach (Type type in GetTypes())
            {
                if (type.FullName == name)
                {
                    return type;
                }
            }

            if (throwOnError)
            {
                throw new TypeLoadException();
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="T:System.Type"/> of the named entity.
        /// </summary>
        /// <param name="name">A string that contains the name of the entity.</param>
        /// <returns>
        /// The <see cref="T:System.Type"/> of the named entity.
        /// </returns>
        public Type GetType(string name)
        {
            return GetType(name, false);
        }

        /// <summary>
        /// Creates and returns an array which contains each <see cref="T:System.Type"/> known to this instance.
        /// </summary>
        /// <returns>
        /// An array which contains each <see cref="T:System.Type"/> known to this instance.
        /// </returns>
        public Type[] GetTypes()
        {
            List<Type> result = new List<Type>();

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                result.AddRange(assembly.GetTypes());
            }

            return result.ToArray();
        }

        /// <summary>
        /// Temporary assembly that is generated during the compilation process to validate the types in the active project that is being compiled.
        /// </summary>
        /// <value></value>
        /// <remarks>Not implemented.</remarks>
        /// <returns>The local <see cref="T:System.Reflection.Assembly"/> defined by this instance.</returns>
        public System.Reflection.Assembly LocalAssembly
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a collection of all assemblies referenced by the <see cref="T:System.Type"/>.
        /// </summary>
        /// <value></value>
        /// <remarks>Not implemented.</remarks>
        /// <returns>A collection of all assemblies referenced by the <see cref="T:System.Type"/>.</returns>
        public ICollection<System.Reflection.Assembly> ReferencedAssemblies
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.IDictionary"/> of load error exceptions keyed by the <see cref="T:System.Object"/> causing the <see cref="T:System.Exception"/>.
        /// </summary>
        /// <value></value>
        /// <remarks>Not implemented.</remarks>
        /// <returns>An <see cref="T:System.Collections.IDictionary"/> of load error exceptions keyed by the <see cref="T:System.Object"/> causing the <see cref="T:System.Exception"/>.</returns>
        public IDictionary<object, Exception> TypeLoadErrors
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Occurs when the collection <see cref="P:System.Workflow.ComponentModel.Compiler.TypeProvider.TypeLoadErrors"/> is modified.
        /// </summary>
        /// <remarks>Not implemented.</remarks>
        public event EventHandler TypeLoadErrorsChanged;

        /// <summary>
        /// Occurs when the types in the type provider that implements this interface change. This can happen when an assembly or <see cref="T:System.CodeDom.CodeCompileUnit"/> is added or removed from type provider.
        /// </summary>
        /// <remarks>Not implemented.</remarks>
        public event EventHandler TypesChanged;

        /// <summary>
        /// Raises the <see cref="E:TypeLoadErrorsChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnTypeLoadErrorsChanged(EventArgs e)
        {
            if (this.TypeLoadErrorsChanged != null)
            {
                this.TypeLoadErrorsChanged(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:TypesChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnTypesChanged(EventArgs e)       
        {                                                            
            if (this.TypesChanged != null)                  
            {                                                        
                this.TypesChanged(this, e);                                                                                      
            }                                                                                                                             
        }                                                                                                                                 
    }                                                                
}                                                                    
