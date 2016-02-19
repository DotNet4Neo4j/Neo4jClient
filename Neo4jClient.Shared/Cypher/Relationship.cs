using Neo4jClient.Cypher;

// There's already a Neo4jClient.Relationship class that we're going to conflict with
// if we try to have Neo4jClient.Cypher.Relationship. Instead, we keep the code here
// in the Cypher folder and just mash it into the root type. It's ugly, but it works
// well enough for now. Sometime in the not too distant future, I'm also hoping to redo
// the relationship infrastructure, and we'll probably remove that type then.
// ReSharper disable CheckNamespace
namespace Neo4jClient
// ReSharper restore CheckNamespace
{
    public partial class Relationship
    {
        /// <summary>
        /// Used for Cypher <code>START</code> clauses, like <code>Start(new { foo = Relationship.ByIndexLookup(…) })</code>
        /// </summary>
        public static StartBit ByIndexLookup(string indexName, string propertyName, object value)
        {
            return new StartBit(createParameterCallback =>
                string.Format(
                    "relationship:`{0}`({1} = {2})",
                    indexName,
                    propertyName,
                    createParameterCallback(value)));
        }

        /// <summary>
        /// Used for Cypher <code>START</code> clauses, like <code>Start(new { foo = Relationship.ByIndexQuery(…) })</code>
        /// </summary>
        public static StartBit ByIndexQuery(string indexName, string query)
        {
            return new StartBit(createParameterCallback =>
                string.Format(
                    "relationship:`{0}`({1})",
                    indexName,
                    createParameterCallback(query)));
        }
    }
}
