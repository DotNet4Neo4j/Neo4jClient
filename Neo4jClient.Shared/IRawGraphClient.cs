using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Neo4jClient.Cypher;

namespace Neo4jClient
{
    /// <summary>
    /// These are signatures that our <see cref="GraphClient"/> exposes to
    /// support things like our Cypher infrastructure, but we don't want
    /// people to call directly. (Because there are nicer ways to do it.)
    /// </summary>
    public interface IRawGraphClient : IGraphClient
    {
        IEnumerable<TResult> ExecuteGetCypherResults<TResult>(CypherQuery query);
        Task<IEnumerable<TResult>> ExecuteGetCypherResultsAsync<TResult>(CypherQuery query);
        void ExecuteCypher(CypherQuery query);
        void ExecuteMultipleCypherQueriesInTransaction(IEnumerable<CypherQuery> queries, NameValueCollection customHeaders = null);
        Task ExecuteCypherAsync(CypherQuery query);
    }
}
