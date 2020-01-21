namespace Reflector.Graph.Graphs
{
	using System;
	using QuickGraph.Collections;
	using QuickGraph.Algorithms;
	using QuickGraph.Algorithms.Search;
	using QuickGraph.Algorithms.Visitors;
	using QuickGraph.Algorithms.ShortestPath;
	using QuickGraph.Concepts;
	using Reflector.CodeModel;

	/// <summary>
	/// Summary description for StatementGraphPopulator.
	/// </summary>
	public sealed class StatementGraphPopulator
	{
        private StatementGraph graph = null;
        private ITranslatorManager translatorManager;
        private IMethodDeclaration visitedMethod = null;

		public StatementGraphPopulator(ITranslatorManager translatorManager)
        {
			this.translatorManager = translatorManager;
        }

        public StatementGraph Graph
        {
            get
            {
                return this.graph;
            }
        }

        public IMethodDeclaration VisitedMethod
        {
            get
            {
                return this.visitedMethod;
            }
        }

        public StatementGraph BuildGraphFromMethod(IMethodDeclaration method)
		{
			if (method==null)
				throw new ArgumentNullException("method");

			this.graph = new StatementGraph(method);

            // resolve body
            this.visitedMethod = this.translatorManager.CreateDisassembler(null, null).TranslateMethodDeclaration(method);

            // explore vertex
			StatementGraphVertexPopulatorVisitor vertexVisitor = 
				new StatementGraphVertexPopulatorVisitor(graph);
            vertexVisitor.VisitMethodDeclaration(visitedMethod);

            // explore edges
			StatementGraphEdgePopulatorVisitor edgeVisitor = 
				new StatementGraphEdgePopulatorVisitor(graph);
            edgeVisitor.VisitMethodDeclaration(visitedMethod);

            return graph;
		}

        public EdgeCollectionCollection GetAllEdgePaths()
        {
            if (this.Graph.VerticesCount == 0)
                return new EdgeCollectionCollection();

            DepthFirstSearchAlgorithm efs = new DepthFirstSearchAlgorithm(this.graph);
            PredecessorRecorderVisitor vis = new PredecessorRecorderVisitor();
            efs.RegisterPredecessorRecorderHandlers(vis);

            // get root vertex
            efs.Compute(this.Graph.Root);

            // close path
            EdgeCollectionCollection paths = vis.AllPaths();
            foreach (EdgeCollection edges in paths)
            {
                StatementVertex target = (StatementVertex)edges[edges.Count - 1].Target;
                if (graph.OutEdgesEmpty(target))
                    continue;

                ClosePath(edges, target);
            }

            return paths;
        }

        private void ClosePath(EdgeCollection edges, IVertex target)
        {
            DijkstraShortestPathAlgorithm diskstra = new DijkstraShortestPathAlgorithm(this.graph,
                DijkstraShortestPathAlgorithm.UnaryWeightsFromEdgeList(this.graph)
                );
            PredecessorRecorderVisitor pvis = new PredecessorRecorderVisitor();
            diskstra.RegisterPredecessorRecorderHandlers(pvis);
            diskstra.Compute(target);

            // for each sink look at the distance
            IVertex sink = null;
            double distance = double.MaxValue;
            foreach (IVertex v in AlgoUtility.Sinks(graph))
            {
                if (diskstra.Distances[v] < distance)
                {
                    distance = diskstra.Distances[v];
                    sink = v;
                }
            }
            if (sink == null)
                return;

            EdgeCollection endPath = pvis.Path(sink);
            edges.AddRange(endPath);
        }
    }
}
