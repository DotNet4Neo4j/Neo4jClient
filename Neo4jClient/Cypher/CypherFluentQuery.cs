using System.Diagnostics;

namespace Neo4jClient.Cypher
{
    [DebuggerDisplay("{Query.DebugQueryText}")]
    public partial class CypherFluentQuery :
        ICypherFluentQueryPreStart,
        ICypherFluentQueryStarted,
        ICypherFluentQueryMatched,
        IAttachedReference
    {
        protected readonly IGraphClient Client;
        protected readonly CypherQueryBuilder Builder;

        public CypherFluentQuery(IGraphClient client)
            : this(client, new CypherQueryBuilder())
        {
        }

        protected CypherFluentQuery(IGraphClient client, CypherQueryBuilder builder)
        {
            Client = client;
            Builder = builder;
        }

        public ICypherFluentQueryStarted Start(string identity, params NodeReference[] nodeReferences)
        {
            var newBuilder = new CypherQueryBuilder();
            newBuilder.AddStartBit(identity, nodeReferences);
            return new CypherFluentQuery(Client, newBuilder);
        }

        public ICypherFluentQueryStarted Start(string identity, params RelationshipReference[] relationshipReferences)
        {
            var newBuilder = new CypherQueryBuilder();
            newBuilder.AddStartBit(identity, relationshipReferences);
            return new CypherFluentQuery(Client, newBuilder);
        }

        public ICypherFluentQueryStarted AddStartPoint(string identity, params NodeReference[] nodeReferences)
        {
            var newBuilder = Builder.AddStartBit(identity, nodeReferences);
            return new CypherFluentQuery(Client, newBuilder);
        }

        public ICypherFluentQueryStarted AddStartPoint(string identity, params RelationshipReference[] relationshipReferences)
        {
            var newBuilder = Builder.AddStartBit(identity, relationshipReferences);
            return new CypherFluentQuery(Client, newBuilder);
        }

        public ICypherFluentQueryMatched Match(string matchText)
        {
            var newBuilder = Builder.SetMatchText(matchText);
            return new CypherFluentQuery(Client, newBuilder);
        }

        public ICypherFluentQueryMatched Relate(string relateText)
        {
            var newBuilder = Builder.SetRelateText(relateText);
            return new CypherFluentQuery(Client, newBuilder);
        }

        public CypherQuery Query
        {
            get { return Builder.ToQuery(); }
        }

        IGraphClient IAttachedReference.Client
        {
            get { return Client; }
        }
    }
}
