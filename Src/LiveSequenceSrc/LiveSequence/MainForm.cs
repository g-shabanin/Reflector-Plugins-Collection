using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using LiveSequence.Common;
using LiveSequence.Common.Context;
using LiveSequence.Common.Domain;
using LiveSequence.Common.Graphics;
using LiveSequence.Common.Presentation;
using LiveSequence.Engine;
using LiveSequence.Shapes;
using LiveSequence.Tree;
using Netron.Diagramming.Core;
using ZedGraph;
using FillType = ZedGraph.FillType;

namespace LiveSequence
{
    internal partial class MainForm : Form, IMainFormView
    {
        private readonly MainFormController mainController;
        private string _assemblyFileName = string.Empty;

        private bool globalMove;
        private Point refPoint;
        private TreeViewController<DTreeItem> tvController;

        public MainForm(MainFormController controller)
        {
            InitializeComponent();

            this.elementHost.Child = new DiagramViewer();

            mainController = controller;
            mainController.Initialize(this);
            AssemblyTree = new DTreeNode<DTreeItem>();
        }

        public DTreeNode<DependencyGraphData> RootGraphData { get; set; }

        #region IMainFormView Members

        public DTreeNode<DTreeItem> AssemblyTree { get; set; }

        public string AssemblyFileName
        {
            get { return _assemblyFileName; }
        }

        public void WorkerCompleted()
        {
            UpdateAssemblyStats();

            statusLabel.Text = "Building Tree";

            treeView.BeginUpdate();
            tvController = new TreeViewController<DTreeItem>(treeView,
                                                             mainController.AssemblyParser.AssemblyData.AssemblyTree,
                                                             new ImageTreeNodeMapper<DTreeItem>());
            treeView.EndUpdate();
            // expand the tree
            treeView.Nodes[0].Expand();

            StopProgress();
        }

        public void WorkerProgressChanged(ProgressChangedEventArgs e)
        {
            statusLabel.Text = "Processing " + e.UserState;
        }

