using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
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
        internal string Realm { get; private set; }

        private NeoServerConfiguration(RootApiResponse apiConfig)
        {
            ApiConfig = apiConfig;
        }

        public static async Task<NeoServerConfiguration> GetConfigurationAsync(Uri rootUri, string username = null, string password = null, string realm = null)
        {
            return await GetConfigurationAsync(rootUri, username, password, realm, null).ConfigureAwait(false);
        }

        internal static async Task<NeoServerConfiguration> GetConfigurationAsync(Uri rootUri, string username, string password, string realm, ExecutionConfiguration executionConfiguration)
        {
            if (executionConfiguration == null)
            {
                var httpClient = new HttpClientWrapper(username, password);

                executionConfiguration = new ExecutionConfiguration
                {
                    HttpClient = httpClient,
                    UserAgent =
                        string.Format("Neo4jClient/{0}", typeof(NeoServerConfiguration).GetTypeInfo().Assembly.GetName().Version),
                    UseJsonStreaming = true,
                    JsonConverters = GraphClient.DefaultJsonConverters,
                    Username = username,
                    Password = password,
                    Realm = realm
                };
            }

            if (!rootUri.AbsoluteUri.EndsWith("/"))
                rootUri = new Uri(rootUri.AbsoluteUri + "/");

            rootUri = new Uri(rootUri, "");

            var result = await Request.With(executionConfiguration)
                .Get(rootUri)
                .WithExpectedStatusCodes(HttpStatusCode.OK)
                .ParseAs<RootApiResponse>()
                .ExecuteAsync().ConfigureAwait(false);

            if (result == null)
            {
                throw new InvalidOperationException("Couldn't obtain server Root API configuration.");
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
                Password = password,
                Realm = realm
            };
        }

        public static NeoServerConfiguration GetConfiguration(Uri rootUri, string username = null, string password = null, string realm = null)
        {
            return GetConfiguration(rootUri, username, password, realm, null);
        }

        internal static NeoServerConfiguration GetConfiguration(Uri rootUri, string username, string password, string realm, ExecutionConfiguration executionConfiguration)
        {
            if (executionConfiguration == null)
            {
                var httpClient = new HttpClientWrapper(username, password);

                executionConfiguration = new ExecutionConfiguration
                {
                    HttpClient = httpClient,
                    UserAgent =
                        string.Format("Neo4jClient/{0}", typeof (NeoServerConfiguration).GetTypeInfo().Assembly.GetName().Version),
                    UseJsonStreaming = true,
                    JsonConverters = GraphClient.DefaultJsonConverters,
                    Username = username,
                    Password = password,
                    Realm = realm
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
                throw new InvalidOperationException("Couldn't obtain server Root API configuration.");
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
