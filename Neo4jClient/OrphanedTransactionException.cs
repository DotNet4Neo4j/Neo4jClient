using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neo4jClient
{
    public class OrphanedTransactionException : Exception
    {
        private readonly NeoException _internalException;
        private readonly string _transactionEndpoint;

        internal OrphanedTransactionException(NeoException internalException, string transactionEndpoint)
            : base("The transaction has timed out and it has been rolled back by the server")
        {
            _internalException = internalException;
            _transactionEndpoint = transactionEndpoint;
        }

        public NeoException InternalException
        {
            get { return _internalException; }
        }

        public string TransactionEndpoint
        {
            get { return _transactionEndpoint; }
        }
    }
}
