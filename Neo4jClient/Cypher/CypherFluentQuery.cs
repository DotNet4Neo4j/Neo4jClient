using System;
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
        protected readonly IRawGraphClient Client;
        protected readonly CypherQueryBuilder Builder;

        public CypherFluentQuery(IGraphClient client)
            : this(client, new CypherQueryBuilder())
        {
        }

        protected CypherFluentQuery(IGraphClient client, CypherQueryBuilder builder)
        {
            if (!(client is IRawGraphClient))
                throw new ArgumentException("The supplied graph client also needs to implement IRawGraphClient", "client");

            Client = (IRawGraphClient)client;
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

        public ICypherFluentQueryStarted StartWithNodeIndexLookup(string identity, string indexName, string key, object value)
        {
            var newBuilder = new CypherQueryBuilder();
            newBuilder.AddStartBitWithNodeIndexLookup(identity, indexName, key, value);
            return new CypherFluentQuery(Client, newBuilder);
        }

        public ICypherFluentQueryStarted AddStartPointWithNodeIndexLookup(string identity, string indexName, string key, object value)
        {
            var newBuilder = Builder.AddStartBitWithNodeIndexLookup(identity, indexName, key, value);
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

        public ICypherFluentQueryMatched Match(params string[] matchText)
        {
            var newBuilder = Builder.SetMatchText(string.Join(", ", matchText));
            return new CypherFluentQuery(Client, newBuilder);
        }

        public ICypherFluentQueryMatched Relate(string relateText)
        {
            if (Client.ServerVersion == new Version(1, 8) ||
                Client.ServerVersion >= new Version(1, 8, 0, 7))
                throw new NotSupportedException("You're trying to use the RELATE keyword against a Neo4j instance ≥ 1.8M07. In Neo4j 1.8M07, it was renamed from RELATE to CREATE UNIQUE. You need to update your code to use our new CreateUnique method. (We didn't want to just plumb the Relate method to CREATE UNIQUE, because that would introduce a deviation between the .NET wrapper and the Cypher language.)\r\n\r\nSee https://github.com/systay/community/commit/c7dbbb929abfef600266a20f065d760e7a1fff2e for detail.");

            var newBuilder = Builder.SetRelateText(relateText);
            return new CypherFluentQuery(Client, newBuilder);
        }

        public ICypherFluentQueryMatched CreateUnique(string createUniqueText)
        {
            if (Client.ServerVersion < new Version(1, 8) ||
                (Client.ServerVersion >= new Version(1, 8, 0, 1) && Client.ServerVersion <= new Version(1, 8, 0, 6)))
                throw new NotSupportedException("The CREATE UNIQUE clause was only introduced in Neo4j 1.8M07, but you're querying against an older version of Neo4j. You'll want to upgrade Neo4j, or use the RELATE keyword instead. See https://github.com/systay/community/commit/c7dbbb929abfef600266a20f065d760e7a1fff2e for detail.");

            var newBuilder = Builder.SetCreateUniqueText(createUniqueText);
            return new CypherFluentQuery(Client, newBuilder);
        }

        public ICypherFluentQueryMatched Create(string createText)
        {
            var newBuilder = Builder.SetCreateText(createText);
            return new CypherFluentQuery(Client, newBuilder);
        }

        public ICypherFluentQueryMatched Delete(string identities)
        {
            var newBuilder = Builder.SetDeleteText(identities);
            return new CypherFluentQuery(Client, newBuilder);
        }

        public CypherQuery Query
        {
            get { return Builder.ToQuery(); }
        }

        public void ExecuteWithoutResults()
        {
            Client.ExecuteCypher(Query);
        }

        IGraphClient IAttachedReference.Client
        {
            get { return Client; }
        }
    }
}
