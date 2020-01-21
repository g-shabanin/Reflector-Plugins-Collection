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
	public class MethodRankControl : RankControl
	{
		public MethodRankControl(System.ComponentModel.IContainer container)
			:base(container)
		{
			this.RankIcon = Image.FromStream(
				typeof(MethodRankControl)
				.Assembly.GetManifestResourceStream("Reflector.Graph.MethodRank.png")
				);	
		}

		public MethodRankControl()
		{
			this.RankIcon = Image.FromStream(
				typeof(MethodRankControl)
				.Assembly.GetManifestResourceStream("Reflector.Graph.MethodRank.png")
				);	
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
			
			pop.PopulateGraph(assembly);	
			pop.ComputePageRank();			

			this.SuspendLayout();
			this.RankGrid.SuspendLayout();
			foreach(DictionaryEntry de in pop.PageRank.Ranks)
			{
				CustomVertex v = (CustomVertex)de.Key;
				IMethodReference me = (IMethodReference)v.Value;
				double rank =(double)de.Value;

				ITypeReference declaringType = me.DeclaringType as ITypeReference;
				if (declaringType==null)
					continue;

				string name=String.Format("{0}.{1}.{2}",
					declaringType.Namespace,
					declaringType.Name,
					me.Name);

				this.AddRow(name,rank);
			}
			this.RankGrid.ResumeLayout();
			this.ResumeLayout();
		}
	}
}
