namespace Neo4jClient
{
    public class NodePacket<TNode>
    {
        public string Self { get; set; }
        public TNode Data { get; set; }
    }
}