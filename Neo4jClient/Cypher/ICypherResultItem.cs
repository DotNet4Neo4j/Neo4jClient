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
        /// http://docs.neo4j.org/chunked/stable/query-aggregation.html#aggregation-collect
        /// </summary>
        IEnumerable<Node<T>> CollectAs<T>();

        /// <summary>
        /// Equivalent to <code>RETURN collect(distinct foo)</code>
        /// http://docs.neo4j.org/chunked/stable/query-aggregation.html#aggregation-collect
        /// </summary>
        IEnumerable<Node<T>> CollectAsDistinct<T>();

        /// <summary>
        /// Equivalent to <code>RETURN head()</code>
        /// http://docs.neo4j.org/chunked/stable/query-functions-scalar.html#functions-head
        /// </summary>
        IFluentCypherResultItem Head();

        /// <summary>
        /// Equivalent to <code>RETURN last()</code>
        /// http://docs.neo4j.org/chunked/stable/query-functions-scalar.html#functions-last
        /// </summary>
        IFluentCypherResultItem Last();

        /// <summary>
        /// Equivalent to <code>RETURN count(foo)</code>
        /// http://docs.neo4j.org/chunked/stable/query-aggregation.html#_count
        /// </summary>
        long Count();

        /// <summary>
        /// Equivalent to <code>RETURN count(distinct foo)</code>
        /// http://docs.neo4j.org/chunked/stable/query-aggregation.html#_count
        /// http://docs.neo4j.org/chunked/stable/query-aggregation.html#aggregation-distinct
        /// </summary>
        long CountDistinct();

        /// <summary>
        /// Equivalent to <code>RETURN length(foo)</code>
        /// http://docs.neo4j.org/chunked/stable/query-function.html#functions-length
        /// </summary>
        long Length();

        /// <summary>
        /// Equivalent to <code>RETURN type(foo)</code>
        /// http://docs.neo4j.org/chunked/stable/query-function.html#functions-type
        /// </summary>
        string Type();

        /// <summary>
        /// Equivalent to <code>RETURN id(foo)</code>
        /// http://docs.neo4j.org/chunked/stable/query-function.html#functions-id
        /// </summary>
        long Id();
    }
}
