namespace Neo4jClient.Cypher
{
    public enum CypherPlanner
    {
        /// <summary>The rule based planner (RULE)</summary>
        Rule,
        /// <summary>The new cost based planner (IDP)</summary>
        CostIdp,
        /// <summary>The first cost based planner (COST)</summary>
        CostGreedy
    }
}