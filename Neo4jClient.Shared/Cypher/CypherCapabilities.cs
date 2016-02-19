namespace Neo4jClient.Cypher
{
    public class CypherCapabilities
    {
        public CypherCapabilities(){ }

        public CypherCapabilities(CypherCapabilities cypherCapabilities)
        {
            SupportsStartsWith = cypherCapabilities.SupportsStartsWith;
            SupportsPlanner = cypherCapabilities.SupportsPlanner;
            SupportsNullComparisonsWithIsOperator = cypherCapabilities.SupportsNullComparisonsWithIsOperator;
            SupportsPropertySuffixesForControllingNullComparisons = cypherCapabilities.SupportsPropertySuffixesForControllingNullComparisons;
            AutoRollsBackOnError = cypherCapabilities.AutoRollsBackOnError;
        }

        public readonly static CypherCapabilities Cypher19 = new CypherCapabilities
        {
            SupportsPropertySuffixesForControllingNullComparisons = true,
            SupportsNullComparisonsWithIsOperator = true,
        };

        public readonly static CypherCapabilities Cypher20 = new CypherCapabilities
        {
            SupportsPropertySuffixesForControllingNullComparisons = false,
            SupportsNullComparisonsWithIsOperator = false,
        };

        public static readonly CypherCapabilities Cypher22 = new CypherCapabilities(Cypher20){SupportsPlanner = true};
        public static readonly CypherCapabilities Cypher226 = new CypherCapabilities(Cypher22) { AutoRollsBackOnError = true };
        public static readonly CypherCapabilities Cypher23 = new CypherCapabilities(Cypher226) {SupportsStartsWith = true};
        public static readonly CypherCapabilities Default = Cypher20;

        public bool SupportsPlanner { get; set; }
        public bool SupportsPropertySuffixesForControllingNullComparisons { get; set; }
        public bool SupportsNullComparisonsWithIsOperator { get; set; }
        public bool SupportsStartsWith { get; set; }

        public bool AutoRollsBackOnError { get; set; }
    }
}
