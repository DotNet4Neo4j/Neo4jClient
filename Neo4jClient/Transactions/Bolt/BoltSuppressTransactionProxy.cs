using System;
using System.Threading.Tasks;

namespace Neo4jClient.Transactions.Bolt
{
    using Neo4j.Driver;

    internal class BoltSuppressTransactionProxy : BoltTransactionScopeProxy
    {
        public BoltSuppressTransactionProxy(ITransactionalGraphClient client)
            : base(client, null)
        {
        }

        public override string Database { get; set; }

        protected override bool ShouldDisposeTransaction()
        {
            return false;
        }

        protected override Task DoCommitAsync()
        {
            throw new InvalidOperationException("Committing during a suppressed transaction scope");
        }

        public override bool Committable => false;

        public override Task RollbackAsync()
        {
            throw new InvalidOperationException("Rolling back during a suppressed transaction scope");
        }

        #pragma warning disable 1998
        public override async Task KeepAliveAsync() { }
        #pragma warning restore 1998

        public override bool IsOpen => true;
        public override Bookmark LastBookmark => throw new NotImplementedException();
    }
}