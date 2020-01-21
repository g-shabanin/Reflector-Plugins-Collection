// ---------------------------------------------------------
// Lutz Roeder's .NET Reflector
// Copyright (c) 2000-2004 Lutz Roeder. All rights reserved.
// http://www.aisto.com/roeder
// ---------------------------------------------------------
namespace Reflector
{
	using System;
	using System.Collections;
	using Reflector.CodeModel;

	internal sealed class TypeEnumerator : IEnumerator //, IPercentComplete
	{
		private int assemblyIndex;
		private IAssembly[] assemblies;
		private int typeIndex;
		private ArrayList types; 

		public TypeEnumerator(IAssembly[] assemblies)
		{
			this.assemblies = assemblies;
			this.Reset();
		}

		public void Reset()
		{
			this.assemblyIndex = -1;
			this.typeIndex = -1;
			this.types = null;
		}

		public bool MoveNext()
		{
			while ((this.types == null) || ((this.typeIndex + 1) >= this.types.Count))
			{
				if ((this.assemblyIndex + 1) < this.assemblies.Length)
				{
					this.assemblyIndex++;
					IAssembly assembly = this.assemblies[this.assemblyIndex];

					this.typeIndex = -1;
					this.types = new ArrayList(0);
					foreach (IModule module in assembly.Modules)
					{
						foreach (ITypeDeclaration typeDeclaration in module.Types)
						{
							types.Add(typeDeclaration);
							types.AddRange(this.GetNestedTypeList(typeDeclaration));
						}
					}
				}
				else
				{
					return false;
				}
			}

			this.typeIndex++;

			return true;
		}

		public object Current
		{
			get
			{
				return this.types[this.typeIndex];
			}
		}

		public int PercentComplete
		{
			get
			{
				if (this.types != null)
				{
					if (this.types.Count == 0)
					{
						return ((this.assemblyIndex + 1) * 100) / this.assemblies.Length;
					}
	
					int current = (this.types.Count * this.assemblyIndex) + this.typeIndex;
					int size = this.types.Count * this.assemblies.Length;
					return (current * 100) / size;
				}
				
				return 100;
			}
		}

		private ICollection GetNestedTypeList(ITypeDeclaration typeDeclaration)
		{
			ArrayList list = new ArrayList(0);
	
			foreach (ITypeDeclaration nestedType in typeDeclaration.NestedTypes)
			{
				list.Add(nestedType);
				list.AddRange(this.GetNestedTypeList(nestedType));
			}

			return list;
		}
	}
}