using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neo4jClient.Transactions;

namespace Neo4jClient.Execution
{
    internal abstract class GraphClientBasedExecutionPolicy : IExecutionPolicy
    {
        protected IGraphClient Client;

        protected GraphClientBasedExecutionPolicy(IGraphClient client)
        {
            Client = client;
        }

        public bool InTransaction
        {
            get
            {
                var transactionalGraphClient = Client as ITransactionalGraphClient;
                return transactionalGraphClient != null && transactionalGraphClient.Transaction != null;
            }
        }

        public abstract TransactionExecutionPolicy TransactionExecutionPolicy { get; }
        public abstract void AfterExecution(IDictionary<string, object> executionMetadata);
        
        public virtual string SerializeRequest(object toSerialize)
        {
            return Client.Serializer.Serialize(toSerialize);
        }

        public abstract Uri BaseEndpoint { get; }
        public virtual Uri AddPath(Uri startUri, object startReference)
        {
            return startUri;
        }
    }
}
