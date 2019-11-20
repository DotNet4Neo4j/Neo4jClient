using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Neo4j.Driver.V1;
using Neo4jClient.Cypher;
using Neo4jClient.Execution;
using Newtonsoft.Json;

namespace Neo4jClient.Transactions
{
    /// <summary>
    /// Encapsulates a transaction object with its transaction scheduler.
    /// </summary>
    /// <remarks>
    /// All requests to the same transaction have to made sequentially. The purpose of this class is to ensure
    /// that such calls are made in that fashion.
    /// </remarks>
    internal class BoltTransactionContext : ITransaction
    {
        /// <summary>
        /// The Neo4j transaction object.
        /// </summary>
        public ITransaction Transaction { get; }

        internal BoltNeo4jTransaction BoltTransaction => Transaction as BoltNeo4jTransaction;

        /// <summary>
        /// This replaces the synchronous queue. It is picked up and replaced atomically.
        /// </summary>
        private Task previousTask;

        public NameValueCollection CustomHeaders { get; set; }
        
        public BoltTransactionContext(ITransaction transaction)
        {
            Transaction = transaction;
        }
        
        public async Task<BoltResponse> EnqueueTask(string commandDescription, BoltGraphClient graphClient, IExecutionPolicy policy, CypherQuery query)
        {
            var taskCompletion = new TaskCompletionSource<BoltResponse>();
            try
            {
                var localPreviousTask = Interlocked.Exchange(ref previousTask, taskCompletion.Task);
                if (localPreviousTask != null) await localPreviousTask.ConfigureAwait(false);
                var result = await BoltTransaction.DriverTransaction.RunAsync(query, graphClient);
                var resp = new BoltResponse {StatementResult = result};
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

        public async Task CommitAsync()
        {
            await PreventAddingAndWait().ConfigureAwait(false);
            
            if (CustomHeaders != null)
            {
                Transaction.CustomHeaders = CustomHeaders;
            }
            
            await Transaction.CommitAsync().ConfigureAwait(false);
        }

        private async Task PreventAddingAndWait()
        {
            var cancelled = new TaskCompletionSource<BoltResponse>();
            cancelled.SetCanceled();
            await Interlocked.Exchange(ref previousTask, cancelled.Task).ConfigureAwait(false); // cancel any newly created tasks
        }
        
        public async Task RollbackAsync()
        {
            await PreventAddingAndWait().ConfigureAwait(false);
            await Transaction.RollbackAsync().ConfigureAwait(false);
        }

        public Task KeepAliveAsync()
        {
            return Transaction.KeepAliveAsync();
        }

        public bool IsOpen => Transaction.IsOpen;

    }
}
