namespace Neo4jClient.Cypher
{
    public class CypherCapabilities
    {
        public readonly static CypherCapabilities Cypher19 = new CypherCapabilities
        {
            SupportsPropertySuffixesForControllingNullComparisons = true
        };

        public readonly static CypherCapabilities Cypher20 = new CypherCapabilities
        {
            SupportsPropertySuffixesForControllingNullComparisons = false
        };

        public static readonly CypherCapabilities Default = Cypher20;

        public bool SupportsPropertySuffixesForControllingNullComparisons { get; set; }
    }
}
