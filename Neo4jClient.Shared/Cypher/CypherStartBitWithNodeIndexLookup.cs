using System;

namespace Neo4jClient.Cypher
{
    [Obsolete("Use IGraphClient.Cypher.Start(new { foo = Node.ByIndexLookup(…) }) instead. See https://bitbucket.org/Readify/neo4jclient/issue/74/support-nicer-cypher-start-notation for more details about this change.")]
    public class CypherStartBitWithNodeIndexLookup : ICypherStartBit
    {
        readonly string identifier;
        readonly string indexName;
        readonly string key;
        readonly object value;

        public CypherStartBitWithNodeIndexLookup(string identifier, string indexName, string key, object value)
        {
            this.identifier = identifier;
            this.indexName = indexName;
            this.key = key;
            this.value = value;
        }

        public string ToCypherText(Func<object, string> createParameterCallback)
        {
            var valueParameter = createParameterCallback(value);
            return string.Format("{0}=node:`{1}`({2} = {3})",
                identifier,
                indexName,
                key,
                valueParameter);
        }
    }
}
