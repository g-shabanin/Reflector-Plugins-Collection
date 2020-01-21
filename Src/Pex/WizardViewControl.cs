// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Reflector.CodeModel;
using System.Diagnostics;

namespace Reflector.Pex
{
    partial class WizardViewControl : 
        UserControl,
        IWizardView
    {
        IAssembly activeItem;

        public WizardViewControl()
        {
            InitializeComponent();
        }

        public event EventHandler GenerateClick
        {
            add { this.generatedButton.Click += value; }
            remove { this.generatedButton.Click -= value; }
        }

        public IAssembly ActiveItem
        {
            get { return this.activeItem; }
            set 
            {
                this.activeItem = value;
                this.activeItemTextBox.Text = 
                    (this.activeItem == null) 
                    ? "" : this.activeItem.ToString();
            }
        }

        public string OutputPath
        {
            get { return this.outputPathTextBox.Text; }
            set { this.outputPathTextBox.Text = value; }
        }

        public string PexPath
        {
            get
            {
                return this.pexPathTextBox.Text;
            }
            set
            {
                this.pexPathTextBox.Text = value;
                this.generatedButton.Enabled =
                    !String.IsNullOrEmpty(this.pexPathTextBox.Text);
            }
        }

        public bool AssertInconclusive
        {
            get
            {
                return this.assertInconclusiveCheckBox.Checked;
            }
            set
            {
                this.assertInconclusiveCheckBox.Checked = value;
            }
        }

        public void ExecutionStarted()
        {
            this.Invoke((MethodInvoker)delegate
            {
                this.generatedButton.Enabled = false;
                this.outputTextBox.Text = "starting wizard..." + Environment.NewLine;
            });
        }

        public void ExecutionFinished()
        {
            this.Invoke((MethodInvoker)delegate
            {
                this.generatedButton.Enabled = true;
                this.outputTextBox.AppendText("wizard execution finished" + Environment.NewLine);
            });
        }

        public void Progress(string message)
        {
            this.Invoke((MethodInvoker)delegate
            {
                this.outputTextBox.AppendText(message + Environment.NewLine);
            });
        }

        private void pexPathBrowseButton_Click(object sender, EventArgs e)
        {
            if (this.openWizardPathDialog.ShowDialog() == DialogResult.OK)
            {
                this.pexPathTextBox.Text = this.openWizardPathDialog.FileName;
            }
        }

        private void outputPathBrowseButton_Click(object sender, EventArgs e)
        {
            if (this.outputPathFolderDialog.ShowDialog() == DialogResult.OK)
            {
                this.outputPathBrowseButton.Text = this.outputPathFolderDialog.SelectedPath;
            }
        }

        private void downloadPexLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start("iexplore.exe", @"http://research.microsoft.com/pex/downloads.aspx");
            }
            catch { }
        }
    }
}
