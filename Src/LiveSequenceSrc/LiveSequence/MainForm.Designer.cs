namespace LiveSequence
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
          this.components = new System.ComponentModel.Container();
          System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
          this.menuBar = new System.Windows.Forms.MenuStrip();
          this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.loadAssemblyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
          this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.statusBar = new System.Windows.Forms.StatusStrip();
          this.parserProgressBar = new System.Windows.Forms.ToolStripProgressBar();
          this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
          this.splitContainer1 = new System.Windows.Forms.SplitContainer();
          this.panel1 = new System.Windows.Forms.Panel();
          this.btnSearch = new System.Windows.Forms.Button();
          this.txtSearchQuery = new System.Windows.Forms.TextBox();
          this.groupBox1 = new System.Windows.Forms.GroupBox();
          this.lblTotalAsmRef = new System.Windows.Forms.Label();
          this.label4 = new System.Windows.Forms.Label();
          this.lblTotalTypes = new System.Windows.Forms.Label();
          this.lblEntryPoint = new System.Windows.Forms.Label();
          this.label5 = new System.Windows.Forms.Label();
          this.lblAssemblyName = new System.Windows.Forms.Label();
          this.label2 = new System.Windows.Forms.Label();
          this.label1 = new System.Windows.Forms.Label();
          this.treeView = new System.Windows.Forms.TreeView();
          this.contextTree = new System.Windows.Forms.ContextMenuStrip(this.components);
          this.statsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.treeImageList = new System.Windows.Forms.ImageList(this.components);
          this.tabControl = new System.Windows.Forms.TabControl();
          this.tabPage4 = new System.Windows.Forms.TabPage();
          this.labelMethodDescription = new System.Windows.Forms.Label();
          this.labelSelectedAssembly = new System.Windows.Forms.Label();
          this.labelTypeName = new System.Windows.Forms.Label();
          this.elementHost = new System.Windows.Forms.Integration.ElementHost();
          this.contextSequenceDiagram = new System.Windows.Forms.ContextMenuStrip(this.components);
          this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
          this.printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.tabPage2 = new System.Windows.Forms.TabPage();
          this.zedGraph = new ZedGraph.ZedGraphControl();
          this.tabPage3 = new System.Windows.Forms.TabPage();
          this.panel2 = new System.Windows.Forms.Panel();
          this.checkInterface = new System.Windows.Forms.CheckBox();
          this.label6 = new System.Windows.Forms.Label();
          this.lblHelp = new System.Windows.Forms.Label();
          this.label3 = new System.Windows.Forms.Label();
          this.checkMethod = new System.Windows.Forms.CheckBox();
          this.checkType = new System.Windows.Forms.CheckBox();
          this.checkNS = new System.Windows.Forms.CheckBox();
          this.checkAssembly = new System.Windows.Forms.CheckBox();
          this.canvas = new Netron.Diagramming.Win.DiagramControl();
          this.assemblyOpenDialog = new System.Windows.Forms.OpenFileDialog();
          this.saveAsDialog = new System.Windows.Forms.SaveFileDialog();
          this.resetFilterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
          this.menuBar.SuspendLayout();
          this.statusBar.SuspendLayout();
          this.splitContainer1.Panel1.SuspendLayout();
          this.splitContainer1.Panel2.SuspendLayout();
          this.splitContainer1.SuspendLayout();
          this.panel1.SuspendLayout();
          this.groupBox1.SuspendLayout();
          this.contextTree.SuspendLayout();
          this.tabControl.SuspendLayout();
          this.tabPage4.SuspendLayout();
          this.contextSequenceDiagram.SuspendLayout();
          this.tabPage2.SuspendLayout();
          this.tabPage3.SuspendLayout();
          this.panel2.SuspendLayout();
          ((System.ComponentModel.ISupportInitialize)(this.canvas)).BeginInit();
          this.SuspendLayout();
          // 
          // menuBar
          // 
          this.menuBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
          this.menuBar.Location = new System.Drawing.Point(0, 0);
          this.menuBar.Name = "menuBar";
          this.menuBar.Size = new System.Drawing.Size(884, 24);
          this.menuBar.TabIndex = 0;
          this.menuBar.Text = "menuStrip1";
          // 
          // fileToolStripMenuItem
          // 
          this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadAssemblyToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
          this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
          this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
          this.fileToolStripMenuItem.Text = "&File";
          // 
          // loadAssemblyToolStripMenuItem
          // 
          this.loadAssemblyToolStripMenuItem.Name = "loadAssemblyToolStripMenuItem";
          this.loadAssemblyToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
          this.loadAssemblyToolStripMenuItem.Text = "Load &Assembly";
          this.loadAssemblyToolStripMenuItem.Click += new System.EventHandler(this.loadAssemblyToolStripMenuItem_Click);
          // 
          // toolStripSeparator1
          // 
          this.toolStripSeparator1.Name = "toolStripSeparator1";
          this.toolStripSeparator1.Size = new System.Drawing.Size(151, 6);
          // 
          // exitToolStripMenuItem
          // 
          this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
          this.exitToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
          this.exitToolStripMenuItem.Text = "E&xit";
          this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
          // 
          // statusBar
          // 
          this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.parserProgressBar,
            this.statusLabel});
          this.statusBar.Location = new System.Drawing.Point(0, 440);
          this.statusBar.Name = "statusBar";
          this.statusBar.Size = new System.Drawing.Size(884, 22);
          this.statusBar.TabIndex = 1;
          this.statusBar.Text = "statusStrip1";
          // 
          // parserProgressBar
          // 
          this.parserProgressBar.Name = "parserProgressBar";
          this.parserProgressBar.Size = new System.Drawing.Size(100, 16);
          // 
          // statusLabel
          // 
          this.statusLabel.Name = "statusLabel";
          this.statusLabel.Size = new System.Drawing.Size(39, 17);
          this.statusLabel.Text = "Ready";
          // 
          // splitContainer1
          // 
          this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
          this.splitContainer1.Location = new System.Drawing.Point(0, 24);
          this.splitContainer1.Name = "splitContainer1";
          // 
          // splitContainer1.Panel1
          // 
          this.splitContainer1.Panel1.Controls.Add(this.panel1);
          // 
          // splitContainer1.Panel2
          // 
          this.splitContainer1.Panel2.Controls.Add(this.tabControl);
          this.splitContainer1.Size = new System.Drawing.Size(884, 416);
          this.splitContainer1.SplitterDistance = 294;
          this.splitContainer1.TabIndex = 2;
          // 
          // panel1
          // 
          this.panel1.Controls.Add(this.btnSearch);
          this.panel1.Controls.Add(this.txtSearchQuery);
          this.panel1.Controls.Add(this.groupBox1);
          this.panel1.Controls.Add(this.treeView);
          this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
          this.panel1.Location = new System.Drawing.Point(0, 0);
          this.panel1.Name = "panel1";
          this.panel1.Size = new System.Drawing.Size(294, 416);
          this.panel1.TabIndex = 0;
          // 
          // btnSearch
          // 
          this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
          this.btnSearch.Location = new System.Drawing.Point(215, 3);
          this.btnSearch.Name = "btnSearch";
          this.btnSearch.Size = new System.Drawing.Size(75, 23);
          this.btnSearch.TabIndex = 3;
          this.btnSearch.Text = "Search";
          this.btnSearch.UseVisualStyleBackColor = true;
          this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
          // 
          // txtSearchQuery
          // 
          this.txtSearchQuery.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.txtSearchQuery.Location = new System.Drawing.Point(3, 4);
          this.txtSearchQuery.Name = "txtSearchQuery";
          this.txtSearchQuery.Size = new System.Drawing.Size(205, 20);
          this.txtSearchQuery.TabIndex = 2;
          // 
          // groupBox1
          // 
          this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.groupBox1.Controls.Add(this.lblTotalAsmRef);
          this.groupBox1.Controls.Add(this.label4);
          this.groupBox1.Controls.Add(this.lblTotalTypes);
          this.groupBox1.Controls.Add(this.lblEntryPoint);
          this.groupBox1.Controls.Add(this.label5);
          this.groupBox1.Controls.Add(this.lblAssemblyName);
          this.groupBox1.Controls.Add(this.label2);
          this.groupBox1.Controls.Add(this.label1);
          this.groupBox1.Location = new System.Drawing.Point(4, 278);
          this.groupBox1.Name = "groupBox1";
          this.groupBox1.Size = new System.Drawing.Size(287, 135);
          this.groupBox1.TabIndex = 1;
          this.groupBox1.TabStop = false;
          this.groupBox1.Text = "Assembly Stats";
          // 
          // lblTotalAsmRef
          // 
          this.lblTotalAsmRef.AutoSize = true;
          this.lblTotalAsmRef.Location = new System.Drawing.Point(120, 82);
          this.lblTotalAsmRef.Name = "lblTotalAsmRef";
          this.lblTotalAsmRef.Size = new System.Drawing.Size(0, 13);
          this.lblTotalAsmRef.TabIndex = 10;
          // 
          // label4
          // 
          this.label4.AutoSize = true;
          this.label4.Location = new System.Drawing.Point(-1, 82);
          this.label4.Name = "label4";
          this.label4.Size = new System.Drawing.Size(115, 13);
          this.label4.TabIndex = 9;
          this.label4.Text = "Total Asm References:";
          // 
          // lblTotalTypes
          // 
          this.lblTotalTypes.AutoSize = true;
          this.lblTotalTypes.Location = new System.Drawing.Point(72, 60);
          this.lblTotalTypes.Name = "lblTotalTypes";
          this.lblTotalTypes.Size = new System.Drawing.Size(0, 13);
          this.lblTotalTypes.TabIndex = 8;
          // 
          // lblEntryPoint
          // 
          this.lblEntryPoint.AutoSize = true;
          this.lblEntryPoint.Location = new System.Drawing.Point(66, 40);
          this.lblEntryPoint.Name = "lblEntryPoint";
          this.lblEntryPoint.Size = new System.Drawing.Size(0, 13);
          this.lblEntryPoint.TabIndex = 6;
          // 
          // label5
          // 
          this.label5.AutoSize = true;
          this.label5.Location = new System.Drawing.Point(0, 60);
          this.label5.Name = "label5";
          this.label5.Size = new System.Drawing.Size(66, 13);
          this.label5.TabIndex = 4;
          this.label5.Text = "Total Types:";
          // 
          // lblAssemblyName
          // 
          this.lblAssemblyName.AutoSize = true;
          this.lblAssemblyName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.lblAssemblyName.Location = new System.Drawing.Point(43, 21);
          this.lblAssemblyName.Name = "lblAssemblyName";
          this.lblAssemblyName.Size = new System.Drawing.Size(0, 15);
          this.lblAssemblyName.TabIndex = 3;
          // 
          // label2
          // 
          this.label2.AutoSize = true;
          this.label2.Location = new System.Drawing.Point(-1, 40);
          this.label2.Name = "label2";
          this.label2.Size = new System.Drawing.Size(61, 13);
          this.label2.TabIndex = 1;
          this.label2.Text = "Entry Point:";
          // 
          // label1
          // 
          this.label1.AutoSize = true;
          this.label1.Location = new System.Drawing.Point(-1, 21);
          this.label1.Name = "label1";
          this.label1.Size = new System.Drawing.Size(38, 13);
          this.label1.TabIndex = 0;
          this.label1.Text = "Name:";
          // 
          // treeView
          // 
          this.treeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                      | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.treeView.ContextMenuStrip = this.contextTree;
          this.treeView.HideSelection = false;
          this.treeView.ImageIndex = 0;
          this.treeView.ImageList = this.treeImageList;
          this.treeView.Location = new System.Drawing.Point(0, 29);
          this.treeView.Name = "treeView";
          this.treeView.SelectedImageIndex = 0;
          this.treeView.ShowLines = false;
          this.treeView.Size = new System.Drawing.Size(294, 242);
          this.treeView.TabIndex = 0;
          this.treeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_AfterSelect);
          // 
          // contextTree
          // 
          this.contextTree.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statsToolStripMenuItem});
          this.contextTree.Name = "contextTree";
          this.contextTree.Size = new System.Drawing.Size(100, 26);
          // 
          // statsToolStripMenuItem
          // 
          this.statsToolStripMenuItem.Name = "statsToolStripMenuItem";
          this.statsToolStripMenuItem.Size = new System.Drawing.Size(99, 22);
          this.statsToolStripMenuItem.Text = "&Stats";
          // 
          // treeImageList
          // 
          this.treeImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("treeImageList.ImageStream")));
          this.treeImageList.TransparentColor = System.Drawing.Color.Transparent;
          this.treeImageList.Images.SetKeyName(0, "assembly.png");
          this.treeImageList.Images.SetKeyName(1, "assembly.png");
          this.treeImageList.Images.SetKeyName(2, "namespace.png");
          this.treeImageList.Images.SetKeyName(3, "type.png");
          this.treeImageList.Images.SetKeyName(4, "method.png");
          this.treeImageList.Images.SetKeyName(5, "interface.png");
          this.treeImageList.Images.SetKeyName(6, "type.png");
          this.treeImageList.Images.SetKeyName(7, "fields.png");
          this.treeImageList.Images.SetKeyName(8, "property.png");
          // 
          // tabControl
          // 
          this.tabControl.Controls.Add(this.tabPage4);
          this.tabControl.Controls.Add(this.tabPage2);
          this.tabControl.Controls.Add(this.tabPage3);
          this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
          this.tabControl.Location = new System.Drawing.Point(0, 0);
          this.tabControl.Name = "tabControl";
          this.tabControl.SelectedIndex = 0;
          this.tabControl.Size = new System.Drawing.Size(586, 416);
          this.tabControl.TabIndex = 0;
          this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
          // 
          // tabPage4
          // 
          this.tabPage4.Controls.Add(this.labelMethodDescription);
          this.tabPage4.Controls.Add(this.labelSelectedAssembly);
          this.tabPage4.Controls.Add(this.labelTypeName);
          this.tabPage4.Controls.Add(this.elementHost);
          this.tabPage4.Location = new System.Drawing.Point(4, 22);
          this.tabPage4.Name = "tabPage4";
          this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
          this.tabPage4.Size = new System.Drawing.Size(578, 390);
          this.tabPage4.TabIndex = 4;
          this.tabPage4.Text = "Sequence Diagram";
          this.tabPage4.UseVisualStyleBackColor = true;
          // 
          // labelMethodDescription
          // 
          this.labelMethodDescription.AutoSize = true;
          this.labelMethodDescription.BackColor = System.Drawing.Color.White;
          this.labelMethodDescription.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.labelMethodDescription.Location = new System.Drawing.Point(3, 3);
          this.labelMethodDescription.Name = "labelMethodDescription";
          this.labelMethodDescription.Size = new System.Drawing.Size(150, 25);
          this.labelMethodDescription.TabIndex = 5;
          this.labelMethodDescription.Text = "<methodname>";
          // 
          // labelSelectedAssembly
          // 
          this.labelSelectedAssembly.AutoSize = true;
          this.labelSelectedAssembly.BackColor = System.Drawing.Color.White;
          this.labelSelectedAssembly.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.labelSelectedAssembly.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
          this.labelSelectedAssembly.Location = new System.Drawing.Point(6, 41);
          this.labelSelectedAssembly.Name = "labelSelectedAssembly";
          this.labelSelectedAssembly.Size = new System.Drawing.Size(38, 13);
          this.labelSelectedAssembly.TabIndex = 7;
          this.labelSelectedAssembly.Text = "<asm>";
          // 
          // labelTypeName
          // 
          this.labelTypeName.AutoSize = true;
          this.labelTypeName.BackColor = System.Drawing.Color.White;
          this.labelTypeName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.labelTypeName.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
          this.labelTypeName.Location = new System.Drawing.Point(6, 28);
          this.labelTypeName.Name = "labelTypeName";
          this.labelTypeName.Size = new System.Drawing.Size(65, 13);
          this.labelTypeName.TabIndex = 6;
          this.labelTypeName.Text = "<typename>";
          // 
          // elementHost
          // 
          this.elementHost.ContextMenuStrip = this.contextSequenceDiagram;
          this.elementHost.Dock = System.Windows.Forms.DockStyle.Fill;
          this.elementHost.Location = new System.Drawing.Point(3, 3);
          this.elementHost.Name = "elementHost";
          this.elementHost.Size = new System.Drawing.Size(572, 384);
          this.elementHost.TabIndex = 0;
          this.elementHost.Child = null;
          // 
          // contextSequenceDiagram
          // 
          this.contextSequenceDiagram.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetFilterToolStripMenuItem,
            this.toolStripMenuItem1,
            this.saveAsToolStripMenuItem,
            this.printToolStripMenuItem});
          this.contextSequenceDiagram.Name = "contextSequenceXps";
          this.contextSequenceDiagram.Size = new System.Drawing.Size(153, 98);
          // 
          // saveAsToolStripMenuItem
          // 
          this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
          this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
          this.saveAsToolStripMenuItem.Text = "Save &As...";
          this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
          // 
          // toolStripMenuItem1
          // 
          this.toolStripMenuItem1.Name = "toolStripMenuItem1";
          this.toolStripMenuItem1.Size = new System.Drawing.Size(149, 6);
          // 
          // printToolStripMenuItem
          // 
          this.printToolStripMenuItem.Name = "printToolStripMenuItem";
          this.printToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
          this.printToolStripMenuItem.Text = "&Print...";
          this.printToolStripMenuItem.Click += new System.EventHandler(this.printToolStripMenuItem_Click);
          // 
          // tabPage2
          // 
          this.tabPage2.Controls.Add(this.zedGraph);
          this.tabPage2.Location = new System.Drawing.Point(4, 22);
          this.tabPage2.Name = "tabPage2";
          this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
          this.tabPage2.Size = new System.Drawing.Size(578, 390);
          this.tabPage2.TabIndex = 1;
          this.tabPage2.Text = "Assembly Stats";
          this.tabPage2.UseVisualStyleBackColor = true;
          // 
          // zedGraph
          // 
          this.zedGraph.Dock = System.Windows.Forms.DockStyle.Fill;
          this.zedGraph.Location = new System.Drawing.Point(3, 3);
          this.zedGraph.Name = "zedGraph";
          this.zedGraph.ScrollGrace = 0;
          this.zedGraph.ScrollMaxX = 0;
          this.zedGraph.ScrollMaxY = 0;
          this.zedGraph.ScrollMaxY2 = 0;
          this.zedGraph.ScrollMinX = 0;
          this.zedGraph.ScrollMinY = 0;
          this.zedGraph.ScrollMinY2 = 0;
          this.zedGraph.Size = new System.Drawing.Size(572, 384);
          this.zedGraph.TabIndex = 0;
          // 
          // tabPage3
          // 
          this.tabPage3.Controls.Add(this.panel2);
          this.tabPage3.Controls.Add(this.canvas);
          this.tabPage3.Location = new System.Drawing.Point(4, 22);
          this.tabPage3.Name = "tabPage3";
          this.tabPage3.Size = new System.Drawing.Size(578, 390);
          this.tabPage3.TabIndex = 3;
          this.tabPage3.Text = "Dependency Explorer";
          this.tabPage3.UseVisualStyleBackColor = true;
          // 
          // panel2
          // 
          this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                      | System.Windows.Forms.AnchorStyles.Right)));
          this.panel2.Controls.Add(this.checkInterface);
          this.panel2.Controls.Add(this.label6);
          this.panel2.Controls.Add(this.lblHelp);
          this.panel2.Controls.Add(this.label3);
          this.panel2.Controls.Add(this.checkMethod);
          this.panel2.Controls.Add(this.checkType);
          this.panel2.Controls.Add(this.checkNS);
          this.panel2.Controls.Add(this.checkAssembly);
          this.panel2.Location = new System.Drawing.Point(0, 0);
          this.panel2.Name = "panel2";
          this.panel2.Size = new System.Drawing.Size(578, 33);
          this.panel2.TabIndex = 3;
          // 
          // checkInterface
          // 
          this.checkInterface.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
          this.checkInterface.AutoSize = true;
          this.checkInterface.Checked = true;
          this.checkInterface.CheckState = System.Windows.Forms.CheckState.Checked;
          this.checkInterface.Location = new System.Drawing.Point(431, 9);
          this.checkInterface.Name = "checkInterface";
          this.checkInterface.Size = new System.Drawing.Size(68, 17);
          this.checkInterface.TabIndex = 7;
          this.checkInterface.Text = "Interface";
          this.checkInterface.UseVisualStyleBackColor = true;
          this.checkInterface.CheckedChanged += new System.EventHandler(this.checkInterface_CheckedChanged);
          // 
          // label6
          // 
          this.label6.AutoSize = true;
          this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.label6.Location = new System.Drawing.Point(0, 13);
          this.label6.Name = "label6";
          this.label6.Size = new System.Drawing.Size(142, 13);
          this.label6.TabIndex = 6;
          this.label6.Text = "Ctrl + Mouse Wheel to Zoom";
          // 
          // lblHelp
          // 
          this.lblHelp.AutoSize = true;
          this.lblHelp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
          this.lblHelp.Location = new System.Drawing.Point(0, 0);
          this.lblHelp.Name = "lblHelp";
          this.lblHelp.Size = new System.Drawing.Size(144, 13);
          this.lblHelp.TabIndex = 5;
          this.lblHelp.Text = "Shift + Mouse Move to Move";
          // 
          // label3
          // 
          this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
          this.label3.AutoSize = true;
          this.label3.Location = new System.Drawing.Point(173, 10);
          this.label3.Name = "label3";
          this.label3.Size = new System.Drawing.Size(34, 13);
          this.label3.TabIndex = 4;
          this.label3.Text = "Show";
          // 
          // checkMethod
          // 
          this.checkMethod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
          this.checkMethod.AutoSize = true;
          this.checkMethod.Checked = true;
          this.checkMethod.CheckState = System.Windows.Forms.CheckState.Checked;
          this.checkMethod.Location = new System.Drawing.Point(508, 9);
          this.checkMethod.Name = "checkMethod";
          this.checkMethod.Size = new System.Drawing.Size(62, 17);
          this.checkMethod.TabIndex = 3;
          this.checkMethod.Text = "Method";
          this.checkMethod.UseVisualStyleBackColor = true;
          this.checkMethod.CheckedChanged += new System.EventHandler(this.checkMethod_CheckedChanged);
          // 
          // checkType
          // 
          this.checkType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
          this.checkType.AutoSize = true;
          this.checkType.Checked = true;
          this.checkType.CheckState = System.Windows.Forms.CheckState.Checked;
          this.checkType.Location = new System.Drawing.Point(379, 9);
          this.checkType.Name = "checkType";
          this.checkType.Size = new System.Drawing.Size(50, 17);
          this.checkType.TabIndex = 2;
          this.checkType.Text = "Type";
          this.checkType.UseVisualStyleBackColor = true;
          this.checkType.CheckedChanged += new System.EventHandler(this.checkType_CheckedChanged);
          // 
          // checkNS
          // 
          this.checkNS.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
          this.checkNS.AutoSize = true;
          this.checkNS.Checked = true;
          this.checkNS.CheckState = System.Windows.Forms.CheckState.Checked;
          this.checkNS.Location = new System.Drawing.Point(290, 9);
          this.checkNS.Name = "checkNS";
          this.checkNS.Size = new System.Drawing.Size(83, 17);
          this.checkNS.TabIndex = 1;
          this.checkNS.Text = "Namespace";
          this.checkNS.UseVisualStyleBackColor = true;
          this.checkNS.CheckedChanged += new System.EventHandler(this.checkNS_CheckedChanged);
          // 
          // checkAssembly
          // 
          this.checkAssembly.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
          this.checkAssembly.AutoSize = true;
          this.checkAssembly.Checked = true;
          this.checkAssembly.CheckState = System.Windows.Forms.CheckState.Checked;
          this.checkAssembly.Location = new System.Drawing.Point(214, 9);
          this.checkAssembly.Name = "checkAssembly";
          this.checkAssembly.Size = new System.Drawing.Size(70, 17);
          this.checkAssembly.TabIndex = 0;
          this.checkAssembly.Text = "Assembly";
          this.checkAssembly.UseVisualStyleBackColor = true;
          this.checkAssembly.CheckedChanged += new System.EventHandler(this.checkAssembly_CheckedChanged);
          // 
          // canvas
          // 
          this.canvas.AllowDrop = true;
          this.canvas.AutoScroll = true;
          this.canvas.BackColor = System.Drawing.Color.White;
          this.canvas.BackgroundType = Netron.Diagramming.Core.CanvasBackgroundTypes.FlatColor;
          this.canvas.Dock = System.Windows.Forms.DockStyle.Fill;
          this.canvas.Document = ((Netron.Diagramming.Core.Document)(resources.GetObject("canvas.Document")));
          this.canvas.EnableAddConnection = true;
          this.canvas.Location = new System.Drawing.Point(0, 0);
          this.canvas.Magnification = new System.Drawing.SizeF(100F, 100F);
          this.canvas.Name = "canvas";
          this.canvas.Origin = new System.Drawing.Point(0, 0);
          this.canvas.ShowPage = false;
          this.canvas.ShowRulers = false;
          this.canvas.Size = new System.Drawing.Size(578, 390);
          this.canvas.TabIndex = 4;
          this.canvas.Text = "diagramControl1";
          this.canvas.MouseUp += new System.Windows.Forms.MouseEventHandler(this.canvas_MouseUp);
          this.canvas.MouseMove += new System.Windows.Forms.MouseEventHandler(this.canvas_MouseMove);
          this.canvas.MouseDown += new System.Windows.Forms.MouseEventHandler(this.canvas_MouseDown);
          // 
          // saveAsDialog
          // 
          this.saveAsDialog.Filter = "XPS-document (*.xps)|*.xps|PNG (*.png)|*.png";
          // 
          // resetFilterToolStripMenuItem
          // 
          this.resetFilterToolStripMenuItem.Name = "resetFilterToolStripMenuItem";
          this.resetFilterToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
          this.resetFilterToolStripMenuItem.Text = "&Reset filter";
          this.resetFilterToolStripMenuItem.Click += new System.EventHandler(this.resetFilterToolStripMenuItem_Click);
          // 
          // MainForm
          // 
          this.AcceptButton = this.btnSearch;
          this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
          this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
          this.ClientSize = new System.Drawing.Size(884, 462);
          this.Controls.Add(this.splitContainer1);
          this.Controls.Add(this.statusBar);
          this.Controls.Add(this.menuBar);
          this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
          this.KeyPreview = true;
          this.MainMenuStrip = this.menuBar;
          this.Name = "MainForm";
          this.Text = "SequenceViz";
          this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
          this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
          this.menuBar.ResumeLayout(false);
          this.menuBar.PerformLayout();
          this.statusBar.ResumeLayout(false);
          this.statusBar.PerformLayout();
          this.splitContainer1.Panel1.ResumeLayout(false);
          this.splitContainer1.Panel2.ResumeLayout(false);
          this.splitContainer1.ResumeLayout(false);
          this.panel1.ResumeLayout(false);
          this.panel1.PerformLayout();
          this.groupBox1.ResumeLayout(false);
          this.groupBox1.PerformLayout();
          this.contextTree.ResumeLayout(false);
          this.tabControl.ResumeLayout(false);
          this.tabPage4.ResumeLayout(false);
          this.tabPage4.PerformLayout();
          this.contextSequenceDiagram.ResumeLayout(false);
          this.tabPage2.ResumeLayout(false);
          this.tabPage3.ResumeLayout(false);
          this.panel2.ResumeLayout(false);
          this.panel2.PerformLayout();
          ((System.ComponentModel.ISupportInitialize)(this.canvas)).EndInit();
          this.ResumeLayout(false);
          this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuBar;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.ToolStripMenuItem loadAssemblyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.OpenFileDialog assemblyOpenDialog;
        //private System.ComponentModel.BackgroundWorker assemblyParserWorker;
        private System.Windows.Forms.ToolStripProgressBar parserProgressBar;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblAssemblyName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblTotalTypes;
        private System.Windows.Forms.Label lblEntryPoint;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblTotalAsmRef;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ImageList treeImageList;
        private System.Windows.Forms.ContextMenuStrip contextTree;
        private System.Windows.Forms.ToolStripMenuItem statsToolStripMenuItem;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPage2;
        private ZedGraph.ZedGraphControl zedGraph;
        private System.Windows.Forms.TabPage tabPage3;
        //private Netron.Diagramming.Win.DiagramControl canvas;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkMethod;
        private System.Windows.Forms.CheckBox checkType;
        private System.Windows.Forms.CheckBox checkNS;
        private System.Windows.Forms.CheckBox checkAssembly;
        private System.Windows.Forms.Label lblHelp;
        private System.Windows.Forms.Label label6;
        private Netron.Diagramming.Win.DiagramControl canvas;
        private System.Windows.Forms.CheckBox checkInterface;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.TextBox txtSearchQuery;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Integration.ElementHost elementHost;
        private System.Windows.Forms.ContextMenuStrip contextSequenceDiagram;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.Label labelMethodDescription;
        private System.Windows.Forms.Label labelSelectedAssembly;
        private System.Windows.Forms.Label labelTypeName;
        private System.Windows.Forms.SaveFileDialog saveAsDialog;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem printToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetFilterToolStripMenuItem;
    }
}

