using System;
using System.Collections;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using QuickGraph;
using QuickGraph.Providers;
using QuickGraph.Representations;
using Reflector.Framework;
using Reflector.CodeModel;

namespace Reflector.Graph.Controls
{
    internal sealed class TypeStateMachineControl : GraphControl
    {
        private AdjacencyGraph graph;
        private GraphvizAlgorithm graphviz;
        private Hashtable typeVertices = new Hashtable();
        private Hashtable methodEdges = new Hashtable();
        private string fileUrl = null;

        public TypeStateMachineControl()
        {
            this.graph = new AdjacencyGraph(
                new CustomVertexProvider(),
                new CustomEdgeProvider(),
                true);
            this.graphviz = new GraphvizAlgorithm(this.graph);

            this.graphviz.ImageType = NGraphviz.Helpers.GraphvizImageType.Svg;
            this.graphviz.CommonVertexFormat.Font = new Font("Tahoma", 8.25f);
            this.graphviz.CommonVertexFormat.FillColor = Color.LightYellow;
            this.graphviz.CommonVertexFormat.Shape = NGraphviz.Helpers.GraphvizVertexShape.Ellipse;
            this.graphviz.GraphFormat.RankDirection = NGraphviz.Helpers.GraphvizRankDirection.TB;
            this.graphviz.CommonVertexFormat.Style = NGraphviz.Helpers.GraphvizVertexStyle.Filled;
            this.graphviz.CommonEdgeFormat.Font = new Font("Tahoma", 8.25f);
            this.graphviz.FormatVertex += new FormatVertexEventHandler(graphviz_FormatVertex);
            this.graphviz.FormatEdge += new FormatEdgeEventHandler(graphviz_FormatEdge);
        }

        public override Reflector.Framework.ReflectorServices Services
        {
            get
            {
                return base.Services;
            }
            set
            {
                if (this.Services != null)
                {
                    this.Services.AssemblyBrowser.ActiveItemChanged -= new EventHandler(AssemblyBrowser_ActiveItemChanged);
                }
                base.Services = value;
                if (this.Services != null)
                {
                    this.Services.AssemblyBrowser.ActiveItemChanged += new EventHandler(AssemblyBrowser_ActiveItemChanged);
                }
            }
        }

        public override QuickGraph.Concepts.Traversals.IVertexListGraph Graph
        {
            get { return this.graph; }
        }

        void AssemblyBrowser_ActiveItemChanged(object sender, EventArgs e)
        {
            if (this.Parent == null)
                return;
            this.Translate();
        }

        protected override void Translate()
        {
            INamespace activeNamespace = this.Services.ActiveNamespace;
            if (activeNamespace == null)
                return;

            BuildGraph(activeNamespace);

            fileUrl = this.GetType().Name;
            fileUrl = this.graphviz.Write(fileUrl);

            this.NavigateSvg(fileUrl);
        }

        private void BuildGraph(INamespace activeNamespace)
        {
            this.graph.Clear();
            this.typeVertices.Clear();
            this.methodEdges.Clear();

            // add vertices
            foreach (IType type in activeNamespace.Types)
            {
                ITypeDeclaration activeType = type as ITypeDeclaration;
                if (activeType == null)
                    continue;
                if (activeType.Visibility != TypeVisibility.Public)
                    continue;
                AddType(activeType);
            }

            // add edges
            foreach (IType type in activeNamespace.Types)
            {
                ITypeDeclaration activeType = type as ITypeDeclaration;
                if (activeType == null)
                    continue;
                if (activeType.Visibility != TypeVisibility.Public)
                    continue;

                // iterate methods
                foreach (IMethodDeclaration method in activeType.Methods)
                {
                    // must return something
                    if (method.ReturnType == null)
                        continue;
                    // no arguments
                    if (method.Parameters.Count > 0)
                        continue;
                    // no special methods
                    if (method.SpecialName)
                        continue;
                    // no static methods
                    if (method.Static)
                        continue;
                    // add method
                    AddMethod(method);
                }
            }
        }

        private CustomVertex AddType(ITypeReference type)
        {
            if (type == null)
                return null;

            CustomVertex v = this.typeVertices[type] as CustomVertex;
            if (v != null)
                return v;

            v = (CustomVertex)this.graph.AddVertex();
            v.Value = type;
            this.typeVertices.Add(type, v);
            return v;
        }

        private CustomEdge AddMethod(IMethodReference method)
        {
            if (this.methodEdges.Contains(method))
                throw new InvalidOperationException("Method already added");

            // get declaring type
            CustomVertex source = AddType(method.DeclaringType as ITypeReference);
            if (source == null)
                return null;
            // get return type
            CustomVertex target = AddType(method.ReturnType.Type as ITypeReference);
            if (target == null)
                return null;

            CustomEdge edge = (CustomEdge)this.graph.AddEdge(source, target);
            edge.Value = method;
            this.methodEdges.Add(method, edge);
            return edge;
        }

        protected override void OnVertexClick(QuickGraph.Concepts.VertexEventArgs e)
        {
            CustomVertex v = (CustomVertex)e.Vertex;
            this.Services.AssemblyBrowser.ActiveItem = v.Value;
        }

        void graphviz_FormatVertex(object sender, FormatVertexEventArgs e)
        {
            CustomVertex v = (CustomVertex)e.Vertex;
            ITypeReference type = v.Value as ITypeReference;

            e.VertexFormatter.Label = type.ToString();
        }

        void graphviz_FormatEdge(object sender, FormatEdgeEventArgs e)
        {
            CustomEdge edge = (CustomEdge)e.Edge;
            IMethodDeclaration method = edge.Value as IMethodDeclaration;

            e.EdgeFormatter.Label.Value = method.Name;
        }
    }
}
