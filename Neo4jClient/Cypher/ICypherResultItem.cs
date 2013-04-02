using System.Collections.Generic;

namespace Neo4jClient.Cypher
{
    public interface ICypherResultItem
    {
        /// <summary>
        /// Does not emit anything into the query; purely used for design-time type coersion in C#
        /// </summary>
        T As<T>();

        /// <summary>
        /// Shortcut for As&lt;Node&lt;T&gt;&gt;
        /// </summary>
        Node<T> Node<T>();

        /// <summary>
        /// Equivalent to <code>RETURN collect(foo)</code>
        /// http://docs.neo4j.org/chunked/stable/query-function.html#functions-collect
        /// </summary>
        IEnumerable<Node<T>> CollectAs<T>();

        /// <summary>
        /// Equivalent to <code>RETURN collect(distinct foo)</code>
        /// http://docs.neo4j.org/chunked/stable/query-function.html#functions-collect
        /// </summary>
        IEnumerable<Node<T>> CollectAsDistinct<T>();
    }
}
