using System;
using System.Net;
using Neo4jClient.Execution;
using NUnit.Framework;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class FactoryTests
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionIfConfigurationIsNotDefined()
        {
            new GraphClientFactory(null);
        }

        [Test]
        [ExpectedException(typeof(ApplicationException))]
        public void ShouldThrowExceptionIfRootApiIsNotDefined()
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

                NeoServerConfiguration.GetConfiguration(new Uri(testHarness.BaseUri), null, null, executeConfiguration);
            }
        }
    }
}
