using System;

namespace Neo4jClient.Transactions
{
    public class ClosedTransactionException : Exception
    {
        public ClosedTransactionException(string transactionEndpoint)
            : base("The transaction has been committed or rolled back.")
        {
            TransactionEndpoint = string.IsNullOrEmpty(transactionEndpoint) ? "No transaction endpoint. No requests were made for the transaction." : transactionEndpoint;
        }

        public string TransactionEndpoint { get; }
    }
}
