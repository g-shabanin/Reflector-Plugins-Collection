namespace Reflector.Sequence
{
  using System.Collections.Generic;
  using Reflector.CodeModel;

  /// <summary>
  /// This class implements methods to support the detection of the derived types.
  /// </summary>
  internal class DerivedTypeInformation
  {
    /// <summary>
    /// Contains a reference to the table with derived type information.
    /// </summary>
    private Dictionary<ITypeReference, List<ITypeDeclaration>> table;

    /// <summary>
    /// Initializes a new instance of the <see cref="DerivedTypeInformation"/> class.
    /// </summary>
    /// <param name="assemblyManager">The assembly manager.</param>
    /// <param name="visibility">The visibility.</param>
    public DerivedTypeInformation(IAssemblyManager assemblyManager, IVisibilityConfiguration visibility)
    {
      this.table = new Dictionary<ITypeReference, List<ITypeDeclaration>>();

      IAssembly[] assemblies = new IAssembly[assemblyManager.Assemblies.Count];
      assemblyManager.Assemblies.CopyTo(assemblies, 0);

      FastTypeEnumerator enumerator = new FastTypeEnumerator(assemblies);
      foreach (ITypeDeclaration typeDeclaration in enumerator.Types)
      {
        if (ReflectorHelper.IsVisible(typeDeclaration, visibility))
        {
          ITypeReference baseType = typeDeclaration.BaseType;
          if (baseType != null)
          {
            if (baseType.GenericType != null)
            {
              this.AddToTable(baseType.GenericType, typeDeclaration);
            }
            else
            {
              this.AddToTable(baseType, typeDeclaration);
            }
          }

          foreach (ITypeReference interfaceType in typeDeclaration.Interfaces)
          {
            this.AddToTable(interfaceType, typeDeclaration);
          }
        }
      }
    }

    /// <summary>
    /// Finds the derived types.
    /// </summary>
    /// <param name="typeDeclaration">The type declaration.</param>
    /// <returns>An IEnumerable'1 of ITypeDeclaration instances that are derived from the given type declaration.</returns>
    public IEnumerable<ITypeDeclaration> FindDerivedTypes(ITypeDeclaration typeDeclaration)
    {
      if (!this.table.ContainsKey(typeDeclaration))
      {
        LiveSequence.Common.Logger.Current.Debug("no derived types in current table");
        return new List<ITypeDeclaration>();
      }

      List<ITypeDeclaration> list = this.table[typeDeclaration];
      list.Sort();
      return list;
    }

    /// <summary>
    /// Adds to table.
    /// </summary>
    /// <param name="keyTypeReference">The key type reference.</param>
    /// <param name="typeDeclaration">The type declaration.</param>
    private void AddToTable(ITypeReference keyTypeReference, ITypeDeclaration typeDeclaration)
    {
      if (!this.table.ContainsKey(keyTypeReference))
      {
        this.table.Add(keyTypeReference, new List<ITypeDeclaration>());
      }

      // Get the list that belongs to the given key
      List<ITypeDeclaration> list = this.table[keyTypeReference];

      // Check if list already contains typeDeclaration; otherwise add it.
      if (list.Find(lookupDeclaration => lookupDeclaration == typeDeclaration) == null)
      {
        list.Add(typeDeclaration);
      }
    }
  }
}