        #endregion

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void loadAssemblyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == assemblyOpenDialog.ShowDialog())
            {
                CleanUp();
                mainController.CleanUp();

                _assemblyFileName = assemblyOpenDialog.FileName;

                // change the form title
                Text = string.Format("SequenceViz :: {0}", _assemblyFileName);

                StartProgress();

                mainController.RunWorker();
            }
        }

        private void CleanUp()
        {
            treeView.Nodes.Clear();
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (tvController.SelectedNode != null)
            {
                SelectTabHandler();
            }
        }

        private void SelectTabHandler()
        {
            SelectionType selectedType = DetermineSelectionType();

            if (selectedType != SelectionType.NONE)
            {
                var selectedData = new TreeSelectionData(tvController, selectedType);

                if (!SelectedDataChanged(selectedData))
                {
                    return;
                }

                oldSelectedData = selectedData;

                switch (tabControl.SelectedIndex)
                {
                    case 0: // sequence diagram tab
                        OnSequenceViewerTab(selectedData);
                        break;
                    case 1: // assembly stats tab
                        OnAssemblyStatTab(selectedData);
                        break;
                    case 2:
                        OnDependencyGraphTab(selectedData);
                        break;
                    default:
                        break;
                }
            }
        }

        private SelectionType DetermineSelectionType()
        {
          SelectionType selectedType = SelectionType.NONE;

          switch (tvController.SelectedNode.Depth)
          {
            case 1:
              // nothing
              break;
            case 2:
              // assembly
              selectedType = SelectionType.ASSEMBLY;
              break;
            case 3:
              // namespace
              selectedType = SelectionType.NAMESPACE;
              break;
            case 4:
              // type
              selectedType = SelectionType.TYPE;
              break;
            case 5:
              // method
              selectedType = SelectionType.METHOD;
              break;
            default:
              // do nothing
              break;
          }

          return selectedType;
        }

        private bool SelectedDataChanged(TreeSelectionData data)
        {
            if (oldSelectedData != null &&
                data.AssemblyName == oldSelectedData.AssemblyName &&
                data.NameSpace == oldSelectedData.NameSpace &&
                data.TypeName == oldSelectedData.TypeName &&
                data.MethodName == oldSelectedData.MethodName)
            {
                return false;
            }

            return true;
        }

        private TreeSelectionData oldSelectedData;

        private void OnSequenceViewerTab(TreeSelectionData selectedData)
        {
            if (!tvController.SelectedNode.HasChildren)
            {
                // if method
                if (tvController.SelectedNode.Value.Text.IndexOf('(') > 0)
                {
                    string methodName = selectedData.MethodName;
                    string typeName = selectedData.TypeName;
                    string nameSpace = selectedData.NameSpace;
                    string assemblyname = selectedData.AssemblyName;

                    labelMethodDescription.Text = methodName;
                    labelTypeName.Text = "Type:" + typeName;
                    labelSelectedAssembly.Text = "Assembly:" + assemblyname;

                    SequenceData data = mainController.GetSequenceData(methodName, typeName, nameSpace, assemblyname);
                    IRenderer renderer = new WPFRenderer();
                    renderer.Export(data);
                }
            }
        }

        private void OnDependencyGraphTab(TreeSelectionData selectedData)
        {
            Logger.Current.Debug("MainForm::OnDependencyGraphTab");

            // check the treeview item
            Logger.Current.Debug(">> TreeView Item Clicked: Depth is:" + tvController.SelectedNode.Depth);

            CreateDependencyGraph(selectedData);
        }

        private void OnAssemblyStatTab(TreeSelectionData selectedData)
        {
            Logger.Current.Debug("MainForm::OnAssemblyStatTab");
            // see which item in treeview is clicked
            Logger.Current.Debug(">> TreeView Clicked: Depth is:" + tvController.SelectedNode.Depth);

            CreateChart(selectedData);
        }

        private void StartProgress()
        {
            parserProgressBar.Style = ProgressBarStyle.Marquee;
            statusLabel.Text = "Parsing Assembly....";
        }

        private void StopProgress()
        {
            parserProgressBar.Style = ProgressBarStyle.Blocks;
            statusLabel.Text = "Ready";
        }

        private void UpdateAssemblyStats()
        {
            AssemblyStats asmStats = mainController.GetAssemblyStats();
            lblAssemblyName.Text = asmStats.AssemblyName;
            lblEntryPoint.Text = asmStats.EntryPoint;
            lblTotalTypes.Text = asmStats.TotalTypes.ToString();
            lblTotalAsmRef.Text = asmStats.TotalAsmReferences.ToString();
        }

        // Call this method from the Form_Load method, passing your ZedGraphControl
        public void CreateChart(TreeSelectionData selectionData)
        {
            GraphPane myPane = zedGraph.GraphPane;
            myPane.CurveList = new CurveList();
            // Set the GraphPane title
            string selectedItem = tvController.SelectedNode.Value.Text;
            string selectedItemParent = tvController.SelectedNode.Parent.Value.Text;
            myPane.Title.Text = selectedItem;
            myPane.Title.FontSpec.IsItalic = true;
            myPane.Title.FontSpec.Size = 24f;
            myPane.Title.FontSpec.Family = "Times New Roman";

            // Fill the pane background with a color gradient
            myPane.Fill = new Fill(Color.White, Color.Goldenrod, 45.0f);
            // No fill for the chart background
            myPane.Chart.Fill.Type = FillType.None;

            // Set the legend to an arbitrary location
            myPane.Legend.Position = LegendPos.Float;
            myPane.Legend.Location = new Location(0.95f, 0.15f, CoordType.PaneFraction,
                                                  AlignH.Right, AlignV.Top);
            myPane.Legend.FontSpec.Size = 10f;
            myPane.Legend.IsHStack = false;
            myPane.Legend.IsVisible = false;

            switch (selectionData.SelectionType)
            {
                case SelectionType.ASSEMBLY:
                    foreach (
                        NamespaceData nsData in mainController.AssemblyParser.AssemblyData.AssemblyStats.NamespaceList)
                    {
                        if (nsData.AssemblyName.Equals(selectedItem))
                        {
                            myPane.AddPieSlice(nsData.TotalTypes, Color.Purple, Color.White, 45f, 0.2,
                                               nsData.NamespaceText);
                        }
                    }
                    break;
                case SelectionType.NAMESPACE:
                    foreach (TypeData tData in mainController.AssemblyParser.AssemblyData.AssemblyStats.TypeList)
                    {
                        if (tData.AssemblyName.Equals(selectedItemParent) && tData.NamespaceText.Equals(selectedItem))
                        {
                            myPane.AddPieSlice(tData.TotalMethods, Color.Turquoise, Color.White, 45f, 0.2,
                                               tData.TypeName);
                        }
                    }
                    break;
                default:
                    break;
            }

            // Sum up the pie values                                                               
            CurveList curves = myPane.CurveList;
            double total = 0;
            for (int x = 0; x < curves.Count; x++)
                total += ((PieItem) curves[x]).Value;

            // Calculate the Axis Scale Ranges
            zedGraph.AxisChange();
            zedGraph.Refresh();
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tvController != null && tvController.SelectedNode != null)
            {
                SelectTabHandler();
            }
        }

        private void CreateDependencyGraph(TreeSelectionData selectedData)
        {
            RootGraphData = mainController.GetDependencyGraphData(selectedData);
            ArrangeDiagram();
        }

        private void ArrangeDiagram()
        {
            if (RootGraphData == null)
            {
                return;
            }

            Logger.Current.Debug(">> Dependency graph data count:" + RootGraphData.Nodes.Count);

            //// clear the existing diagram
            canvas.NewDocument();
            canvas.BackColor = Color.WhiteSmoke;

            var rootShape = new ImageRectangle(RootGraphData.Value.Title, GetImageList(RootGraphData.Value.SelectedType),
                                               true);
            canvas.AddShape(rootShape);

            // place the main part in the center of the control
            var centerPoint = new Point(canvas.Width/2, canvas.Height/2);
            rootShape.Move(centerPoint);

            DTreeNode<DependencyGraphData> filteredData = GetFilteredNodes(RootGraphData);

            List<Point> pointsToDraw = GetShapeLocationList(filteredData.Nodes.Count, centerPoint);

            Logger.Current.Debug(">> Total Points Selected:" + pointsToDraw.Count);

            int i = 0;
            foreach (var dNode in filteredData.Nodes)
            {
                var child = new ImageRectangle(dNode.Value.Title, GetImageList(dNode.Value.SelectedType));
                //  treeImageList.Images[2]);
                canvas.AddShape(child);
                child.Move(pointsToDraw[i]);

                // add connection from root to the child
                AddConnection(rootShape, child, pointsToDraw[i], dNode.Value);
                i++;
            }
        }

        private DTreeNode<DependencyGraphData> GetFilteredNodes(DTreeNode<DependencyGraphData> root)
        {
            var fData = new DTreeNode<DependencyGraphData>();

            foreach (var dNode in root.Nodes)
            {
                bool isValid = true;

                switch (dNode.Value.SelectedType)
                {
                    case SelectionType.ASSEMBLY:
                        isValid = checkAssembly.Checked;
                        break;
                    case SelectionType.NAMESPACE:
                        isValid = checkNS.Checked;
                        break;
                    case SelectionType.TYPE:
                        isValid = checkType.Checked;
                        break;
                    case SelectionType.INTERFACE:
                        isValid = checkInterface.Checked;
                        break;
                    case SelectionType.METHOD:
                        isValid = checkMethod.Checked;
                        break;
                    case SelectionType.NONE:
                        isValid = false;
                        break;
                }

                if (isValid)
                {
                    fData.Nodes.Add(dNode.Value);
                }
            }

            return fData;
        }

        private void AddConnection(ShapeBase rootShape, ImageRectangle child, Point point,
                                   DependencyGraphData dGraphData)
        {
            IConnector rootConnector = null;
            IConnector childConnector = null;

            if (point.X <= rootShape.X)
            {
                if (point.Y < rootShape.Y)
                {
                    rootConnector = rootShape.Connectors[0];
                    childConnector = child.Connectors[2];
                }
                else if (point.Y < (rootShape.Y + rootShape.Height))
                {
                    rootConnector = rootShape.Connectors[3];
                    childConnector = child.Connectors[1];
                }
                else
                {
                    rootConnector = rootShape.Connectors[2];
                    childConnector = child.Connectors[0];
                }
            }
            else if (point.X > rootShape.X)
            {
                if (point.Y < rootShape.Y)
                {
                    rootConnector = rootShape.Connectors[0];
                    childConnector = child.Connectors[2];
                }
                else if (point.Y < (rootShape.Y + rootShape.Height))
                {
                    rootConnector = rootShape.Connectors[1];
                    childConnector = child.Connectors[3];
                }
                else
                {
                    rootConnector = rootShape.Connectors[2];
                    childConnector = child.Connectors[0];
                }
            }

            var pS = new LinePenStyle {EndCap = LineCap.DiamondAnchor};

            if (dGraphData.SelectedType == SelectionType.INTERFACE || dGraphData.SelectedType == SelectionType.ABSTRACT)
            {
                pS.Color = Color.Chocolate;
                pS.Width = 2;
                pS.DashStyle = DashStyle.Dash;
            }

            IConnection conn;

            if (dGraphData.Arrow == GraphDataArrow.TO)
            {
                conn = canvas.AddConnection(rootConnector, childConnector);
            }
            else
            {
                conn = canvas.AddConnection(childConnector, rootConnector);
            }

            conn.PenStyle = pS;
        }

        private Image GetImageList(SelectionType selectionType)
        {
            Image i = treeImageList.Images[0];

            switch (selectionType)
            {
                case SelectionType.ASSEMBLY:
                    i = treeImageList.Images[1];
                    break;
                case SelectionType.NAMESPACE:
                    i = treeImageList.Images[2];
                    break;
                case SelectionType.TYPE:
                    i = treeImageList.Images[3];
                    break;
                case SelectionType.METHOD:
                    i = treeImageList.Images[4];
                    break;
                case SelectionType.INTERFACE:
                    i = treeImageList.Images[5];
                    break;
                case SelectionType.ABSTRACT:
                    i = treeImageList.Images[6];
                    break;
                case SelectionType.FIELD:
                    i = treeImageList.Images[7];
                    break;
                case SelectionType.PROPERTY:
                    i = treeImageList.Images[8];
                    break;
                default:
                    // do nothing
                    break;
            }

            return i;
        }

        private static List<Point> GetShapeLocationList(int totalNodes, Point centerPoint)
        {
            var points = new List<Point>();

            for (int i = 1; i <= totalNodes; i++)
            {
                var length = (float) (Math.Min(centerPoint.X, centerPoint.Y)/1.25);

                var thetaInc = (float) ((2*Math.PI)/totalNodes);
                var theta = (float) (-Math.PI/2 + i*thetaInc);

                var x = (int) (centerPoint.X + length*Math.Cos(theta));
                var y = (int) (centerPoint.Y + length*Math.Sin(theta));

                points.Add(new Point(x, y));
            }

            return points;
        }

        private void checkAssembly_CheckedChanged(object sender, EventArgs e)
        {
            ArrangeDiagram();
        }

        private void checkNS_CheckedChanged(object sender, EventArgs e)
        {
            ArrangeDiagram();
        }

        private void checkType_CheckedChanged(object sender, EventArgs e)
        {
            ArrangeDiagram();
        }

        private void checkInterface_CheckedChanged(object sender, EventArgs e)
        {
            ArrangeDiagram();
        }

        private void checkMethod_CheckedChanged(object sender, EventArgs e)
        {
            ArrangeDiagram();
        }

        private void canvas_MouseDown(object sender, MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (ModifierKeys == Keys.Shift)
            {
                globalMove = true;
                refPoint = new Point(e.X, e.Y);
                return;
            }
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var p = new Point(e.X, e.Y);
            if (globalMove)
            {
                foreach (ShapeBase shape in canvas.Controller.Model.DefaultPage.Shapes)
                {
                    shape.Move(new Point(p.X - refPoint.X, p.Y - refPoint.Y));
                    Invalidate();
                }
                refPoint = p;
                return;
            }
        }

        private void canvas_MouseUp(object sender, MouseEventArgs e)
        {
            base.OnMouseUp(e);
            globalMove = false;
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            var query = txtSearchQuery.Text;
            if (treeView.Nodes.Count == 0) return;

            TreeNode targetTreeNode = FindTargetTreeNode(treeView.Nodes[0].Nodes, query);

            if (targetTreeNode != null)
            {
                treeView.SelectedNode = targetTreeNode;
                treeView.Focus();
            }
        }

        private static TreeNode FindTargetTreeNode(TreeNodeCollection nodes, String query)
        {
            // find the appropriate treeNode
            foreach (TreeNode asmNode in nodes)
            {
                foreach (TreeNode nsNode in asmNode.Nodes)
                {
                    foreach (TreeNode typeNode in nsNode.Nodes)
                    {
                        if (typeNode.Text.StartsWith(query))
                        {
                            return typeNode;
                        }

                        foreach (TreeNode methodNode in typeNode.Nodes)
                        {
                            if (!methodNode.Text.StartsWith(query)) continue;
                            return methodNode;
                        }
                    }
                }
            }

            return null;
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F3)
            {
                txtSearchQuery.SelectAll();
                txtSearchQuery.Focus();
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.saveAsDialog.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    using (ContextHelper.CreateSaveScope(this.elementHost.Child as DiagramViewer, this.saveAsDialog.FileName))
                    {
                        // save scope will process the file and save it to XPS file.
                        // no further processing necessary at this point (for now).
                        Logger.Current.Info(string.Format("The diagram has been saved to '{0}'.",
                                                          this.saveAsDialog.FileName));
                    }
                }
                catch (IOException ex)
                {
                    Logger.Current.Error("An exception occurred, most likely the target file is in use.", ex);
                }
            }
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
          System.Windows.Controls.PrintDialog printDialog = new System.Windows.Controls.PrintDialog();
          if (printDialog.ShowDialog().Equals(false))
          {
            return;
          }

          using (ContextHelper.CreatePrintScope(this.elementHost.Child as DiagramViewer, printDialog))
          {
            // Print scope will print out the file to the appropriate printer.
            // no further processing necessary at this point (for now).
            Logger.Current.Info(string.Format("Diagram printed to {0}", printDialog.PrintQueue.FullName));
          }
        }

        private void resetFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
          SelectionType selectedType = this.DetermineSelectionType();
          if (selectedType != SelectionType.NONE)
          {
            var selectedData = new TreeSelectionData(tvController, selectedType);
            this.OnSequenceViewerTab(selectedData);
          }
        }
    }
}