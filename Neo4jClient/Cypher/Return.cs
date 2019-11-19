using System;

namespace Neo4jClient.Cypher
{
    public static class Return
    {
        /// <summary>
        /// Used for Cypher <code>RETURN</code> clauses, like <code>Return(() => new { Foo = Return.As&lt;string&gt;("weird_func(foo).wow") })</code>
        /// </summary>
        public static T As<T>(string cypherText)
        {
            throw new InvalidOperationException("This method should only be called from lambda expressions in the RETURN clause of Cypher fluent queries. Do not call it directly: it is just syntactic sugar, there is no .NET implementation.");
        }
    }
}
