﻿namespace LiveSequence
{
  partial class ConsoleView
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
      this.SuspendLayout();
      // 
      // ConsoleView
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(104, 19);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ConsoleView";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.Text = "ConsoleForm";
      this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
      this.Load += new System.EventHandler(this.ConsoleFormOnLoad);
      this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConsoleFormOnFormClosing);
      this.ResumeLayout(false);

    }

    #endregion
  }
}