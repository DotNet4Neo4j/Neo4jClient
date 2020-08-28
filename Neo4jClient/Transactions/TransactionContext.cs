using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Neo4jClient.Cypher;
using Neo4jClient.Execution;

namespace Neo4jClient.Transactions
{
    /// <summary>
    /// Encapsulates a transaction object with its transaction scheduler.
    /// </summary>
    /// <remarks>
    /// All requests to the same transaction have to made sequentially. The purpose of this class is to ensure
    /// that such calls are made in that fashion.
    /// </remarks>
    internal class TransactionContext : TransactionContextBase<IGraphClient, HttpResponseMessage>, INeo4jTransaction
    {
        public INeo4jTransaction NeoTransaction => Transaction as INeo4jTransaction;
        
        public TransactionContext(INeo4jTransaction transaction): base(transaction)
        {
        }

        protected override Task<HttpResponseMessage> RunQuery(IGraphClient client, CypherQuery query, IExecutionPolicy policy, string commandDescription)
        {
            
            var txBaseEndpoint = policy.BaseEndpoint((policy.InTransaction) ? policy.Database : query.Database, !policy.InTransaction);
            var serializedQuery = policy.SerializeRequest(query);
            CustomHeaders = query.CustomHeaders;
            return Request.With(client.ExecutionConfiguration, query.CustomHeaders, query.MaxExecutionTime)
                .Post(Endpoint ?? txBaseEndpoint)
                .WithJsonContent(serializedQuery)
                // HttpStatusCode.Created may be returned when emitting the first query on a transaction
                .WithExpectedStatusCodes(HttpStatusCode.OK, HttpStatusCode.Created)
                .ExecuteAsync(
                    commandDescription,
                    response =>
                    {
                        // we need to check for errors returned by the transaction. The difference with a normal REST cypher
                        // query is that the errors are embedded within the result object, instead of having a 400 bad request
                        // status code.
                        policy.AfterExecution(TransactionHttpUtils.GetMetadataFromResponse(response), this);

                        return response;
                    });
        }

        public Uri Endpoint
        {
            get => NeoTransaction.Endpoint;
            set => NeoTransaction.Endpoint = value;
        }
    }
}
