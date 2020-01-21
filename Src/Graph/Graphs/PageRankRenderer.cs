/*
 * Created by SharpDevelop.
 * User: dehalleux
 * Date: 19/05/2004
 * Time: 10:12
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using QuickGraph.Algorithms.Graphviz;
using QuickGraph.Algorithms.Ranking;
using QuickGraph.Concepts.Traversals;
using QuickGraph.Concepts;
using QuickGraph;
using NGraphviz.Helpers;

namespace Reflector.Graph.Graphs
{
	using Reflector.CodeModel;
	public class PageRankRenderer
	{
		private PageRankAlgorithm pageRank;
		private GraphvizAlgorithm graphviz;
		
		public PageRankRenderer(PageRankAlgorithm pageRank)
		{
			this.pageRank = pageRank;
			this.graphviz = new GraphvizAlgorithm((IVertexAndEdgeListGraph)pageRank.VisitedGraph);
			this.graphviz.ImageType = GraphvizImageType.Svg;
			this.graphviz.CommonVertexFormat.Shape = GraphvizVertexShape.Box;			
			this.graphviz.FormatVertex += new FormatVertexEventHandler(formatVertex);
		}
		
		public PageRankAlgorithm PageRank
		{
			get
			{
				return this.pageRank;
			}
		}
		
		public GraphvizAlgorithm Graphviz
		{
			get
			{
				return this.graphviz;
			}
		}
		
		public string Render(string fileName)
		{
			return this.graphviz.Write(fileName);
		}
		
		private void formatVertex(Object sender, FormatVertexEventArgs e)
		{
			CustomVertex v = (CustomVertex)e.Vertex;
			IMethodDeclaration method = (IMethodDeclaration)v.Value;
			
			e.VertexFormatter.Label = 
				String.Format("{0}, {1:0.###}",
				              method,							
				              this.pageRank.Ranks[v]
				              );
		}
	}
}
