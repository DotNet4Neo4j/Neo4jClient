using System;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using Neo4jClient.Cypher;
using Neo4jClient.Execution;
using Neo4jClient.Transactions.Bolt;

namespace Neo4jClient.Transactions
{
    using Neo4j.Driver;

    internal abstract class TransactionContextBase<TClient, TResponse> : ITransaction
    {
        /// <summary>
        /// This replaces the synchronous queue. It is picked up and replaced atomically.
        /// </summary>
        private Task previousTask;

        protected TransactionContextBase(ITransaction transaction)
        {
            Transaction = transaction;
        }

        /// <summary>
        /// The Neo4j transaction object.
        /// </summary>
        public ITransaction Transaction { get; }

        public NameValueCollection CustomHeaders { get; set; }
        public Bookmark LastBookmark => Transaction.LastBookmark;
        public bool IsOpen => Transaction.IsOpen;

        protected abstract Task<TResponse> RunQuery(TClient client, CypherQuery query, IExecutionPolicy policy, string commandDescription);

        public async Task<TResponse> EnqueueTask(string commandDescription, TClient graphClient, IExecutionPolicy policy, CypherQuery query)
        {
            var taskCompletion = new TaskCompletionSource<TResponse>();
            try
            {
                var localPreviousTask = Interlocked.Exchange(ref previousTask, taskCompletion.Task);
                if (localPreviousTask != null) await localPreviousTask.ConfigureAwait(false);
                var resp = await RunQuery(graphClient, query, policy, commandDescription).ConfigureAwait(false);
                taskCompletion.SetResult(resp);
            }
            catch (OperationCanceledException)
            {
                taskCompletion.SetCanceled();
            }
            catch (Exception e)
            {
                taskCompletion.SetException(e);
            }

            return await taskCompletion.Task.ConfigureAwait(false);
        }

        public void Dispose()
        {
            Transaction.Dispose();
        }

        public string Database
        {
            get => Transaction.Database;
            // set => Transaction.Database = value;
        }

        public async Task CommitAsync()
        {
            // await PreventAddingAndWait().ConfigureAwait(false);
            
            if (CustomHeaders != null)
            {
                Transaction.CustomHeaders = CustomHeaders;
            }
            
            await Transaction.CommitAsync().ConfigureAwait(false);
        }

        // private async Task PreventAddingAndWait()
        // {
        //     var cancelled = new TaskCompletionSource<BoltResponse>();
        //     cancelled.SetCanceled();
        //     var task = Interlocked.Exchange(ref previousTask, cancelled.Task); // cancel any newly created tasks
        //     if (task != null) await task.ConfigureAwait(false);
        // }

        public async Task RollbackAsync()
        {
            //await PreventAddingAndWait().ConfigureAwait(false);
            await Transaction.RollbackAsync().ConfigureAwait(false);
        }

        public Task KeepAliveAsync()
        {
            return Transaction.KeepAliveAsync();
        }
    }
}