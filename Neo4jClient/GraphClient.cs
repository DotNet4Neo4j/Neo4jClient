using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Neo4jClient.Cypher;
using Neo4jClient.Transactions;

namespace Neo4jClient
{
    public partial class GraphClient : IRawGraphClient, IInternalTransactionalGraphClient<HttpResponseMessage>, IDisposable
    {
        public GraphClient(Uri rootUri, string username = null, string password = null)
            : this(rootUri, new HttpClientWrapper(username, password))
        {
//            ServicePointManager.Expect100Continue = true;
//            ServicePointManager.UseNagleAlgorithm = false;
        }

        public GraphClient(Uri rootUri, bool expect100Continue, bool useNagleAlgorithm, string username = null, string password = null)
            : this(rootUri, new HttpClientWrapper(username, password))
        {
//            ServicePointManager.Expect100Continue = expect100Continue;
//            ServicePointManager.UseNagleAlgorithm = useNagleAlgorithm;
        }

        public virtual async Task ConnectAsync(NeoServerConfiguration configuration = null)
        {
            if (IsConnected)
            {
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var operationCompletedArgs = new OperationCompletedEventArgs
            {
                QueryText = "Connect",
                ResourcesReturned = 0
            };

            Action stopTimerAndNotifyCompleted = () =>
            {
                stopwatch.Stop();
                operationCompletedArgs.TimeTaken = stopwatch.Elapsed;
                OnOperationCompleted(operationCompletedArgs);
            };

            try
            {
                configuration = configuration ?? await NeoServerConfiguration.GetConfigurationAsync(
                    RootUri,
                    ExecutionConfiguration.Username,
                    ExecutionConfiguration.Password,
                    ExecutionConfiguration.Realm,
                    ExecutionConfiguration).ConfigureAwait(false);

                RootApiResponse = configuration.ApiConfig;

                if (!string.IsNullOrWhiteSpace(RootApiResponse.Transaction))
                {
                    //  transactionManager = new TransactionManager(this);
                }

                rootNode = string.IsNullOrEmpty(RootApiResponse.ReferenceNode)
                    ? null
                    : new RootNode(long.Parse(GetLastPathSegment(RootApiResponse.ReferenceNode)), this);

                // http://blog.neo4j.org/2012/04/streaming-rest-api-interview-with.html
                ExecutionConfiguration.UseJsonStreaming = ExecutionConfiguration.UseJsonStreaming &&
                                                          RootApiResponse.Version >= new Version(1, 8);

                if (RootApiResponse.Version < new Version(2, 0))
                    cypherCapabilities = CypherCapabilities.Cypher19;

                if (RootApiResponse.Version >= new Version(2, 2))
                    cypherCapabilities = CypherCapabilities.Cypher22;

                if (RootApiResponse.Version >= new Version(2, 2, 6))
                    cypherCapabilities = CypherCapabilities.Cypher226;

                if (RootApiResponse.Version >= new Version(2, 3))
                    cypherCapabilities = CypherCapabilities.Cypher23;

                if (RootApiResponse.Version >= new Version(3, 0))
                    cypherCapabilities = CypherCapabilities.Cypher30;
            }
            catch (AggregateException ex)
            {
                Exception unwrappedException;
                var wasUnwrapped = ex.TryUnwrap(out unwrappedException);
                operationCompletedArgs.Exception = wasUnwrapped ? unwrappedException : ex;

                stopTimerAndNotifyCompleted();

                if (wasUnwrapped)
                    throw unwrappedException;

                throw;
            }
            catch (Exception e)
            {
                operationCompletedArgs.Exception = e;
                stopTimerAndNotifyCompleted();
                throw;
            }

            stopTimerAndNotifyCompleted();
        }
    }
}