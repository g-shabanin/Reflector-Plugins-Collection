namespace Reflector.Sequence
{
  using System.Collections.Generic;
  using Reflector.CodeModel;

  /// <summary>
  /// Implements a enumerator like pattern in the Types property to enumerate over all the
  /// types that are found in the given assembly list.
  /// </summary>
  internal sealed class FastTypeEnumerator
  {
    /// <summary>
    /// Contains a reference to the list of assemblies.
    /// </summary>
    private IList<IAssembly> assemblyList;

    /// <summary>
    /// Initializes a new instance of the <see cref="FastTypeEnumerator"/> class.
    /// </summary>
    /// <param name="assemblyList">The assembly list.</param>
    internal FastTypeEnumerator(IList<IAssembly> assemblyList)
    {
      this.assemblyList = assemblyList;
    }

    /// <summary>
    /// Gets the types.
    /// </summary>
    /// <value>The types.</value>
    internal IEnumerable<ITypeDeclaration> Types
    {
      get
      {
        foreach (IAssembly assembly in this.assemblyList)
        {
          foreach (IModule module in assembly.Modules)
          {
            foreach (ITypeDeclaration typeDeclaration in module.Types)
            {
              yield return typeDeclaration;
              foreach (ITypeDeclaration nestedType in NestedTypes(typeDeclaration))
              {
                yield return nestedType;
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// Detects the nested types.
    /// </summary>
    /// <param name="typeDeclaration">The type declaration.</param>
    /// <returns>An IEnumerable of ITypeDeclaration with the nested types.</returns>
    private static IEnumerable<ITypeDeclaration> NestedTypes(ITypeDeclaration typeDeclaration)
    {
      foreach (ITypeDeclaration nestedType in typeDeclaration.NestedTypes)
      {
        yield return nestedType;
        foreach (ITypeDeclaration nestedNestedType in NestedTypes(nestedType))
        {
          yield return nestedNestedType;
        }
      }
    }
  }
}