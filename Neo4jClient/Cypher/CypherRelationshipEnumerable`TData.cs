using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Neo4jClient.Cypher
{
    [DebuggerDisplay("{DebugQueryText}")]
    internal class CypherRelationshipEnumerable<TData>
        : ICypherRelationshipQuery<TData>
        where TData : class, new()
    {
        readonly IGraphClient client;
        readonly string queryText;
        readonly IDictionary<string, object> queryParameters;
        readonly IList<string> queryDeclarations;

        public CypherRelationshipEnumerable(ICypherQuery query)
        {
            client = query.Client;
            queryText = query.QueryText;
            queryParameters = query.QueryParameters;
            queryDeclarations = query.QueryDeclarations;
        }

        public string DebugQueryText
        {
            get
            {
                var text = queryText;
                foreach (var key in queryParameters.Keys.Reverse())
                {
                    text = text.Replace(key, string.Format("'{0}'", queryParameters[key]));
                }
                return text;
            }
        }

        IEnumerator<RelationshipInstance<TData>> GetEnumeratorInternal()
        {
            if (client == null) throw new DetachedNodeException();
            var results = client.ExecuteGetAllRelationshipsCypher<TData>(queryText, queryParameters);
            return results.GetEnumerator();
        }

        IEnumerator<RelationshipInstance<TData>> IEnumerable<RelationshipInstance<TData>>.GetEnumerator()
        {
            return GetEnumeratorInternal();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumeratorInternal();
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
            get { return queryDeclarations; }
        }
    }
}