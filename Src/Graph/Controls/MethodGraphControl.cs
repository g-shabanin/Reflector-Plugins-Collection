using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System;
using System.ComponentModel;
using QuickGraph;
using QuickGraph.Concepts;
using QuickGraph.Providers;
using QuickGraph.Representations;
using Reflector.CodeModel;
using System.Reflection;
using Reflector.Framework;
using Reflector.Graph.Graphs;
using QuickGraph.Concepts.Traversals;
using QuickGraph.Algorithms.Search;
using QuickGraph.Algorithms;
using QuickGraph.Algorithms.Visitors;
using QuickGraph.Collections;

namespace Reflector.Graph
{
    internal sealed class MethodGraphControl : GraphControl
    {
        private BidirectionalGraph graph = null;
        private Hashtable methodVertices = new Hashtable();
        private ITypeDeclaration type = null;
        private DepthFirstSearchAlgorithm dfs = null;
        private HeightFirstSearchAlgorithm hfs = null;
        private Hashtable componentVertices = new Hashtable();

        public MethodGraphControl()
        {
            this.graph = new BidirectionalGraph(
                new CustomVertexProvider(),
                new CustomEdgeProvider(),
                false
                );
            this.dfs = new DepthFirstSearchAlgorithm(this.graph);
            this.dfs.TreeEdge+=new EdgeEventHandler(dfs_TreeEdge);
            this.hfs = new HeightFirstSearchAlgorithm(this.graph);
            this.hfs.TreeEdge+=new EdgeEventHandler(hfs_TreeEdge);

            this.graphviz = new GraphvizAlgorithm(this.graph);
            this.graphviz.ImageType = GraphvizImageType.Svg;
            this.graphviz.CommonVertexFormat.Shape = GraphvizVertexShape.Box;
            this.graphviz.CommonVertexFormat.Font = new Font("Tahoma", 8.25f);
            this.graphviz.CommonVertexFormat.Style = GraphvizVertexStyle.Filled;

            this.graphviz.GraphFormat.RankDirection = GraphvizRankDirection.LR;
            this.graphviz.FormatVertex += new FormatVertexEventHandler(graphviz_FormatVertex);
        }

        public override QuickGraph.Concepts.Traversals.IVertexListGraph Graph
        {
            get
            {
                return this.graph;
            }
        }

        [Browsable(false)]
        public override ReflectorServices Services
        {
            get
            {
                return base.Services;
            }
            set
            {
                if (this.Services != null)
                {
                    this.graph.Clear();
                    this.methodVertices.Clear();
                    this.Services.AssemblyBrowser.ActiveItemChanged -= new EventHandler(assemblyBrowser_ActiveItemChanged);
                }
                base.Services = value;
                if (this.Services != null)
                {
                    this.Services.AssemblyBrowser.ActiveItemChanged += new EventHandler(assemblyBrowser_ActiveItemChanged);
                }
            }
        }

        private void assemblyBrowser_ActiveItemChanged(object sender, EventArgs e)
        {
            if (this.Parent == null)
            {
                this.graph.Clear();
                this.methodVertices.Clear();
                return;
            }

            this.Translate();
        }

        protected override void Translate()
        {
            ITypeDeclaration type = this.Services.ActiveTypeDeclaration;
            if (type != null)
            {
                this.type = type;
              //  this.BuildGraphFromType();
                Render();
                return;
            }

            IMethodDeclaration method = this.Services.ActiveMethod;
            if (method != null)
            {
                this.type = method.DeclaringType as ITypeDeclaration;
                this.BuildGraphFromMethod(method);
                this.CleanGraph(method);
                Render();
                return;
            }
        }

        private void Render()
        {
            string fileName = this.GetType().Name;
            fileName = this.graphviz.Write(fileName);
            fileName = Path.GetFullPath(fileName);

            this.NavigateSvg(fileName);
        }

        private void CleanGraph(IMethodReference method)
        {
            IVertex v = this.GetVertex(method);

            this.componentVertices.Clear();
            dfs.Initialize();
            hfs.Initialize();
            dfs.Visit(v, int.MaxValue);
            hfs.Visit(v, int.MaxValue);

            VertexCollection vertexToRemove = new VertexCollection();
            foreach (IVertex child in graph.Vertices)
            {
                if (child == v)
                    continue;
                if (this.componentVertices.Contains(child))
                    continue;
                vertexToRemove.Add(child);
            }
            foreach (CustomVertex child in vertexToRemove)
            {
                this.graph.ClearVertex(child);
                this.graph.RemoveVertex(child);
                this.methodVertices.Remove(child.Value);
            }
        }

        private void BuildGraphFromMethod(IMethodReference method)
        {
            IMethodBody body = method as IMethodBody;
            if (body == null)
                return;

            CustomVertex source = this.GetVertex(method);
            if (source == null)
                return;
            foreach (IInstruction il in body.Instructions)
            {
                IMethodReference methodRef = il.Value as IMethodReference;
                if (methodRef == null)
                    continue;
                CustomVertex target = GetVertex(methodRef);
                if (target == null)
                    continue;

                if (graph.ContainsEdge(source, target))
                    continue;

                this.graph.AddEdge(source, target);
            }
        }

        private CustomVertex GetVertex(IMethodReference method)
        {
            if (IsException(method.DeclaringType as ITypeReference))
                return null; ;

            CustomVertex target = (CustomVertex)this.methodVertices[method];
            if (target != null)
                return target;
            
            // add method
            target = (CustomVertex)this.graph.AddVertex();
            target.Value = method;
            this.methodVertices.Add(method, target);

            return target;
        }

        private void BuildGraphFromType()
        {
            // add vertices
            foreach (IMethodReference method in type.Methods)
            {
                this.BuildGraphFromMethod(method);
            }
        }

        private bool IsException(ITypeReference t)
        {
            return t.Name.EndsWith("Exception");
        }

        void graphviz_FormatVertex(object sender, FormatVertexEventArgs e)
        {
            CustomVertex v = (CustomVertex)e.Vertex;
            IMethodReference method = (IMethodReference)v.Value;

            e.VertexFormatter.Url = String.Format("uri:{0}", v.ID);

            ITypeDeclaration vtype = method.DeclaringType as ITypeDeclaration;
            while (vtype!=null)
            {
                if (vtype!=type)
                {
                    vtype = vtype.BaseType as ITypeDeclaration;
                    continue;
                }

                e.VertexFormatter.FillColor = Color.White;
                e.VertexFormatter.Label = method.ToString();
                if (v.Value == this.Services.AssemblyBrowser.ActiveItem)
                    e.VertexFormatter.FillColor = Color.LightSkyBlue;
                return;
            }

            e.VertexFormatter.FillColor = Color.LightGreen;
            e.VertexFormatter.Label = String.Format("{0}.{1}",
                method.DeclaringType,
                method);
            if (v.Value == this.Services.AssemblyBrowser.ActiveItem)
                e.VertexFormatter.FillColor = Color.LightSkyBlue;

        }


        protected override void OnVertexClick(VertexEventArgs e)
        {
            base.OnVertexClick(e);
            CustomVertex v = (CustomVertex)e.Vertex;
            this.Services.AssemblyBrowser.ActiveItem = v.Value;
        }

        void dfs_TreeEdge(object sender, EdgeEventArgs e)
        {
            this.componentVertices.Add(e.Edge.Target, null);
        }

        void hfs_TreeEdge(object sender, EdgeEventArgs e)
        {
            this.componentVertices.Add(e.Edge.Source, null);
        }
    }
}
