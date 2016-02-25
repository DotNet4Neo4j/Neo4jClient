namespace Neo4jClient.Execution
{
    internal abstract partial class GraphClientBasedExecutionPolicy : IExecutionPolicy
    {
        public bool InTransaction
        {
            get { return false; }
        }

        public abstract TransactionExecutionPolicy TransactionExecutionPolicy { get; }
    }
}