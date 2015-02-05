namespace Neo4jClient.Cypher
{
    public class CypherCapabilities
    {
        public readonly static CypherCapabilities Cypher190 = new CypherCapabilities
        {
            SupportsPropertySuffixesForControllingNullComparisons = true,
            SupportsNullComparisonsWithIsOperator = true,
            SupportsUnwindOperator = false
        };

        public readonly static CypherCapabilities Cypher203 = new CypherCapabilities
        {
            SupportsPropertySuffixesForControllingNullComparisons = false,
            SupportsNullComparisonsWithIsOperator = false,
            SupportsUnwindOperator = false
        };

        public static readonly CypherCapabilities Cypher204 = new CypherCapabilities
        {
            SupportsPropertySuffixesForControllingNullComparisons = false,
            SupportsNullComparisonsWithIsOperator = false,
            SupportsUnwindOperator = true
        };

        public static readonly CypherCapabilities Default = Cypher204;

        public bool SupportsPropertySuffixesForControllingNullComparisons { get; set; }
        public bool SupportsNullComparisonsWithIsOperator { get; set; }
        public bool SupportsUnwindOperator { get; set; }
    }
}
