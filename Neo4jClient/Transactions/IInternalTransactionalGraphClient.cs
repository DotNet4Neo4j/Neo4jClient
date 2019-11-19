namespace Neo4jClient.Transactions
{
    /// <summary>
    /// Exposes the same methods and members as ITransactionalGraphClient, however it is used
    /// internally to access the ITransactionManager that the GraphClient uses.
    /// </summary>
    internal interface IInternalTransactionalGraphClient<T> : ITransactionalGraphClient
    {
        ITransactionManager<T> TransactionManager { get; }
    }
}
