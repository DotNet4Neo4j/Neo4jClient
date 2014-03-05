using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neo4jClient.Transactions
{
    /// <summary>
    /// Exposes the same methods and members as ITransactionalGraphClient, however it is used
    /// internally to access the ITransactionManager that the GraphClient uses.
    /// </summary>
    internal interface IInternalTransactionalGraphClient : ITransactionalGraphClient
    {
        ITransactionManager TransactionManager { get; }
    }
}
