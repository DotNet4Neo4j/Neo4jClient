using System;

namespace Neo4jClient.Cypher
{
    /// <summary>
    /// Represents a star in a Cypher function, so <code>All.Count()</code>
    /// is equivalent to <code>count(*)</code>. Only for use in return expressions
    /// like <code>.Return(() => new { Count = All.Count() })</code>, or start
    /// expressions like <code>Start(new { n = All.Nodes })</code>, but not to be
    /// called directly. (This class is just syntactic sugar for lambda expressions;
    /// there is no .NET implementation of its methods.)
    /// </summary>
    public abstract class All
    {
        /// <summary>
        /// Equivalent to <code>count(*)</code>
        /// http://docs.neo4j.org/chunked/stable/query-aggregation.html#_count
        /// </summary>
        public static long Count()
        {
            throw new InvalidOperationException("This method can't be executed directly: it has no .NET implementation. You need to use it as part of a Cypher return expression, like .Return(() => new { Count = All.Count() });");
        }

        /// <summary>
        /// Equivalent to <code>node(*)</code>, for use in <code>START</code> clauses
        /// such as <code>Start(new { n = All.Nodes })</code>
        /// </summary>
        public const string Nodes = "node(*)";
    }
}
