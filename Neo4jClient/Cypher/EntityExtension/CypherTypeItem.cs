using System;

namespace Neo4jClient.Cypher.EntityExtension
{
    public class CypherTypeItem : IEquatable<CypherTypeItem>
    {
        public Type Type { get; set; }
        public Type AttributeType { get; set; }

        public bool Equals(CypherTypeItem other)
        {
            return other.Type == Type && other.AttributeType == AttributeType;
        }

        bool IEquatable<CypherTypeItem>.Equals(CypherTypeItem other)
        {
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode() ^ AttributeType.GetHashCode();
        }
    }
}
