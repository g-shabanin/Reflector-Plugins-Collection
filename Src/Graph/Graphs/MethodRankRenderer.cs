/*
 * Created by SharpDevelop.
 * User: dehalleux
 * Date: 18/05/2004
 * Time: 15:07
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using QuickGraph;
using QuickGraph.Concepts;
using QuickGraph.Algorithms.Graphviz;
using QuickGraph.Representations;
using NGraphviz.Helpers;
using Reflector.CodeModel;

namespace Reflector.Graph.Graphs
{
	public class MethodRankRenderer
	{
		private MethodRankPopulator populator;
		private GraphvizAlgorithm graphviz;
		
		public MethodRankRenderer(MethodRankPopulator populator)
		{
			if (populator==null)
				throw new ArgumentNullException("populator");
			this.populator = populator;
			this.graphviz = new GraphvizAlgorithm(populator.Graph);
			this.graphviz.ImageType = GraphvizImageType.Svg;
			this.graphviz.CommonVertexFormat.Shape = GraphvizVertexShape.Box;
			
			this.graphviz.FormatVertex += new FormatVertexEventHandler(this.formatVertex);
		}
		
		public BidirectionalGraph Graph
		{
			get
			{
				return this.populator.Graph;
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
			
			e.VertexFormatter.Label = String.Format(
				"{0}",method
				);
		}
	}
}
