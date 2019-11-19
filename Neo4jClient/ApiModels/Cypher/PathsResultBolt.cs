using System.Collections.Generic;
using System.Linq;
using Neo4j.Driver.V1;
using Neo4jClient.Extensions;
using Newtonsoft.Json;

namespace Neo4jClient.ApiModels.Cypher
{
    public class PathsResultBolt
    {
        public PathsResultBolt()
        {
            Nodes = new List<PathsResultBoltNode>();
            Relationships = new List<PathsResultBoltRelationship>();
        }

        internal PathsResultBolt(IPath path)
        {
            Start = new PathsResultBoltNode(path.Start);
            End = new PathsResultBoltNode(path.End);
            Relationships = path.Relationships.Select(r => new PathsResultBoltRelationship(r)).ToList();
            Nodes = path.Nodes.Select(r => new PathsResultBoltNode(r)).ToList();
        }

        [JsonProperty("Start")]
        public PathsResultBoltNode Start { get; set; }

        [JsonProperty("End")]
        public PathsResultBoltNode End { get; set; }

        [JsonIgnore]
        public int Length => Relationships.Count();

        [JsonProperty("Nodes")]
        public List<PathsResultBoltNode> Nodes { get; set; }

        [JsonProperty("Relationships")]
        public List<PathsResultBoltRelationship> Relationships { get; set; }

        public class PathsResultBoltRelationship
        {
            public long Id { get; set; }
            public string Type { get; set; }
            public long StartNodeId { get; set; }
            public long EndNodeId { get; set; }

            public object this[string key] => Properties[key];

            public Dictionary<string, object> Properties { get; set; }

            public PathsResultBoltRelationship() { Properties = new Dictionary<string, object>(); }

            public PathsResultBoltRelationship(IRelationship relationship)
            {
                Id = relationship.Id;
                StartNodeId = relationship.StartNodeId;
                EndNodeId = relationship.EndNodeId;
                Type = relationship.Type;
                Properties = relationship.Properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            public bool Equals(PathsResultBoltRelationship other)
            {
                if (other == null)
                    return false;

                return Id == other.Id
                       && StartNodeId == other.StartNodeId
                       && EndNodeId == other.EndNodeId
                       && Type == other.Type
                       && Properties.ContentsEqual(other.Properties);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as PathsResultBoltRelationship);
            }

            public override int GetHashCode()
            {
                var hashCode = 2105322407;
                hashCode = hashCode * -1521134295 + Id.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Type);
                hashCode = hashCode * -1521134295 + StartNodeId.GetHashCode();
                hashCode = hashCode * -1521134295 + EndNodeId.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<IReadOnlyDictionary<string, object>>.Default.GetHashCode(Properties);
                return hashCode;
            }
        }

        public class PathsResultBoltNode 
        {
            public long Id { get; set; }
            public List<string> Labels { get; set; }
            public object this[string key] => Properties[key];
            public Dictionary<string, object> Properties { get; set; }

            public PathsResultBoltNode() { Properties = new Dictionary<string, object>(); }

            internal PathsResultBoltNode(INode node)
            {
                Id = node.Id;
                Labels = node.Labels?.ToList();
                Properties = node.Properties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            public bool Equals(PathsResultBoltNode other)
            {
                if (other == null)
                    return false;

                return Id == other.Id
                       && Labels.ContentsEqual(other.Labels)
                       && Properties.ContentsEqual(other.Properties);
            }

            public override bool Equals(object obj)
            {
                return Equals( obj as PathsResultBoltNode);
            }

            public override int GetHashCode()
            {
                var hashCode = 1343812023;
                hashCode = hashCode * -1521134295 + Id.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<IReadOnlyList<string>>.Default.GetHashCode(Labels);
                hashCode = hashCode * -1521134295 + EqualityComparer<IReadOnlyDictionary<string, object>>.Default.GetHashCode(Properties);
                return hashCode;
            }
        }
    }
}