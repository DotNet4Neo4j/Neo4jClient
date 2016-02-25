using System;
using System.Diagnostics;
using System.Net;
using Neo4jClient.ApiModels;
using Neo4jClient.Cypher;
using Neo4jClient.Execution;
using Neo4jClient.Transactions;

namespace Neo4jClient
{
    public partial class GraphClient : IRawGraphClient, IInternalTransactionalGraphClient, IDisposable
    {
        public GraphClient(Uri rootUri, string username = null, string password = null)
            : this(rootUri, new HttpClientWrapper(username, password))
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.UseNagleAlgorithm = false;
        }

        public GraphClient(Uri rootUri, bool expect100Continue, bool useNagleAlgorithm, string username = null, string password = null)
            : this(rootUri, new HttpClientWrapper(username, password))
        {
            ServicePointManager.Expect100Continue = expect100Continue;
            ServicePointManager.UseNagleAlgorithm = useNagleAlgorithm;
        }


        public virtual void Connect()
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
                var result = Request.With(ExecutionConfiguration)
                    .Get(BuildUri(""))
                    .WithExpectedStatusCodes(HttpStatusCode.OK)
                    .ParseAs<RootApiResponse>()
                    .Execute();

                var rootUriWithoutUserInfo = RootUri;
                if (!string.IsNullOrEmpty(rootUriWithoutUserInfo.UserInfo))
                    rootUriWithoutUserInfo = new UriBuilder(RootUri.AbsoluteUri) {UserName = "", Password = ""}.Uri;
                var baseUriLengthToTrim = rootUriWithoutUserInfo.AbsoluteUri.Length;

                RootApiResponse = result;
                RootApiResponse.Batch = RootApiResponse.Batch.Substring(baseUriLengthToTrim);
                RootApiResponse.Node = RootApiResponse.Node.Substring(baseUriLengthToTrim);
                RootApiResponse.NodeIndex = RootApiResponse.NodeIndex.Substring(baseUriLengthToTrim);
                RootApiResponse.Relationship = "/relationship"; //Doesn't come in on the Service Root
                RootApiResponse.RelationshipIndex = RootApiResponse.RelationshipIndex.Substring(baseUriLengthToTrim);
                RootApiResponse.ExtensionsInfo = RootApiResponse.ExtensionsInfo.Substring(baseUriLengthToTrim);

                if (!string.IsNullOrEmpty(RootApiResponse.Transaction))
                {
                    RootApiResponse.Transaction = RootApiResponse.Transaction.Substring(baseUriLengthToTrim);
                    transactionManager = new TransactionManager(this);
                }

                if (RootApiResponse.Extensions != null && RootApiResponse.Extensions.GremlinPlugin != null)
                {
                    RootApiResponse.Extensions.GremlinPlugin.ExecuteScript =
                        RootApiResponse.Extensions.GremlinPlugin.ExecuteScript.Substring(baseUriLengthToTrim);
                }

                if (RootApiResponse.Cypher != null)
                {
                    RootApiResponse.Cypher =
                        RootApiResponse.Cypher.Substring(baseUriLengthToTrim);
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