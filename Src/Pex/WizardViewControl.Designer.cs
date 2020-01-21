// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

namespace Reflector.Pex
{
    partial class WizardViewControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WizardViewControl));
            this.label1 = new System.Windows.Forms.Label();
            this.generatedButton = new System.Windows.Forms.Button();
            this.assertInconclusiveCheckBox = new System.Windows.Forms.CheckBox();
            this.activeItemTextBox = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.pexPathBrowseButton = new System.Windows.Forms.Button();
            this.pexPathTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.outputPathTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.outputPathBrowseButton = new System.Windows.Forms.Button();
            this.openWizardPathDialog = new System.Windows.Forms.OpenFileDialog();
            this.outputPathFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.outputTextBox = new System.Windows.Forms.TextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.downloadPexLabel = new System.Windows.Forms.LinkLabel();
            this.label4 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Target";
            // 
            // generatedButton
            // 
            this.generatedButton.Location = new System.Drawing.Point(309, 70);
            this.generatedButton.Name = "generatedButton";
            this.generatedButton.Size = new System.Drawing.Size(149, 46);
            this.generatedButton.TabIndex = 3;
            this.generatedButton.Text = "Generate Test Project";
            this.generatedButton.UseVisualStyleBackColor = true;
            // 
            // assertInconclusiveCheckBox
            // 
            this.assertInconclusiveCheckBox.AutoSize = true;
            this.assertInconclusiveCheckBox.Location = new System.Drawing.Point(10, 44);
            this.assertInconclusiveCheckBox.Name = "assertInconclusiveCheckBox";
            this.assertInconclusiveCheckBox.Size = new System.Drawing.Size(118, 17);
            this.assertInconclusiveCheckBox.TabIndex = 2;
            this.assertInconclusiveCheckBox.Text = "Assert.Inconclusive";
            this.assertInconclusiveCheckBox.UseVisualStyleBackColor = true;
            // 
            // activeItemTextBox
            // 
            this.activeItemTextBox.Location = new System.Drawing.Point(85, 17);
            this.activeItemTextBox.Name = "activeItemTextBox";
            this.activeItemTextBox.ReadOnly = true;
            this.activeItemTextBox.Size = new System.Drawing.Size(373, 20);
            this.activeItemTextBox.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.pexPathBrowseButton);
            this.groupBox1.Controls.Add(this.pexPathTextBox);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.assertInconclusiveCheckBox);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 200);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(464, 100);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Options";
            // 
            // pexPathBrowseButton
            // 
            this.pexPathBrowseButton.Location = new System.Drawing.Point(426, 15);
            this.pexPathBrowseButton.Name = "pexPathBrowseButton";
            this.pexPathBrowseButton.Size = new System.Drawing.Size(32, 23);
            this.pexPathBrowseButton.TabIndex = 1;
            this.pexPathBrowseButton.Text = "...";
            this.pexPathBrowseButton.UseVisualStyleBackColor = true;
            this.pexPathBrowseButton.Click += new System.EventHandler(this.pexPathBrowseButton_Click);
            // 
            // pexPathTextBox
            // 
            this.pexPathTextBox.Location = new System.Drawing.Point(85, 18);
            this.pexPathTextBox.Name = "pexPathTextBox";
            this.pexPathTextBox.Size = new System.Drawing.Size(335, 20);
            this.pexPathTextBox.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Wizard";
            // 
            // outputPathTextBox
            // 
            this.outputPathTextBox.Location = new System.Drawing.Point(85, 43);
            this.outputPathTextBox.Name = "outputPathTextBox";
            this.outputPathTextBox.Size = new System.Drawing.Size(335, 20);
            this.outputPathTextBox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Output Path";
            // 
            // outputPathBrowseButton
            // 
            this.outputPathBrowseButton.Location = new System.Drawing.Point(426, 41);
            this.outputPathBrowseButton.Name = "outputPathBrowseButton";
            this.outputPathBrowseButton.Size = new System.Drawing.Size(32, 23);
            this.outputPathBrowseButton.TabIndex = 2;
            this.outputPathBrowseButton.Text = "...";
            this.outputPathBrowseButton.UseVisualStyleBackColor = true;
            this.outputPathBrowseButton.Click += new System.EventHandler(this.outputPathBrowseButton_Click);
            // 
            // openWizardPathDialog
            // 
            this.openWizardPathDialog.DefaultExt = "exe";
            this.openWizardPathDialog.FileName = "pexwizard.exe";
            this.openWizardPathDialog.Filter = "Executables|*.exe";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.outputTextBox);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(0, 300);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(464, 140);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Output";
            // 
            // outputTextBox
            // 
            this.outputTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.outputTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputTextBox.Location = new System.Drawing.Point(3, 16);
            this.outputTextBox.Multiline = true;
            this.outputTextBox.Name = "outputTextBox";
            this.outputTextBox.ReadOnly = true;
            this.outputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.outputTextBox.Size = new System.Drawing.Size(458, 121);
            this.outputTextBox.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.generatedButton);
            this.groupBox3.Controls.Add(this.outputPathTextBox);
            this.groupBox3.Controls.Add(this.outputPathBrowseButton);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.activeItemTextBox);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox3.Location = new System.Drawing.Point(0, 75);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(464, 125);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Generation";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.downloadPexLabel);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(464, 75);
            this.panel1.TabIndex = 0;
            // 
            // downloadPexLabel
            // 
            this.downloadPexLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.downloadPexLabel.AutoSize = true;
            this.downloadPexLabel.Location = new System.Drawing.Point(276, 6);
            this.downloadPexLabel.Name = "downloadPexLabel";
            this.downloadPexLabel.Size = new System.Drawing.Size(76, 13);
            this.downloadPexLabel.TabIndex = 1;
            this.downloadPexLabel.TabStop = true;
            this.downloadPexLabel.Text = "Download Pex";
            this.downloadPexLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.downloadPexLabel_LinkClicked);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(7, 49);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(268, 16);
            this.label4.TabIndex = 1;
            this.label4.Text = "Create Parameterized Unit Test Stubs Project";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(358, 6);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(100, 60);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // WizardViewControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.panel1);
            this.Name = "WizardViewControl";
            this.Size = new System.Drawing.Size(464, 440);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button generatedButton;
        private System.Windows.Forms.CheckBox assertInconclusiveCheckBox;
        private System.Windows.Forms.TextBox activeItemTextBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button pexPathBrowseButton;
        private System.Windows.Forms.TextBox pexPathTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox outputPathTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button outputPathBrowseButton;
        private System.Windows.Forms.OpenFileDialog openWizardPathDialog;
        private System.Windows.Forms.FolderBrowserDialog outputPathFolderDialog;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox outputTextBox;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.LinkLabel downloadPexLabel;
    }
}
