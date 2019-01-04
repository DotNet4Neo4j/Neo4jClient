using System.Transactions;
using Neo4jClient.Transactions;

namespace Neo4jClient.Execution
{
    internal abstract partial class GraphClientBasedExecutionPolicy : IExecutionPolicy
    {
        public bool InTransaction
        {
            get
            {
                var transactionalGraphClient = Client as ITransactionalGraphClient;
                return transactionalGraphClient != null &&
                       (transactionalGraphClient.InTransaction || Transaction.Current != null);
            }
        }

        public abstract TransactionExecutionPolicy TransactionExecutionPolicy { get; }
    }
}