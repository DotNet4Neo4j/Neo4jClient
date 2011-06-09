namespace Neo4jClient
{
    internal class NodePacket<TNode>
    {
        public string Self { get; set; }
        public TNode Data { get; set; }
    }
}