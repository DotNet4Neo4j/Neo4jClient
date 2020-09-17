using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Neo4j.Driver;
using Neo4jClient.ApiModels;
using Neo4jClient.Execution;

namespace Neo4jClient
{
    public class NeoServerConfiguration
    {
        internal RootApiResponse ApiConfig { get; private set; }

        internal Uri RootUri { get; private set; }

        internal EncryptionLevel? EncryptionLevel { get; }
        internal string Username { get; private set; }
        internal string Password { get; private set; }
        internal string Realm { get; private set; }

        private NeoServerConfiguration(RootApiResponse apiConfig)
        {
            ApiConfig = apiConfig;
        }

        public static async Task<NeoServerConfiguration> GetConfigurationAsync(Uri rootUri, string username = null, string password = null, string realm = null, EncryptionLevel? encryptionLevel = null)
        {
            return await GetConfigurationAsync(rootUri, username, password, realm, null, null).ConfigureAwait(false);
        }

        internal static async Task<NeoServerConfiguration> GetConfigurationAsync(Uri rootUri, string username, string password, string realm, EncryptionLevel? encryptionLevel, ExecutionConfiguration executionConfiguration)
        {
            if (executionConfiguration == null)
            {
                var httpClient = new HttpClientWrapper(username, password);

                executionConfiguration = new ExecutionConfiguration
                {
                    HttpClient = httpClient,
                    UserAgent = $"Neo4jClient/{typeof(NeoServerConfiguration).GetTypeInfo().Assembly.GetName().Version}",
                    UseJsonStreaming = true,
                    JsonConverters = GraphClient.DefaultJsonConverters,
                    Username = username,
                    Password = password,
                    Realm = realm,
                    EncryptionLevel = encryptionLevel
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

            result.TrimUriFromProperties(rootUriWithoutUserInfo.AbsoluteUri);
            
            return new NeoServerConfiguration(result)
            {
                RootUri = rootUri,
                Username = username,
                Password = password,
                Realm = realm
            };
        }
    }
}
