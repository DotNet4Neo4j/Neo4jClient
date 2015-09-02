namespace Neo4jClient.Cypher
{
    public class CypherCapabilities
    {
        public readonly static CypherCapabilities Cypher19 = new CypherCapabilities
        {
            SupportsPropertySuffixesForControllingNullComparisons = true,
            SupportsNullComparisonsWithIsOperator = true,
            SupportsPlanner = false
        };

        public readonly static CypherCapabilities Cypher20 = new CypherCapabilities
        {
            SupportsPropertySuffixesForControllingNullComparisons = false,
            SupportsNullComparisonsWithIsOperator = false,
            SupportsPlanner = false
        };


        public static readonly CypherCapabilities Cypher22 = new CypherCapabilities
        {
            SupportsPlanner = true,
            SupportsPropertySuffixesForControllingNullComparisons = false,
            SupportsNullComparisonsWithIsOperator = false,
        };

        public static readonly CypherCapabilities Default = Cypher20;

        public bool SupportsPlanner { get; set; }
        public bool SupportsPropertySuffixesForControllingNullComparisons { get; set; }
        public bool SupportsNullComparisonsWithIsOperator { get; set; }
    }
}
