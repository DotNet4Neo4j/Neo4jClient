using System;
using System.Net;
using Neo4jClient.ApiModels;
using Neo4jClient.Execution;

namespace Neo4jClient
{
    public class NeoServerConfiguration
    {
        internal RootApiResponse ApiConfig { get; private set; }

        internal Uri RootUri { get; private set; }

        internal string Username { get; private set; }
        internal string Password { get; private set; }

        private NeoServerConfiguration(RootApiResponse apiConfig)
        {
            ApiConfig = apiConfig;
        }

        public static NeoServerConfiguration GetConfiguration(Uri rootUri, string username = null, string password = null)
        {
            return GetConfiguration(rootUri, username, password, null);
        }

        internal static NeoServerConfiguration GetConfiguration(Uri rootUri, string username, string password, ExecutionConfiguration executionConfiguration)
        {
            if (executionConfiguration == null)
            {
                var httpClient = new HttpClientWrapper(username, password);

                executionConfiguration = new ExecutionConfiguration
                {
                    HttpClient = httpClient,
                    UserAgent =
                        string.Format("Neo4jClient/{0}", typeof (NeoServerConfiguration).Assembly.GetName().Version),
                    UseJsonStreaming = true,
                    JsonConverters = GraphClient.DefaultJsonConverters,
                    Username = username,
                    Password = password
                };
            }

            if (!rootUri.AbsoluteUri.EndsWith("/"))
                rootUri = new Uri(rootUri.AbsoluteUri + "/");

            rootUri = new Uri(rootUri, "");

            var result = Request.With(executionConfiguration)
                .Get(rootUri)
                .WithExpectedStatusCodes(HttpStatusCode.OK)
                .ParseAs<RootApiResponse>()
                .Execute();

            if (result == null)
            {
                throw new ApplicationException("Couldn't obtain server Root API configuration.");
            }

            var rootUriWithoutUserInfo = rootUri;
            if (!string.IsNullOrEmpty(rootUriWithoutUserInfo.UserInfo))
            {
                rootUriWithoutUserInfo = new UriBuilder(rootUri.AbsoluteUri)
                {
                    UserName = "",
                    Password = ""
                }.Uri;
            }

            var baseUriLengthToTrim = rootUriWithoutUserInfo.AbsoluteUri.Length - 1;

            result.Batch = result.Batch.Substring(baseUriLengthToTrim);
            result.Node = result.Node.Substring(baseUriLengthToTrim);
            result.NodeIndex = result.NodeIndex.Substring(baseUriLengthToTrim);
            result.Relationship = "/relationship"; //Doesn't come in on the Service Root
            result.RelationshipIndex = result.RelationshipIndex.Substring(baseUriLengthToTrim);
            result.ExtensionsInfo = result.ExtensionsInfo.Substring(baseUriLengthToTrim);

            if (!string.IsNullOrEmpty(result.Transaction))
            {
                result.Transaction = result.Transaction.Substring(baseUriLengthToTrim);
            }

            if (result.Extensions != null && result.Extensions.GremlinPlugin != null)
            {
                result.Extensions.GremlinPlugin.ExecuteScript =
                    result.Extensions.GremlinPlugin.ExecuteScript.Substring(baseUriLengthToTrim);
            }

            if (result.Cypher != null)
            {
                result.Cypher = result.Cypher.Substring(baseUriLengthToTrim);
            }

            return new NeoServerConfiguration(result)
            {
                RootUri = rootUri,
                Username = username,
                Password = password
            };
        }
    }
}
