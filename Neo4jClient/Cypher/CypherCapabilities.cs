namespace Neo4jClient.Cypher
{
    public class CypherCapabilities
    {
        public readonly static CypherCapabilities Cypher19 = new CypherCapabilities
        {
            SupportsPropertySuffixesForControllingNullComparisons = true,
            SupportsNullComparisonsWithIsOperator = true
        };

        public readonly static CypherCapabilities Cypher20 = new CypherCapabilities
        {
            SupportsPropertySuffixesForControllingNullComparisons = false,
            SupportsNullComparisonsWithIsOperator = false
        };

        public static readonly CypherCapabilities Default = Cypher20;

        public bool SupportsPropertySuffixesForControllingNullComparisons { get; set; }
        public bool SupportsNullComparisonsWithIsOperator { get; set; }
    }
}
