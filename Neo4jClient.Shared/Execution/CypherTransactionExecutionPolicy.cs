namespace Neo4jClient.Execution
{
    internal class CypherTransactionExecutionPolicy : CypherExecutionPolicy
    {
        public CypherTransactionExecutionPolicy(IGraphClient client)
            : base(client)
        {
        }

        public override TransactionExecutionPolicy TransactionExecutionPolicy
        {
            get { return TransactionExecutionPolicy.Required; }
        }
    }
}