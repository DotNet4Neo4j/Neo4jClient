using System;

namespace Neo4jClient.Transactions
{
    internal interface ITransactionResourceManager
    {
        void Enlist(ITransactionExecutionEnvironment transactionExecutionEnvironment, byte[] transactionToken);
        byte[] Promote(ITransactionExecutionEnvironment transactionExecutionEnvironment);
        void CommitTransaction(int transactionId);
        void RollbackTransaction(int transactionId);

    }

    internal interface ITransactionResourceManagerBolt
    {
        void Enlist(ITransactionExecutionEnvironmentBolt transactionExecutionEnvironment, byte[] transactionToken);
        byte[] Promote(ITransactionExecutionEnvironmentBolt transactionExecutionEnvironment);
        void RollbackTransaction(Guid transactionId);
        void CommitTransaction(Guid transactionId);
    }
}
