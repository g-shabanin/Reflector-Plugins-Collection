using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Drawing;

using Reflector.CodeModel;
using Reflector.Framework;

using QuickGraph;
using QuickGraph.Providers;
using QuickGraph.Representations;
using QuickGraph.Concepts;

namespace Reflector.Graph.Controls
{
    internal sealed class TypedDataSetVisualizerControl : GraphControl
    {
        private DataSet dataSet = null;
        private AdjacencyGraph graph = null;
        private GraphvizAlgorithm graphviz=null;
        private string url = null;
        private Hashtable tableVertices = new Hashtable();
        private Hashtable columnTypes = new Hashtable();

        public TypedDataSetVisualizerControl()
        {
            this.graph = new AdjacencyGraph(
                new CustomVertexProvider(),
                new CustomEdgeProvider(),
                true
                );

            this.graphviz = new GraphvizAlgorithm(this.graph);
            this.graphviz.ImageType = NGraphviz.Helpers.GraphvizImageType.Svg;
            this.graphviz.CommonVertexFormat.Shape = NGraphviz.Helpers.GraphvizVertexShape.Box;
            this.graphviz.CommonVertexFormat.Font = new System.Drawing.Font("Tahoma", 8.25f);
            this.graphviz.CommonVertexFormat.Style = GraphvizVertexStyle.Filled;
            this.graphviz.CommonVertexFormat.FillColor = Color.LightYellow;
            this.graphviz.FormatVertex += new FormatVertexEventHandler(graphviz_FormatVertex);
            this.graphviz.FormatEdge += new FormatEdgeEventHandler(graphviz_FormatEdge);
        }

        public override QuickGraph.Concepts.Traversals.IVertexListGraph Graph
        {
            get { return this.graph; }
        }



        public override ReflectorServices Services
        {
            get
            {
                return base.Services;
            }
            set
            {
                if (base.Services != null)
                {
                    base.Services.AssemblyBrowser.ActiveItemChanged -= new EventHandler(AssemblyBrowser_ActiveItemChanged);
                }
                base.Services = value;
                if (base.Services != null)
                {
                    base.Services.AssemblyBrowser.ActiveItemChanged+=new EventHandler(AssemblyBrowser_ActiveItemChanged);
                }
            }
        }

        void AssemblyBrowser_ActiveItemChanged(object sender, EventArgs e)
        {            
            this.Translate();
        }

        protected override void Translate()
        {
            // get active type
            ITypeReference activeType = this.Services.ActiveType as ITypeReference;
            if (activeType == null)
                return;

            // check if dataset
            if (!activeType.Name.EndsWith("DataSet"))
                return;

            // ok we can build the dataset
            this.BuildDataSet(activeType.Resolve());
            this.RenderDataSet();
        }

        private void BuildDataSet(ITypeDeclaration activeType)
        {
            this.graph.Clear();
            this.dataSet = new DataSet();
            this.columnTypes.Clear();

            // get tables
            foreach (IPropertyReference property in activeType.Properties)
            {
                ITypeReference propertyType = property.PropertyType as ITypeReference;
                if (propertyType == null)
                    continue;

                if (propertyType.Name.EndsWith("DataTable"))
                {
                    DataTable table = this.dataSet.Tables.Add(property.Name);
                    CustomVertex v = (CustomVertex)this.graph.AddVertex();
                    v.Value = table;
                    this.tableVertices.Add(table, v);
                    ITypeDeclaration tableType = propertyType.Resolve();   
              
                    // find row type
                    foreach (IMethodDeclaration method in tableType.Methods)
                    {
                        if (method.Name.StartsWith("New") && method.Name.EndsWith("Row"))
                        {
                            // return type is the row type
                            ITypeReference rowReference = method.ReturnType.Type as ITypeReference;
                            if (rowReference == null)
                                continue;

                            ITypeDeclaration rowDecl = rowReference.Resolve();
                            foreach (IPropertyReference pref in rowDecl.Properties)
                            {
                                if (pref.Name.EndsWith("Row"))
                                    continue;
                                ITypeReference propertyRefType =
                                    pref.PropertyType as ITypeReference;
                                if (propertyRefType==null)
                                    continue;
                                if (propertyRefType.Name.EndsWith("Row"))
                                    continue;
                                DataColumn column = table.Columns.Add(pref.Name);
                                this.columnTypes.Add(column, pref.PropertyType);
                            }
                        }
                    }

                    // add columns
                }
            }

            // get relations
            foreach (IPropertyReference property in activeType.Properties)
            {
                ITypeReference propertyType = property.PropertyType as ITypeReference;
                if (propertyType == null)
                    continue;

                if (propertyType.Name.EndsWith("DataTable"))
                {
                    DataTable table = this.dataSet.Tables[property.Name];
                    IVertex source = (IVertex)this.tableVertices[table];
                    ITypeDeclaration tableType = propertyType.Resolve();

                    // find row type
                    foreach (IMethodDeclaration method in tableType.Methods)
                    {
                        if (method.Name.StartsWith("New") && method.Name.EndsWith("Row"))
                        {
                            // return type is the row type
                            ITypeReference rowReference = method.ReturnType.Type as ITypeReference;
                            if (rowReference == null)
                                continue;

                            ITypeDeclaration rowDecl = rowReference.Resolve();
                            foreach (IPropertyReference pref in rowDecl.Properties)
                            {
                                ITypeReference propertyRefType =
                                    pref.PropertyType as ITypeReference;
                                if (propertyRefType == null)
                                    continue;
                                if (!propertyRefType.Name.EndsWith("Row"))
                                    continue;

                                // get data table by name
                                string tableName = pref.Name.Substring(0, propertyRefType.Name.Length- "Row".Length);
                                DataTable targetTable = dataSet.Tables[tableName];
                                if (targetTable == null)
                                    continue;

                                IVertex target = (IVertex)this.tableVertices[targetTable];
                                if (target == null)
                                    continue;

                                this.graph.AddEdge(source, target);
                            }
                        }
                    }

                    // add columns
                }
            }
        }

        private void RenderDataSet()
        {

            string fileName = this.GetType().Name;
            this.url = this.graphviz.Write(fileName);

            this.NavigateSvg(url);
        }

        void graphviz_FormatVertex(object sender, FormatVertexEventArgs e)
        {
            CustomVertex v = (CustomVertex)e.Vertex;

            DataTable table = (DataTable)v.Value;

            GraphvizRecord record = new GraphvizRecord();
            GraphvizRecordCell parent = new GraphvizRecordCell();
            record.Cells.Add(parent);

            GraphvizRecordCell title = new GraphvizRecordCell();
            title.Text = table.TableName;
            parent.Cells.Add(title);

            GraphvizRecordCell columns = new GraphvizRecordCell();
            parent.Cells.Add(columns);
            StringWriter sw =new StringWriter();
            for(int i = 0;i<table.Columns.Count;++i)
            {
                DataColumn column = table.Columns[i];
                IType type = this.columnTypes[column] as IType;
                sw.Write("{0}: {1}", column.ColumnName,type);
                if (i < table.Columns.Count - 1)
                    sw.Write("\\n");
            }
            columns.Text = sw.ToString();

            e.VertexFormatter.Record = record;
            e.VertexFormatter.Shape = GraphvizVertexShape.Record;
            e.VertexFormatter.Label = table.TableName;
        }
    }
}
