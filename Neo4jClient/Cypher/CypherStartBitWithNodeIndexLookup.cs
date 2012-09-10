namespace Neo4jClient.Cypher
{
    internal class CypherStartBitWithNodeIndexLookup
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
    }
}
