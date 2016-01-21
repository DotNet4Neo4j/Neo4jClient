namespace Neo4jClient.Transactions
{
    internal interface ITransactionResourceManager
    {
        void Enlist(ITransactionExecutionEnvironment transactionExecutionEnvironment, byte[] transactionToken);
        byte[] Promote(ITransactionExecutionEnvironment transactionExecutionEnvironment);
        void CommitTransaction(int transactionId);
        void RollbackTransaction(int transactionId);
    }
}
