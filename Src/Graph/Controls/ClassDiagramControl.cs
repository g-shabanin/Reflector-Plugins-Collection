using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Windows.Forms;
using System;
using System.ComponentModel;
using Reflector.CodeModel;
using System.Reflection;
using Reflector.Browser;
using Microsoft.Glee.Drawing;

namespace Reflector.Graph
{
    internal sealed class ClassDiagramControl : GraphControl
    {
        private StringCollection excludedTypes = new StringCollection();
		private IAssemblyBrowser assemblyBrowser;
		private IAssemblyManager assemblyManager;

		private enum EdgeTypes
		{
			Implements, Extends, References
		}

		public ClassDiagramControl(IServiceProvider serviceProvider) : base(serviceProvider)
        {
			this.assemblyBrowser = (IAssemblyBrowser)serviceProvider.GetService(typeof(IAssemblyBrowser));
			this.assemblyManager = (IAssemblyManager)serviceProvider.GetService(typeof(IAssemblyManager));

            this.excludedTypes.Add("System.Object");
        }

		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);

			if (this.Parent != null)
			{
				this.Translate();
				this.assemblyBrowser.ActiveItemChanged += new EventHandler(assemblyBrowser_ActiveItemChanged);
			}
			else
			{
				this.assemblyBrowser.ActiveItemChanged -= new EventHandler(assemblyBrowser_ActiveItemChanged);
			}
		}

        private void assemblyBrowser_ActiveItemChanged(object sender, EventArgs e)
        {
            if (this.Parent == null)
                return;

			ITypeReference typeReference = this.assemblyBrowser.ActiveItem as ITypeReference;
			if (typeReference == null)
                return;

            this.Translate();
        }

        private void Translate()
        {
            Node node;
            // Edge edge;
			// creating type vertices
			ITypeDeclaration activeType = this.assemblyBrowser.ActiveItem as ITypeDeclaration;
            if (activeType == null)
                return;
            Microsoft.Glee.Drawing.Graph graph = this.CreateGraph("Class Diagram");

            node = this.AddNode(graph, activeType);           

			ITypeReference baseType = activeType.BaseType;
            if (baseType != null)
            {
                this.AddNode(graph, baseType);
                this.AddEdge(graph, activeType, baseType, EdgeTypes.Extends);
			}

            ITypeReferenceCollection interfaceTypes = activeType.Interfaces;
            foreach (ITypeReference interfaceType in interfaceTypes)
            {
                this.AddNode(graph, interfaceType);
                this.AddEdge(graph, activeType, interfaceType, EdgeTypes.Implements);
            }

            IVisibilityConfiguration visibility = new PrivateVisibilityConfiguration();
            DerivedTypeInformation derivedTypeInformation = new DerivedTypeInformation(this.assemblyManager, visibility);
            IEnumerable derivedTypes = derivedTypeInformation.GetDerivedTypes(activeType);
            foreach (ITypeReference derivedType in derivedTypes)
            {
                string derivedTypeFullName = Helper.GetNameWithResolutionScope(derivedType);
                // NOTE: Duplicate types are showing up in derivedTypes.
                if (graph.FindNode(derivedTypeFullName) == null)
				{
                    this.AddNode(graph, derivedType);
			    }

                this.AddEdge(graph, derivedType, activeType,
                    (activeType.Interface) ? EdgeTypes.Implements : EdgeTypes.Extends);
			}

			/// rendering
            this.Viewer.Graph = graph;
        }

        private Node AddNode(Microsoft.Glee.Drawing.Graph graph, 
            ITypeReference type)
        {
            Node node = (Node)graph.AddNode(Helper.GetNameWithResolutionScope(type));
            algorithm_FormatVertex(type, node);
            return node;
        }

        private Edge AddEdge(Microsoft.Glee.Drawing.Graph graph,
            ITypeReference source,
            ITypeReference target,
            EdgeTypes edgeType)
        {
            Edge edge = (Edge)graph.AddEdge(
                Helper.GetNameWithResolutionScope(source),
                Helper.GetNameWithResolutionScope(target));
            algorithm_FormatEdge(edgeType, edge);
            return edge;
        }

		// NOTE: How correct is this?
		private static ITypeReference getReferencedType(IInstruction instruction)
		{
			ITypeReference typeReference = instruction.Value as ITypeReference;
			if (typeReference != null)
			{
				return typeReference;
			}

			IMemberReference memberReference = instruction.Value as IMemberReference;
			if(memberReference != null)
			{
				return memberReference.DeclaringType as ITypeReference;
			}

			return null;
		}

		private class PrivateVisibilityConfiguration : IVisibilityConfiguration
        {
            public bool Assembly { get { return true; } }
            public bool Family { get { return true; } }
            public bool FamilyAndAssembly { get { return true; } }
            public bool FamilyOrAssembly { get { return true; } }
            public bool Private { get { return true; } }
            public bool Public { get { return true; } }
        }

        private void algorithm_FormatEdge(EdgeTypes edgeType, Edge edge)
        {
			switch (edgeType)
			{
				case EdgeTypes.Extends:
                    edge.Attr.ArrowHeadAtTarget = ArrowStyle.Normal;
					break;
				case EdgeTypes.Implements:
                    edge.Attr.ArrowHeadAtTarget = ArrowStyle.Tee;
                    break;
				case EdgeTypes.References:
                    edge.Attr.ArrowHeadAtTarget = ArrowStyle.None;
                    break;
			}
		}

        private string VisibilityToString(IMethodDeclaration method)
        {
            if (method == null)
                return "";
            switch (method.Visibility)
            {
                case MethodVisibility.Public:
                    return "+";
                case MethodVisibility.Private:
                    return "-";
                case MethodVisibility.Family:
                    return "#";
                default:
                    return "~";
            }
        }

        private string GetSetAvailability(IPropertyDeclaration property)
        {
            if (property.GetMethod != null && property.SetMethod != null)
                return "get,set";
            if (property.GetMethod != null)
                return "get";

            return "set";
        }

		private void algorithm_FormatVertex(object value, Node node)
		{
            node.UserData = value;

			ITypeReference typeReference = value as ITypeReference;
			if (typeReference != null)
			{
				formatTypeReference(node, typeReference);
				return;
			}

			IAssemblyReference assemblyName = value as IAssemblyReference;
			if (assemblyName != null)
			{
				formatTypeReference(node, assemblyName);
				return;
			}
		}

		private void formatTypeReference(Node node, IAssemblyReference assemblyName)
		{
			node.Attr.Label = assemblyName.Name + "\n" + assemblyName.Version.ToString();
		}

		private void formatTypeReference(Node node, ITypeReference typeReference)
		{
			string label = typeReference.Name;

			node.Attr.Fillcolor = Color.LightYellow;
          //  node.Attr.Shape = Shape.Record;
            node.Attr.Label = label;

            //GraphvizRecord record = new GraphvizRecord();
            //GraphvizRecordCell recordCell = new GraphvizRecordCell();
            //recordCell.Text = label;
            //record.Cells.Add(recordCell);

            //if (vertex.Value == this.Services.ActiveTypeDeclaration.BaseType)
            //{
            //    // base class
            //    e.VertexFormatter.FillColor = Color.LightSkyBlue;
            //}

            //// Expand the selected type
            //if (vertex.Value == this.Services.AssemblyBrowser.ActiveItem)
            //{
            //    e.VertexFormatter.FillColor = Color.LightSteelBlue;

            //    GraphvizRecordCell methodsCell = new GraphvizRecordCell();
            //    methodsCell.Text = "";
            //    record.Cells.Add(methodsCell);

            //    GraphvizRecordCell propertiesCell = new GraphvizRecordCell();
            //    propertiesCell.Text = "";
            //    record.Cells.Add(propertiesCell);

            //    ITypeDeclaration typeDeclaration = vertex.Value as ITypeDeclaration;
            //    if (typeDeclaration != null)
            //    {
            //        IDictionary methodDictionary = new Hashtable();
            //        foreach (IMethodDeclaration methodDeclaration in typeDeclaration.Methods)
            //        {
            //            if (methodDeclaration.SpecialName)
            //                continue;
            //            if (methodDeclaration.DeclaringType != this.Services.ActiveType)
            //                continue;

            //            string name = methodDeclaration.Name;
            //            if (methodDictionary[name] == null)
            //            {
            //                methodDictionary[name] = methodDeclaration;
            //                string cellText = String.Format("{0} {1}\\n",
            //                        VisibilityToString(methodDeclaration),
            //                        name);
            //                methodsCell.Text += cellText;
            //            }
            //        }

            //        foreach (IPropertyDeclaration propertyDeclaration in typeDeclaration.Properties)
            //        {
            //            if (propertyDeclaration.DeclaringType != this.Services.ActiveType)
            //                continue;

            //            string visibility;
            //            if (propertyDeclaration.GetMethod != null)
            //            {
            //                visibility = VisibilityToString(propertyDeclaration.GetMethod.Resolve());
            //            }
            //            else
            //            {
            //                visibility = VisibilityToString(propertyDeclaration.SetMethod.Resolve());
            //            }

            //            string cellText = String.Format("{0} {1} ({2})\\n",
            //                            visibility,
            //                            propertyDeclaration,
            //                            GetSetAvailability(propertyDeclaration)
            //                            );
            //            propertiesCell.Text += cellText;
            //        }
            //    }
            //}
		}
    }
}
