/*
 * Created by SharpDevelop.
 * User: dehalleux
 * Date: 3/06/2004
 * Time: 13:07
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections;
using System.IO;
namespace Reflector.Graph.Graphs
{
	using QuickGraph;
	using QuickGraph.Collections;
	using QuickGraph.Providers;
	using QuickGraph.Concepts;
	using QuickGraph.Representations;
	using QuickGraph.Concepts.Collections;
	using QuickGraph.Concepts.Traversals;
	using QuickGraph.Concepts.Predicates;
	using Reflector;
	using Reflector.CodeModel;
//	using QuickGraph.Algorithms.Ranking;
	using QuickGraph.Predicates;
    using QuickGraph.Algorithms.Ranking;
	
		
	/// <summary>
	/// Description of BugRankPopulator.	
	/// </summary>
	public class TypeRankPopulator
	{
		private StringWriter log = null;
		private BidirectionalGraph graph=null;
		private IAssembly assembly=null;
		private Hashtable typeVertices = new Hashtable();
		private VertexCollection assemblyVertices = new VertexCollection();
		private PageRankAlgorithm pageRank = null;

		private bool linkInherance=true;
		private bool linkMethodSignature=true;
		private bool linkProperty=true;
		private bool linkField=true;
		private bool linkMethodBody=true;
		
		public TypeRankPopulator()
		{}

		#region Populator options
		public bool LinkInherance
		{
			get
			{
				return this.linkInherance;
			}
			set
			{
				this.linkInherance=value;
			}
		}

		public bool LinkMethodSignature
		{
			get
			{
				return this.linkMethodSignature;
			}
			set
			{
				this.linkMethodSignature=value;
			}
		}

		public bool LinkField
		{
			get
			{
				return this.linkField;
			}
			set
			{
				this.linkField=value;
			}
		}

		public bool LinkMethodBody
		{
			get
			{
				return this.linkMethodBody;
			}
			set
			{
				this.linkMethodBody=value;
			}
		}
		public bool LinkProperty
		{
			get
			{
				return this.linkProperty;
			}
			set
			{
				this.linkProperty=value;
			}
		}
		#endregion
		
		public BidirectionalGraph Graph
		{
			get
			{
				return this.graph;
			}
		}
		
		public PageRankAlgorithm PageRank
		{
			get
			{
				return this.pageRank;
			}
		}
		
		public String Log
		{
			get
			{
				return this.log.ToString();
			}
		}

		public IVertexCollection AssemblyVertices
		{
			get
			{
				return this.assemblyVertices;
			}
		}
		
		public bool ContainsVertex(ITypeDeclaration method)
		{
			return this.typeVertices.Contains(method);
		}

		public void AddEdge(ITypeReference source, ITypeReference target)
		{
			if (target==null)
				return;

			IVertex vs = (IVertex)this.typeVertices[source];
			if (vs==null)
			{
				vs=this.AddType(source);
			}
			IVertex vt = (IVertex)this.typeVertices[target];
			if (vt==null)
			{
				vt=this.AddType(target);
			}

			if (this.graph.ContainsEdge(vs,vt))
				return;
			else
				this.graph.AddEdge(vs,vt);			
		}
		
		public void CreateFromAssembly(IAssembly assembly)
		{
			this.assembly = assembly;			
			this.log = new StringWriter();
			this.typeVertices.Clear();
			this.assemblyVertices.Clear();
			
			this.graph = new BidirectionalGraph(
				new CustomVertexProvider(),
				new EdgeProvider(),
				false);

			if (this.assembly==null)
				return;
			
			// load types from module
			foreach(IModule module in this.assembly.Modules)
				this.loadTypesFromModule(module);
			this.log.WriteLine("Loaded {0} methods", this.graph.VerticesCount);
			
			// create link
			foreach(IModule module in this.assembly.Modules)
				this.createLinksFromModule(module);			
			this.log.WriteLine("Loaded {0} links", this.graph.EdgesCount);
		}

		public void RemoveIsolatedVertices()
		{
			int count;
			VertexCollection vs = new VertexCollection();
			do
			{
				count = this.graph.VerticesCount;
				foreach(IVertex v in this.Graph.Vertices)
				{
					if (this.graph.OutDegree(v)==0 && this.graph.InDegree(v)==0)
						vs.Add(v);
				}
				foreach(IVertex v in vs)
				{
					this.graph.RemoveVertex(v);
					this.assemblyVertices.Remove(v);
				}
				vs.Clear();
			}while(this.graph.VerticesCount!=count);
		}

		public void RemoveSinks()
		{
			int count;
			VertexCollection vs = new VertexCollection();
			do
			{
				count = this.graph.VerticesCount;
				foreach(IVertex v in this.Graph.Vertices)
				{
					if (this.graph.OutDegree(v)==0)
						vs.Add(v);
				}
				foreach(IVertex v in vs)
				{
					this.graph.RemoveVertex(v);
					this.assemblyVertices.Remove(v);
				}
				vs.Clear();
			}while(this.graph.VerticesCount!=count);
		}		

		public void ComputePageRank()
		{
//			this.RemoveIsolatedVertices();
//			this.RemoveSinks();
			this.pageRank = new PageRankAlgorithm(this.Graph);
			this.pageRank.InitializeRanks();
			this.pageRank.Compute();
		}
		
		private void loadTypesFromModule(IModule module)
		{
			foreach(ITypeReference type in module.Types)
			{
				this.assemblyVertices.Add(this.AddType(type));
			}
		}

		protected virtual CustomVertex AddType(ITypeReference type)
		{
			CustomVertex v=(CustomVertex)this.graph.AddVertex();
			v.Value=type;
			this.typeVertices[type]=v;
			this.assemblyVertices.Add(v);

			return v;
		}
		
		private void createLinksFromModule(IModule module)
		{
			foreach(ITypeReference typeRef in module.Types)
			{
				ITypeDeclaration type = typeRef as ITypeDeclaration;
				if (type==null)
					continue;

				if (this.LinkInherance)
				{
					this.AddEdge(type, type.BaseType);
				}

				if (this.LinkField)
				{
					foreach(IFieldDeclaration field in type.Fields)
					{
						ITypeReference fieldType = field.FieldType as ITypeReference;
						if (fieldType==null)
							continue;

						this.AddEdge(type,fieldType);
					}
				}

				if (this.LinkMethodSignature)
				{
					foreach(IMethodDeclaration method in type.Methods)
					{
						ITypeReference returnType = method.ReturnType.Type as ITypeReference;
						if (returnType==null)
							continue;

						this.AddEdge(type, returnType);
						foreach(IParameterDeclaration par in method.Parameters)
						{
							ITypeReference parameterType = par.ParameterType as ITypeReference;
							if(parameterType==null)
								continue;
							this.AddEdge(type,parameterType);
						}
					}
				}

				if (this.LinkMethodBody)
				{
					foreach(IMethodDeclaration method in type.Methods)
					{
						IMethodBody body = method as IMethodBody;
						if (body==null)
							continue;

						foreach(IInstruction il in body.Instructions)
						{
							IMethodReference mr = il.Value as IMethodReference;
							if (mr!=null)
							{
								ITypeReference declaringType = mr.DeclaringType as ITypeReference;
								if (declaringType==null)
									continue;

								this.AddEdge(type,declaringType);
								continue;
							}
							IPropertyReference pr = il.Value as IPropertyReference;
							if (pr!=null)
							{
								ITypeReference declaringType = pr.DeclaringType as ITypeReference;
								if (declaringType==null)
									continue;
								this.AddEdge(type, declaringType);
								continue;
							}
						}
					}
				}

				if (this.LinkProperty)
				{
					foreach(IPropertyDeclaration prop in type.Properties)
					{
						ITypeReference propertyType =prop.PropertyType as ITypeReference;
						if (propertyType==null)
							continue;
						this.AddEdge(type,propertyType);
					}
				}
			}
		}
	}
}
