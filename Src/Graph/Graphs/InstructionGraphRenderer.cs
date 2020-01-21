namespace Reflector.Graph.Graphs
{
	using System;
	using QuickGraph.Concepts;
	using Reflector.Graph;
	using Microsoft.Glee.Drawing;

	/// <summary>
    /// Summary description for IlGraphRenderer.
    /// </summary>
    internal sealed class InstructionGraphRenderer
    {
        private FlowToCodeConverter flowConverter = new FlowToCodeConverter();
        private InstructionGraph graph;

        public InstructionGraphRenderer(InstructionGraph graph)
        {
            this.graph = graph;
        }

        public InstructionGraph Graph
        {
            get
            {
                return this.graph;
            }
        }

        public Microsoft.Glee.Drawing.Graph Render()
        {
            Microsoft.Glee.Drawing.Graph g = new Microsoft.Glee.Drawing.Graph("ILGraph");
            g.GraphAttr.NodeAttr.FontName = "Tahoma";
            g.GraphAttr.NodeAttr.Fontsize = 8;
            g.GraphAttr.NodeAttr.Shape = Shape.Box;

            g.GraphAttr.EdgeAttr.FontName = "Tahoma";
            g.GraphAttr.EdgeAttr.Fontsize = 8;

            foreach (InstructionVertex v in this.graph.Vertices)
            {
                Node node = (Node)g.AddNode(v.ID.ToString());
                formatVertex(v, node);
            }

            foreach (QuickGraph.Concepts.IEdge edge in this.graph.Edges)
                g.AddEdge(edge.Source.ID.ToString(), edge.Target.ID.ToString());

            return g;
        }

        private void formatVertex(InstructionVertex v, Node node)
        {
            node.Attr.Label = v.ToString();

            if (v.Instruction.Value != null)
            {
                node.UserData = v.Instruction.Value;
                node.Attr.Fontcolor = Color.Blue;
            }
            else
                node.Attr.Fontcolor = Color.Black;

            switch (this.flowConverter.Convert(v.Instruction.Code))
            {
                case System.Reflection.Emit.FlowControl.Throw:
                    node.Attr.Fillcolor = Color.MediumVioletRed;
                    break;
                case System.Reflection.Emit.FlowControl.Cond_Branch:
                    node.Attr.Fillcolor = Color.LightSkyBlue;
                    break;
                case System.Reflection.Emit.FlowControl.Branch:
                    node.Attr.Fillcolor = Color.LightSalmon;
                    break;
                case System.Reflection.Emit.FlowControl.Return:
                    node.Attr.Fillcolor = Color.LightGreen;
                    break;
                case System.Reflection.Emit.FlowControl.Break:
                    node.Attr.Fillcolor = Color.LightPink;
                    break;
            }
        }
    }
}
