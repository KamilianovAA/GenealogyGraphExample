namespace GenealogyGraph
{
    public class GenealogyGraphEdge<T>
    {
        public int RelationId { get; set; }
        public string? RelationName { get; set; }
        public string? Prefix { get; set; }
        public bool IsPreBuilt { get; set; } = true;
        public GenealogyGraphNode<T>? Source { get; set; }
        public GenealogyGraphNode<T>? Target { get; set; }

        public GenealogyGraphEdge(string? relationName, int relationId = -1, GenealogyGraphNode<T>? source = null,
            GenealogyGraphNode<T>? target = null)
        {
            RelationName = relationName;
            RelationId = relationId;
            Source = source;
            Target = target;
        }
    }
}
