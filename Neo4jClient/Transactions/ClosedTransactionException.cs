using System;

namespace Neo4jClient.Transactions
{
    public class ClosedTransactionException : Exception
    {
        private readonly string _transactionEndpoint;

        public ClosedTransactionException(string transactionEndpoint)
            : base("The transaction has been committed or rolled back.")
        {
            _transactionEndpoint = string.IsNullOrEmpty(transactionEndpoint) ? 
                "No transaction endpoint. No requests were made for the transaction." : 
                transactionEndpoint;
        }

        public string TransactionEndpoint
        {
            get { return _transactionEndpoint; }
        }
    }
}
