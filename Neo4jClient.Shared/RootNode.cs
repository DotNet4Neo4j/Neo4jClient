using System;

namespace Neo4jClient
{
    [Obsolete("The concept of a single root node has being dropped in Neo4j 2.0. Use an alternate strategy for having known reference points in the graph, such as labels.")]
    public class RootNode : NodeReference<RootNode>
    {
        public RootNode() : base(0) { }
        public RootNode(long id) : base(id) {}
        public RootNode(long id, IGraphClient client) : base(id, client) {}
    }
}
