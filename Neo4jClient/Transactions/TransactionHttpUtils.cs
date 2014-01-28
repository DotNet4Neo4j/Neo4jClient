using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Neo4jClient.Transactions
{
    /// <summary>
    /// Contains utility methods for handling HttpResponseMessages in a transaction scope
    /// </summary>
    internal static class TransactionHttpUtils
    {
        public static IDictionary<string, object> GetMetadataFromResponse(HttpResponseMessage response)
        {
            return response.Headers.ToDictionary(
               headerPair => headerPair.Key,
               headerPair => (object)headerPair.Value);
        }
    }
}
