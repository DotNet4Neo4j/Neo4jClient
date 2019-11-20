using System;
using System.Collections.Generic;
using System.Transactions;
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

        public bool InTransaction =>
            Client is ITransactionalGraphClient transactionalGraphClient &&
            (transactionalGraphClient.InTransaction || Transaction.Current != null);

        public abstract TransactionExecutionPolicy TransactionExecutionPolicy { get; }

        public abstract void AfterExecution(IDictionary<string, object> executionMetadata, object executionContext);

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