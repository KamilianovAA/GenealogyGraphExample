using GenealogyGraph;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Layout.Layered;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenealogyGraphVisualizerExample
{
    public class WpfGraphBuilder : ITreeVisualizer<GenealogyGraph<CsvRecord>, Graph>
    {
        public Graph VisualizeTree(GenealogyGraph<CsvRecord> tree)
        {
            var graph = new Graph();
            graph.Attr.LayerDirection = LayerDirection.LR;
            var layoutSettings = graph.LayoutAlgorithmSettings;
            layoutSettings.EdgeRoutingSettings.KeepOriginalSpline = true;
            layoutSettings.ClusterMargin = 3;
            layoutSettings.NodeSeparation = 10;
            AddNodesToGraph(graph, tree);
            return graph;
        }

        private void AddNodesToGraph(Graph graph, GenealogyGraph<CsvRecord> tree)
        {
            bool first = true;
            foreach (var n in tree.Nodes)
            {
                var node = new Node(n.ID.ToString());
                node.LabelText = n.Value?.FIO;
                if (first)
                {
                    first = false;
                    node.Attr.Color = Color.Red;
                }
                graph.AddNode(node);
            }
            foreach (var n in graph.Nodes)
            {
                var inEdges = tree.Edges.Where(x => x.Target?.ID.ToString() == n.Id);
                foreach (var inEdge in inEdges)
                {
                    var sourceNode = graph.Nodes.Where(x => inEdge.Source?.ID.ToString() == x.Id).FirstOrDefault();
                    if (n.InEdges.Select(x => x.SourceNode).Contains(sourceNode)) continue;
                    var edge = new Edge(sourceNode, n, ConnectionToGraph.Connected);
                    edge.LabelText = inEdge.RelationName;
                    edge.UserData = Tuple.Create(inEdge.RelationId, inEdge.IsPreBuilt);
                    n.AddInEdge(edge);
                    sourceNode?.AddOutEdge(edge);
                }
            }
        }
    }
}
