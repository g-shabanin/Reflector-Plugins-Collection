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
	public class TypeRankControl : RankControl
	{
		public TypeRankControl(System.ComponentModel.IContainer container)
			:base(container)
		{
			this.RankIcon = Image.FromStream(
				typeof(TypeRankControl)
				.Assembly.GetManifestResourceStream("Reflector.Graph.TypeRank.png")
				);	
		}

		public TypeRankControl()
		{
			this.RankIcon = Image.FromStream(
				typeof(TypeRankControl)
				.Assembly.GetManifestResourceStream("Reflector.Graph.TypeRank.png")
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
			TypeRankPopulator pop = new TypeRankPopulator();
			
			pop.CreateFromAssembly(assembly);	
			pop.ComputePageRank();			
			foreach(DictionaryEntry de in pop.PageRank.Ranks)
			{
				CustomVertex v = (CustomVertex)de.Key;
				ITypeReference te = (ITypeReference)v.Value;
				double rank =(double)de.Value;

				this.AddRow(te.Name,rank);
			}
		}
	}
}
