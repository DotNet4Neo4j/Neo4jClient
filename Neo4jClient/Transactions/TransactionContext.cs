using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Neo4jClient.Cypher;
using Neo4jClient.Execution;

namespace Neo4jClient.Transactions
{
    /// <summary>
    /// Encapsulates a transaction object with its transaction scheduler.
    /// </summary>
    /// <remarks>
    /// All requests to the same transaction have to made sequentially. The purpose of this class is to ensure
    /// that such calls are made in that fashion.
    /// </remarks>
    internal class TransactionContext : INeo4jTransaction
    {
        /// <summary>
        /// The Neo4j transaction object.
        /// </summary>
        public INeo4jTransaction Transaction { get; protected set; }

        /// <summary>
        /// The consumer of all the tasks (a single thread)
        /// </summary>
        private Action _consumer = null;
        
        /// <summary>
        /// This is where the producer generates all the tasks
        /// </summary>
        private BlockingCollection<Task> _taskQueue;

        public NameValueCollection CustomHeaders { get; set; }

        /// <summary>
        /// Where the cancellation token generates
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;
        
        public TransactionContext(INeo4jTransaction transaction)
        {
            Transaction = transaction;
            _cancellationTokenSource = new CancellationTokenSource();
            _taskQueue = new BlockingCollection<Task>();
        }

        public Task<HttpResponseMessage> EnqueueTask(string commandDescription, IGraphClient client, IExecutionPolicy policy, CypherQuery query)
        {
            // grab the endpoint in the same thread
            var txBaseEndpoint = policy.BaseEndpoint;
            var serializedQuery = policy.SerializeRequest(query);
            CustomHeaders = query.CustomHeaders;
            var task = new Task<HttpResponseMessage>(() =>
                Request.With(client.ExecutionConfiguration, query.CustomHeaders, query.MaxExecutionTime)
                    .Post(Endpoint ?? txBaseEndpoint)
                    .WithJsonContent(serializedQuery)
                    // HttpStatusCode.Created may be returned when emitting the first query on a transaction
                    .WithExpectedStatusCodes(HttpStatusCode.OK, HttpStatusCode.Created)
                    .ExecuteAsync(
                        commandDescription,
                        responseTask =>
                        {
                            // we need to check for errors returned by the transaction. The difference with a normal REST cypher
                            // query is that the errors are embedded within the result object, instead of having a 400 bad request
                            // status code.
                            var response = responseTask.Result;
                            policy.AfterExecution(TransactionHttpUtils.GetMetadataFromResponse(response), this);

                            return response;
                        })
                    .Result
            );
            _taskQueue.Add(task, _cancellationTokenSource.Token);

            if (_consumer == null)
            {
                _consumer = () =>
                {
                    while (true)
                    {
                        try
                        {
                            Task queuedTask;
                            if (!_taskQueue.TryTake(out queuedTask, 0, _cancellationTokenSource.Token))
                            {
                                // no items to consume
                                _consumer = null;
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

                _consumer.BeginInvoke(null, null);
            }

            return task;
        }

        public void Dispose()
        {
            Transaction.Dispose();
        }

        public void Commit()
        {
            _taskQueue.CompleteAdding();
            if (_taskQueue.Count > 0)
            {
                _cancellationTokenSource.Cancel();
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
            _taskQueue.CompleteAdding();
            _cancellationTokenSource.Cancel();
            Transaction.Rollback();
        }

        public void KeepAlive()
        {
            Transaction.KeepAlive();
        }

        public bool IsOpen
        {
            get { return Transaction.IsOpen; }
        }

        public Uri Endpoint
        {
            get { return Transaction.Endpoint; }
            set { Transaction.Endpoint = value; }
        }
    }
}
