namespace GenealogyGraph
{
    public class GenealogyGraph<T>
    {
        public List<GenealogyGraphNode<T>> Nodes { get; set; }
        public List<GenealogyGraphEdge<T>> Edges { get; set; }

        public GenealogyGraph()
        {
            Edges = new List<GenealogyGraphEdge<T>>();
            Nodes = new List<GenealogyGraphNode<T>>();
        }

        public GenealogyGraph(GenealogyGraphNode<T> startingNode)
        {
            Edges = new List<GenealogyGraphEdge<T>>();
            Nodes = new List<GenealogyGraphNode<T>>() { startingNode };
        }
    }
}
