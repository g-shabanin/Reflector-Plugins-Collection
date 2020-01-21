using System;
using QuickGraph;
using QuickGraph.Providers;
using QuickGraph.Concepts;

namespace Reflector.Graph.Graphs
{
    public sealed class StatementEdge : NamedEdge
    {
        public StatementEdge()
        { }
        public StatementEdge(int id,IVertex source, IVertex target)
            :base(id,source,target)
        {
            this.Name = "";
        }

        public new StatementVertex Source
        {
            get
            {
                return (StatementVertex)base.Source;
            }
        }

        public new StatementVertex Target
        {
            get
            {
                return (StatementVertex)base.Target;
            }
        }

        public class Provider : TypedEdgeProvider
        {
            public Provider()
                :base(typeof(StatementEdge))
            {}
        }
    }
}
