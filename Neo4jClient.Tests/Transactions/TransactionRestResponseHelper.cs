using System;

namespace Neo4jClient.Tests.Transactions
{
    internal static class TransactionRestResponseHelper
    {
        internal static string ResetTransactionTimer()
        {
            return new DateTime().AddSeconds(60).ToString("ddd, dd, MMM yyyy HH:mm:ss +0000");
        }

        internal static string GenerateInitTransactionResponse(int id, string results)
        {
            return string.Format(
                @"{{'commit': 'http://foo/db/data/transaction/{0}/commit', 'results': [{1}], 'errors': [], 'transaction': {{ 'expires': '{2}' }} }}",
                id,
                results, ResetTransactionTimer()
                );
        }

        internal static string GenerateInitTransactionResponse(int id)
        {
            return GenerateInitTransactionResponse(id, string.Empty);
        }

        internal static string GenerateCypherErrorResponse(int id,  string error, string results = "")
        {
            return string.Format(
                @"{{'commit': 'http://foo/db/data/transaction/{0}/commit', 'results': [{1}], 'errors': [{2}], 'transaction': {{ 'expires': '{3}' }} }}",
                id,
                results,
                error, 
                ResetTransactionTimer()
                );
        }
    }
}