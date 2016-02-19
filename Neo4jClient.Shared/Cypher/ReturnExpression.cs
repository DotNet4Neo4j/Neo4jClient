namespace Neo4jClient.Cypher
{
    public struct ReturnExpression
    {
        public string Text { get; set; }
        public CypherResultMode ResultMode { get; set; }
        public CypherResultFormat ResultFormat { get; set; }
    }
}
