using System;

namespace Neo4jClient.Cypher
{
    [Obsolete("Use IGraphClient.Cypher.Start(new { foo = Node.ByIndexQuery(…) }) instead. See https://bitbucket.org/Readify/neo4jclient/issue/74/support-nicer-cypher-start-notation for more details about this change.")]
    public class CypherStartBitWithNodeIndexLookupWithSingleParameter : ICypherStartBit
    {
        readonly string identifier;
        readonly string indexName;
        readonly string parameter;

        public CypherStartBitWithNodeIndexLookupWithSingleParameter(string identifier, string indexName, string parameter)
        {
            this.identifier = identifier;
            this.indexName = indexName;
            this.parameter = parameter;
        }

        public string ToCypherText(Func<object, string> createParameterCallback)
        {
            var valueParameter = createParameterCallback(parameter);
            return string.Format("{0}=node:{1}({2})",
                identifier,
                indexName,
                valueParameter);
        }
    }
}
