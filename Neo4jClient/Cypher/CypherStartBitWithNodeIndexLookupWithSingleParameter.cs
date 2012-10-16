namespace Neo4jClient.Cypher
{
    internal class CypherStartBitWithNodeIndexLookupWithSingleParameter
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
    }
}
