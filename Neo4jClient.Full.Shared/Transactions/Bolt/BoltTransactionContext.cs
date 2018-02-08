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
        public ITransaction Transaction { get; protected set; }

        internal BoltNeo4jTransaction BoltTransaction => Transaction as BoltNeo4jTransaction;

        /// <summary>
        /// The consumer of all the tasks (a single thread)
        /// </summary>
        private Action consumer;
        
        /// <summary>
        /// This is where the producer generates all the tasks
        /// </summary>
        private readonly BlockingCollection<Task> taskQueue;

        public NameValueCollection CustomHeaders { get; set; }

        /// <summary>
        /// Where the cancellation token generates
        /// </summary>
        private readonly CancellationTokenSource cancellationTokenSource;
        
        public BoltTransactionContext(ITransaction transaction)
        {
            Transaction = transaction;
            cancellationTokenSource = new CancellationTokenSource();
            taskQueue = new BlockingCollection<Task>();
        }
        
        public Task<BoltResponse> EnqueueTask(string commandDescription, BoltGraphClient graphClient, IExecutionPolicy policy, CypherQuery query)
        {
            var task = new Task<BoltResponse>(() =>
                {
                   var result = BoltTransaction.DriverTransaction.Run(query, graphClient);
                    
                    var resp = new BoltResponse{StatementResult = result};
                    return resp;
                }
            );
            taskQueue.Add(task, cancellationTokenSource.Token);

            if (consumer == null)
            {
                consumer = () =>
                {
                    while (true)
                    {
                        try
                        {
                            Task queuedTask;
                            if (!taskQueue.TryTake(out queuedTask, 0, cancellationTokenSource.Token))
                            {
                                // no items to consume
                                consumer = null;
                                break;
                            }
                            queuedTask.RunSynchronously();
                        }
                        catch (InvalidOperationException)
                        {
                            // we are done, CompleteAdding has been called 
                            break;
                        }
                        catch (OperationCanceledException)
                        {
                            // we are done, we were canceled
                            break;
                        }
                    }
                };

                consumer.BeginInvoke(null, null);
            }

            return task;
        }

        public void Dispose()
        {
            Transaction.Dispose();
        }

        public void Commit()
        {
            taskQueue.CompleteAdding();
            if (taskQueue.Count > 0)
            {
                cancellationTokenSource.Cancel();
                throw new InvalidOperationException("Cannot commit unless all tasks have been completed");
            }
            if (CustomHeaders != null)
            {
                Transaction.CustomHeaders = CustomHeaders;
            }
            Transaction.Commit();
        }

        public void Rollback()
        {
            taskQueue.CompleteAdding();
            cancellationTokenSource.Cancel();
            Transaction.Rollback();
        }

        public void KeepAlive()
        {
            Transaction.KeepAlive();
        }

        public bool IsOpen => Transaction.IsOpen;

    }
}
