using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Neo4jClient.Cypher
{
    [DebuggerDisplay("{DebugQueryText}")]
    internal class CypherRelationshipEnumerable : ICypherRelationshipQuery
    {
        readonly IGraphClient client;
        readonly string queryText;
        readonly IDictionary<string, object> queryParameters;
        readonly IList<string> queryDeclaration;

        public CypherRelationshipEnumerable(ICypherQuery query)
        {
            client = query.Client;
            queryText = query.QueryText;
            queryParameters = query.QueryParameters;
            queryDeclaration = query.QueryDeclarations;
        }

        public string DebugQueryText
        {
            get { return this.ToDebugQueryText(); }
        }

        IEnumerator<RelationshipInstance> IEnumerable<RelationshipInstance>.GetEnumerator()
        {
            if (client == null) throw new DetachedNodeException();
            var results = client.ExecuteGetAllRelationshipsCypher(queryText, queryParameters);
            return results.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<RelationshipInstance>)this).GetEnumerator();
        }

        IGraphClient ICypherQuery.Client
        {
            get { return client; }
        }

        string ICypherQuery.QueryText
        {
            get { return queryText; }
        }

        IDictionary<string, object> ICypherQuery.QueryParameters
        {
            get { return queryParameters; }
        }

        IList<string> ICypherQuery.QueryDeclarations
        {
            get { return queryDeclaration; }
        }
    }
}