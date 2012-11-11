using System;

namespace Neo4jClient.Cypher
{
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

        public string Identifier { get { return identifier; } }
        public string IndexName { get { return indexName; } }
        public string Key { get { return key; } }
        public object Value { get { return value; } }

        public string ToCypherText(Func<object, string> createParameterCallback)
        {
            var valueParameter = createParameterCallback(value);
            return string.Format("{0}=node:{1}({2} = {3})",
                identifier,
                indexName,
                key,
                valueParameter);
        }
    }
}
