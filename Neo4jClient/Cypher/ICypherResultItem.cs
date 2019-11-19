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
        /// DO NOT USE IN NEW CODE.
        /// Shortcut for As&lt;Node&lt;T&gt;&gt;.
        /// The general trend in both Neo4j and Cypher is moving way from returning node references. Use As&lt;T&gt; instead to return the information you need, without the metadata of the node. For future queries, use indexes, match expressions and well-known domain-specific data to locate the node you need rather than passing in a reference to it. (In the Neo4j 2.0 timeframe, even the START clause is starting to be deprecated in favour of MATCH and WHERE.) For updating or deleting nodes, start to use mutable Cypher instead. Clauses likE CREATE, SET and DELETE will let you do more in the same query, rather than sending extra HTTP calls over the wire for each action.
        /// </summary>
        Node<T> Node<T>();

        /// <summary>
        /// Equivalent to <code>RETURN collect(foo)</code>
        /// http://docs.neo4j.org/chunked/stable/query-aggregation.html#aggregation-collect
        /// </summary>
        IEnumerable<T> CollectAs<T>();

        /// <summary>
        /// Equivalent to <code>RETURN collect(distinct foo)</code>
        /// http://docs.neo4j.org/chunked/stable/query-aggregation.html#aggregation-collect
        /// </summary>
        IEnumerable<T> CollectAsDistinct<T>();

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

        /// <summary>
        /// Equivalent to <code>RETURN labels(foo)</code>
        /// http://docs.neo4j.org/chunked/preview/query-functions-collection.html#functions-labels
        /// </summary>
        IEnumerable<string> Labels();
    }
}
