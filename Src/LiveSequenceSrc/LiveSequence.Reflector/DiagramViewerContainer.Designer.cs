namespace Reflector.Sequence
{
  partial class DiagramViewerContainer
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      this.elementHost = new System.Windows.Forms.Integration.ElementHost();
      this.contextViewer = new System.Windows.Forms.ContextMenuStrip(this.components);
      this.menuItemSaveAs = new System.Windows.Forms.ToolStripMenuItem();
      this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
      this.menuItemPrint = new System.Windows.Forms.ToolStripMenuItem();
      this.resetFilterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.saveAsDialog = new System.Windows.Forms.SaveFileDialog();
      this.contextViewer.SuspendLayout();
      this.SuspendLayout();
      // 
      // elementHost
      // 
      this.elementHost.ContextMenuStrip = this.contextViewer;
      this.elementHost.Dock = System.Windows.Forms.DockStyle.Fill;
      this.elementHost.Location = new System.Drawing.Point(0, 0);
      this.elementHost.Name = "elementHost";
      this.elementHost.Size = new System.Drawing.Size(777, 527);
      this.elementHost.TabIndex = 0;
      this.elementHost.Text = "elementHost1";
      this.elementHost.Child = null;
      // 
      // contextViewer
      // 
      this.contextViewer.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetFilterToolStripMenuItem,
            this.toolStripMenuItem1,
            this.menuItemSaveAs,
            this.menuItemPrint});
      this.contextViewer.Name = "contextViewer";
      this.contextViewer.Size = new System.Drawing.Size(153, 98);
      // 
      // menuItemSaveAs
      // 
      this.menuItemSaveAs.Name = "menuItemSaveAs";
      this.menuItemSaveAs.Size = new System.Drawing.Size(152, 22);
      this.menuItemSaveAs.Text = "Save &As...";
      this.menuItemSaveAs.Click += new System.EventHandler(this.OnSaveAsMenuItemClick);
      // 
      // toolStripMenuItem1
      // 
      this.toolStripMenuItem1.Name = "toolStripMenuItem1";
      this.toolStripMenuItem1.Size = new System.Drawing.Size(149, 6);
      // 
      // menuItemPrint
      // 
      this.menuItemPrint.Name = "menuItemPrint";
      this.menuItemPrint.Size = new System.Drawing.Size(152, 22);
      this.menuItemPrint.Text = "&Print...";
      this.menuItemPrint.Click += new System.EventHandler(this.OnPrintMenuItemClick);
      // 
      // resetFilterToolStripMenuItem
      // 
      this.resetFilterToolStripMenuItem.Name = "resetFilterToolStripMenuItem";
      this.resetFilterToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
      this.resetFilterToolStripMenuItem.Text = "&Reset filter";
      this.resetFilterToolStripMenuItem.Click += new System.EventHandler(this.OnResetFilterMenuItemClick);
      // 
      // saveAsDialog
      // 
      this.saveAsDialog.Filter = "XPS-document (*.xps)|*.xps|PNG (*.png)|*.png";
      // 
      // DiagramViewerContainer
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(this.elementHost);
      this.Name = "DiagramViewerContainer";
      this.Size = new System.Drawing.Size(777, 527);
      this.contextViewer.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.Integration.ElementHost elementHost;
    private System.Windows.Forms.SaveFileDialog saveAsDialog;
    private System.Windows.Forms.ContextMenuStrip contextViewer;
    private System.Windows.Forms.ToolStripMenuItem menuItemSaveAs;
    private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
    private System.Windows.Forms.ToolStripMenuItem menuItemPrint;
    private System.Windows.Forms.ToolStripMenuItem resetFilterToolStripMenuItem;
  }
}
