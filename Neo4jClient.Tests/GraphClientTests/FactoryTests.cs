using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;
using Neo4jClient.Execution;
using Xunit;

namespace Neo4jClient.Tests.GraphClientTests
{
    
    public class FactoryTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void ShouldThrowExceptionIfConfigurationIsNotDefined()
        {
            Assert.Throws<ArgumentNullException>(() => new GraphClientFactory(null));
        }

        [Fact]
        public async Task ShouldThrowExceptionIfRootApiIsNotDefined()
        {
            using (var testHarness = new RestTestHarness
            {
                { MockRequest.Get("/"), new MockResponse { StatusCode = HttpStatusCode.OK } }
            })
            {
                var httpClient = testHarness.GenerateHttpClient(testHarness.BaseUri);

                var executeConfiguration = new ExecutionConfiguration
                {
                    HttpClient = httpClient,
                    UserAgent = $"Neo4jClient/{typeof(NeoServerConfiguration).Assembly.GetName().Version}",
                    UseJsonStreaming = true,
                    JsonConverters = GraphClient.DefaultJsonConverters
                };

                await Assert.ThrowsAsync<InvalidOperationException>(async () => await NeoServerConfiguration.GetConfigurationAsync(new Uri(testHarness.BaseUri), null, null, null, null, executeConfiguration));
            }
        }

        [Fact]
        public async Task GraphClientFactoryUseCase()
        {
            const string queryText = @"RETURN d";

            var cypherQuery = new CypherQuery(queryText, new Dictionary<string, object>(), CypherResultMode.Set, CypherResultFormat.Rest, "neo4j");
            var cypherApiQuery = new CypherStatementList { new CypherTransactionStatement(cypherQuery) };

            using (var testHarness = new RestTestHarness
            {
                { MockRequest.Get("/"), MockResponse.NeoRoot20() },
                { MockRequest.PostObjectAsJson("/transaction/commit", cypherApiQuery), new MockResponse { StatusCode = HttpStatusCode.OK } }
            })
            {
                var httpClient = testHarness.GenerateHttpClient(testHarness.BaseUri);

                var executeConfiguration = new ExecutionConfiguration
                {
                    HttpClient = httpClient,
                    UserAgent = $"Neo4jClient/{typeof(NeoServerConfiguration).Assembly.GetName().Version}",
                    UseJsonStreaming = true,
                    JsonConverters = GraphClient.DefaultJsonConverters
                };

                var configuration = await NeoServerConfiguration.GetConfigurationAsync(new Uri(testHarness.BaseUri), null, null,null,null, executeConfiguration);

                var factory = new GraphClientFactory(configuration);

                using (var client = await factory.CreateAsync(httpClient))
                {
                    await client.Cypher.Return<object>("d").ExecuteWithoutResultsAsync();
                }
            }
        }
    }
}
