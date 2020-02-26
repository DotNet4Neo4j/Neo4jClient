using System;
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
                    UserAgent =
                        string.Format("Neo4jClient/{0}", typeof(NeoServerConfiguration).Assembly.GetName().Version),
                    UseJsonStreaming = true,
                    JsonConverters = GraphClient.DefaultJsonConverters
                };

                await Assert.ThrowsAsync<InvalidOperationException>(async () => await NeoServerConfiguration.GetConfigurationAsync(new Uri(testHarness.BaseUri), null, null, null, executeConfiguration));
            }
        }

        [Fact]
        public async Task GraphClientFactoryUseCase()
        {
            const string queryText = @"MATCH (d) RETURN d";

            var cypherQuery = new CypherQuery(queryText, null, CypherResultMode.Set, CypherResultFormat.Rest, "neo4j");
            var cypherApiQuery = new CypherApiQuery(cypherQuery);

            using (var testHarness = new RestTestHarness
            {
                { MockRequest.Get("/"), MockResponse.NeoRoot() },
                { MockRequest.PostObjectAsJson("/cypher", cypherApiQuery), new MockResponse { StatusCode = HttpStatusCode.OK } }
            })
            {
                var httpClient = testHarness.GenerateHttpClient(testHarness.BaseUri);

                var executeConfiguration = new ExecutionConfiguration
                {
                    HttpClient = httpClient,
                    UserAgent =
                        string.Format("Neo4jClient/{0}", typeof(NeoServerConfiguration).Assembly.GetName().Version),
                    UseJsonStreaming = true,
                    JsonConverters = GraphClient.DefaultJsonConverters
                };

                var configuration = await NeoServerConfiguration.GetConfigurationAsync(new Uri(testHarness.BaseUri), null, null,null, executeConfiguration);

                var factory = new GraphClientFactory(configuration);

                using (var client = await factory.CreateAsync(httpClient))
                {
                    await client.Cypher.Match("(d)").Return<object>("d").ExecuteWithoutResultsAsync();
                }
            }
        }
    }
}
