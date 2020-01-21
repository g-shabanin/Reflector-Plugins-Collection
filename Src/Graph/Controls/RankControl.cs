using System;
using System.Xml;
using System.Xml.Serialization;
using System.Data;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Drawing;

namespace Reflector.Graph.Controls
{
	using Reflector.CodeModel;
	using Reflector.Graph.Graphs;
	using Reflector.Framework;
	using QuickGraph.Concepts;
	using QuickGraph;

	internal abstract class RankControl : ScrollableControl, IServiceComponent
	{
		private ReflectorServices services=null;
		private System.Windows.Forms.DataGrid rankGrid;
		private System.Data.DataSet rankDataSet;
		private System.Data.DataTable rankTable;
		private System.Data.DataColumn nameColumn;
		private System.Data.DataColumn rankColumn;
		private Image rankIcon;
		private System.Windows.Forms.ContextMenu contextMenu1;
		private System.Windows.Forms.MenuItem copyAsTextItem;
		private System.Windows.Forms.MenuItem copyAsXmlItem;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public RankControl(System.ComponentModel.IContainer container)
		{
			///
			/// Required for Windows.Forms Class Composition Designer support
			///
			container.Add(this);
			InitializeComponent();
	
		}

		public RankControl()
		{
			///
			/// Required for Windows.Forms Class Composition Designer support
			///
			InitializeComponent();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}


		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.rankGrid = new System.Windows.Forms.DataGrid();
			this.rankTable = new System.Data.DataTable();
			this.nameColumn = new System.Data.DataColumn();
			this.rankColumn = new System.Data.DataColumn();
			this.rankDataSet = new System.Data.DataSet();
			this.contextMenu1 = new System.Windows.Forms.ContextMenu();
			((System.ComponentModel.ISupportInitialize)(this.rankGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.rankTable)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.rankDataSet)).BeginInit();
			this.SuspendLayout();

			this.copyAsTextItem = this.contextMenu1.MenuItems.Add("Copy (Text)",new EventHandler(copyAsText_Click));
			this.copyAsXmlItem = this.contextMenu1.MenuItems.Add("Copy (XML)",new EventHandler(copyAsXml_Click));

			// 
			// rankGrid
			// 
			this.rankGrid.AlternatingBackColor = System.Drawing.Color.WhiteSmoke;
			this.rankGrid.DataMember = "";
			this.rankGrid.DataSource = this.rankTable;
			this.rankGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rankGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.rankGrid.Location = new System.Drawing.Point(0, 0);
			this.rankGrid.Name = "rankGrid";
			this.rankGrid.PreferredColumnWidth = 200;
			this.rankGrid.Size = new System.Drawing.Size(0, 0);
			this.rankGrid.TabIndex = 0;
			// 
			// rankTable
			// 
			this.rankTable.Columns.AddRange(new System.Data.DataColumn[] {
																			 this.nameColumn,
																			 this.rankColumn});
			this.rankTable.TableName = "MethodRanks";
			// 
			// nameColumn
			// 
			this.nameColumn.AllowDBNull = false;
			this.nameColumn.Caption = "Method Name";
			this.nameColumn.ColumnName = "Method";
			// 
			// rankColumn
			// 
			this.rankColumn.Caption = "Rank";
			this.rankColumn.ColumnName = "Rank";
			this.rankColumn.DataType = typeof(System.Double);
			// 
			// rankDataSet
			// 
			this.rankDataSet.DataSetName = "Method Ranks";
			this.rankDataSet.Locale = new System.Globalization.CultureInfo("en");
			this.rankDataSet.Tables.AddRange(new System.Data.DataTable[] {
																			 this.rankTable});
			// 
			// RankControl
			// 
			this.AutoScroll = true;
			this.BackColor = System.Drawing.SystemColors.Info;
			this.ContextMenu = this.contextMenu1;
			this.Controls.Add(this.rankGrid);
			this.Dock = System.Windows.Forms.DockStyle.Fill;
			((System.ComponentModel.ISupportInitialize)(this.rankGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.rankTable)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.rankDataSet)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

	
		public Image RankIcon
		{
			get
			{
				return this.rankIcon;
			}
			set
			{
				this.rankIcon=value;
			}
		}

		public ReflectorServices Services
		{
			get
			{
				return this.services;
			}
			set
			{
				if(this.services!=null)
				{
					this.services.AssemblyBrowser.ActiveItemChanged-=new EventHandler(assemblyBrowser_ActiveItemChanged);
				}
				this.services=value;
				if (this.services!=null)
				{
					this.services.AssemblyBrowser.ActiveItemChanged+=new EventHandler(assemblyBrowser_ActiveItemChanged);
				}
			}
		}

		public DataGrid RankGrid
		{
			get
			{
				return this.rankGrid;
			}
		}
		public DataTable RankTable
		{
			get
			{
				return this.rankTable;
			}
		}

		public DataColumn NameColumn
		{
			get
			{
				return this.NameColumn;
			}
		}

		public DataColumn RankColumn
		{
			get
			{
				return this.rankColumn;
			}
		}

		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);			
			if (this.Parent != null)
			{
				this.Translate();
			}					
		}
	
		protected void ClearRows()
		{
			this.rankTable.Rows.Clear();
		}

		protected void AddRow(string name, double value)
		{
			this.rankTable.Rows.Add(new Object[]{name,value});
		}

		public abstract void Translate();

		private void assemblyBrowser_ActiveItemChanged(object sender, EventArgs e)
		{
			this.ClearRows();
			if (this.Parent==null)
				return;

			this.Translate();
		}

		protected virtual void CopyAsTextToClipboard()
		{
			StringWriter sw = new StringWriter();
			sw.WriteLine("{0}, {1}",this.nameColumn.Caption,this.rankColumn.Caption);
			foreach(DataRow dr in this.rankTable.Rows)
			{
				sw.WriteLine("{0}, {1}",dr.ItemArray[0], dr.ItemArray[1]);
			}
			Clipboard.SetDataObject( sw.ToString(), true );
		}

		protected virtual void CopyAsXmlToClipboard()
		{
			StringWriter sw = new StringWriter();
			sw.WriteLine("<ranking>",this.nameColumn.Caption,this.rankColumn.Caption);
			foreach(DataRow dr in this.rankTable.Rows)
			{
				sw.WriteLine("    <rank name=\"{0}\" value=\"{1}\" />",dr.ItemArray[0], dr.ItemArray[1]);
			}
			sw.WriteLine("</ranking>");
			Clipboard.SetDataObject( sw.ToString(), true );
		}

		private void copyAsText_Click(Object sender, EventArgs e)
		{
			this.CopyAsTextToClipboard();
		}
		private void copyAsXml_Click(Object sender, EventArgs e)
		{
			this.CopyAsXmlToClipboard();
		}
	}
}
