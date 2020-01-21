using System;
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
	using QuickGraph.Concepts;
	using QuickGraph;

	/// <summary>
	/// Summary description for MethodRankControl.
	/// </summary>
	public class DegreeMethodRankControl : RankControl
	{
		private DataColumn outDegreeColumn;
		public DegreeMethodRankControl(System.ComponentModel.IContainer container)
			:base(container)
		{
			this.RankIcon = Image.FromStream(
				typeof(MethodRankControl)
				.Assembly.GetManifestResourceStream("Reflector.Graph.MethodRank.png")
				);	

			this.outDegreeColumn=this.RankTable.Columns.Add("Out");
			this.outDegreeColumn.DataType=typeof(int);

			this.RankColumn.ColumnName="In";
			this.RankColumn.DataType=typeof(int);
		}

		public DegreeMethodRankControl()
		{
			this.RankIcon = Image.FromStream(
				typeof(MethodRankControl)
				.Assembly.GetManifestResourceStream("Reflector.Graph.MethodRank.png")
				);	

			this.outDegreeColumn=this.RankTable.Columns.Add("Out");
			this.outDegreeColumn.DataType=typeof(int);

			this.RankColumn.ColumnName="In";
			this.RankColumn.DataType=typeof(int);
		}

		public override void Translate()
		{
			this.ClearRows();
			IAssembly assembly = this.Services.ActiveAssembly;
			if (assembly==null)
			{
				return;
			}
				
			MethodRankPopulator pop = new MethodRankPopulator();
			
			pop.CreateFromAssembly(assembly);
			foreach(CustomVertex v in pop.Graph.Vertices)
			{
				IMethodDeclaration me = (IMethodDeclaration)v.Value;
				if (((ITypeReference)me.DeclaringType).Resolve().IsInterface)
					continue;
				string name=String.Format("{0}.{1}",me.DeclaringType,me.Name);
				int inDegree = pop.Graph.InDegree(v);
				int outDegree=pop.Graph.OutDegree(v);
				this.RankTable.Rows.Add(
					new Object[]{name,inDegree,outDegree}
					);
			}
		}
	}
}
