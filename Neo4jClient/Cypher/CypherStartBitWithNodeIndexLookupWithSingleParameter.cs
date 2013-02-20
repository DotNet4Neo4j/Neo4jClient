using System;

namespace Neo4jClient.Cypher
{
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

        public string Identifier { get { return identifier; } }
        public string IndexName { get { return indexName; } }
        public string Parameter { get { return parameter; } }

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
