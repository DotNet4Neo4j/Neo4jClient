using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;
using Neo4jClient.Transactions;

namespace Neo4jClient.Execution
{
    /// <summary>
    /// Describes the behavior for a cypher execution.
    /// </summary>
    internal partial class CypherExecutionPolicy : GraphClientBasedExecutionPolicy
    {

        private INeo4jTransaction GetTransactionInScope()
        {
            // first try to get the Non DTC transaction and if it doesn't succeed then try it with the DTC
            var transactionalClient = Client as IInternalTransactionalGraphClient<HttpResponseMessage>;
            if (transactionalClient == null)
            {
                return null;
            }

            var proxiedTransaction = transactionalClient.Transaction as TransactionScopeProxy;
            if (proxiedTransaction != null)
            {
                return proxiedTransaction.TransactionContext;;
            }

            var ambientTransaction = transactionalClient.TransactionManager.CurrentDtcTransaction;
            if (ambientTransaction != null)
            {
                return (INeo4jTransaction) ambientTransaction;
            }

            return null;
        }

        public override Uri BaseEndpoint
        {
            get
            {
                if (!InTransaction)
                {
                    return Client.CypherEndpoint;
                }

                var proxiedTransaction = GetTransactionInScope();
                var transactionalClient = (ITransactionalGraphClient) Client;
                if (proxiedTransaction == null)
                {
                    return transactionalClient.TransactionEndpoint;
                }

                var startingReference = proxiedTransaction.Endpoint ?? transactionalClient.TransactionEndpoint;
                return startingReference;
            }
        }

        public override TransactionExecutionPolicy TransactionExecutionPolicy
        {
            get { return TransactionExecutionPolicy.Allowed; }
        }

     
    }
}
