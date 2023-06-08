namespace GenealogyGraph
{
    public class GenealogyGraphNode<T>
    {
        public T? Value { get; set; }
        /// <summary>
        /// Unique identifier for a node
        /// </summary>
        public int ID { get; set; }
        public char Gender { get; set; }
        public Dictionary<GenealogyGraphNode<T>, GenealogyGraphEdge<T>> RelatedNodes { get; set; } = new Dictionary<GenealogyGraphNode<T>, GenealogyGraphEdge<T>>();
        public List<GenealogyGraphEdge<CsvRecord>> InEdges { get; set; } = new List<GenealogyGraphEdge<CsvRecord>>();
        public List<GenealogyGraphEdge<CsvRecord>> OutEdges { get; set; } = new List<GenealogyGraphEdge<CsvRecord>>();

        public GenealogyGraphNode(int id, T? value)
        {
            ID = id;
            Value = value;
        }
    }
}
