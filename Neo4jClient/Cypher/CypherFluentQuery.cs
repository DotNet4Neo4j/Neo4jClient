using System.Diagnostics;

namespace Neo4jClient.Cypher
{
    [DebuggerDisplay("{Query.QueryText}")]
    public class CypherFluentQuery :
        ICypherFluentQueryPreStart,
        ICypherFluentQueryStarted
    {
        readonly CypherQueryBuilder builder;

        public CypherFluentQuery(IGraphClient client)
        {
            builder = new CypherQueryBuilder();
        }

        public ICypherFluentQueryStarted Start(string identity, params NodeReference[] nodeReferences)
        {
            builder.AddStartBit(identity, nodeReferences);
            return this;
        }

        public ICypherFluentQueryStarted Start(string identity, params RelationshipReference[] relationshipReferences)
        {
            builder.AddStartBit(identity, relationshipReferences);
            return this;
        }

        public ICypherFluentQueryStarted AddStartPoint(string identity, params NodeReference[] nodeReferences)
        {
            builder.AddStartBit(identity, nodeReferences);
            return this;
        }

        public ICypherFluentQueryStarted AddStartPoint(string identity, params RelationshipReference[] relationshipReferences)
        {
            builder.AddStartBit(identity, relationshipReferences);
            return this;
        }

        ICypherFluentQuery ICypherFluentQueryStarted.Return(params string[] identities)
        {
            builder.ReturnIdentites = identities;
            return this;
        }

        public ICypherQuery Query
        {
            get { return builder.ToQuery(); }
        }
    }
}
