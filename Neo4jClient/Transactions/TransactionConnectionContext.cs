using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neo4jClient.Transactions
{
    internal class TransactionConnectionContext
    {
        public ITransactionalGraphClient Client { get; set; }

    }
}
