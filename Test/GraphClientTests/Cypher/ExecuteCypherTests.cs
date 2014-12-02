using System.Collections.Generic;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net;
using NSubstitute;
using NUnit.Framework;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;

namespace Neo4jClient.Test.GraphClientTests.Cypher
{


    [TestFixture]
    public class ExecuteCypherTests
    {
        [Test]
        public void ShouldSendCommandAndNotCareAboutResults()
        {
            // Arrange
            const string queryText = @"START d=node({p0}), e=node({p1}) CREATE UNIQUE d-[:foo]->e";
            var parameters = new Dictionary<string, object>
            {
                {"p0", 215},
                {"p1", 219}
            };

            var cypherQuery = new CypherQuery(queryText, parameters, CypherResultMode.Set);
            var cypherApiQuery = new CypherApiQuery(cypherQuery);

            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.PostObjectAsJson("/cypher", cypherApiQuery),
                    MockResponse.Http((int)HttpStatusCode.OK)
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();

                //Act
                graphClient.ExecuteCypher(cypherQuery);
            }
        }

        [Test]
        public void SendsCommandWithCorrectTimeout()
        {
            const string queryText = "MATCH n SET n.Value = 'value'";
            const int expectedMaxExecutionTime = 100;

            var cypherQuery = new CypherQuery(queryText, new Dictionary<string, object>(), CypherResultMode.Set, maxExecutionTime: expectedMaxExecutionTime);
            var cypherApiQuery = new CypherApiQuery(cypherQuery);

            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get(""),
                    MockResponse.NeoRoot()
                },
                {
                    MockRequest.PostObjectAsJson("/cypher", cypherApiQuery),
                    MockResponse.Http((int) HttpStatusCode.OK)
                }
            })
            {
                var httpClient = testHarness.GenerateHttpClient(testHarness.BaseUri);
                var graphClient = new GraphClient(new Uri(testHarness.BaseUri), httpClient);
                graphClient.Connect();

                httpClient.ClearReceivedCalls();
                ((IRawGraphClient)graphClient).ExecuteCypher(cypherQuery);

                var call = httpClient.ReceivedCalls().Single();
                var requestMessage = (HttpRequestMessage)call.GetArguments()[0];
                var maxExecutionTimeHeader = requestMessage.Headers.Single(h => h.Key == "max-execution-time");
                Assert.AreEqual(expectedMaxExecutionTime.ToString(CultureInfo.InvariantCulture), maxExecutionTimeHeader.Value.Single());
            }
        }

        [Test]
        public void DoesntSetMaxExecutionTime_WhenNotSet()
        {
            const string queryText = "MATCH n SET n.Value = 'value'";

            var cypherQuery = new CypherQuery(queryText, new Dictionary<string, object>(), CypherResultMode.Set);
            var cypherApiQuery = new CypherApiQuery(cypherQuery);

            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get(""),
                    MockResponse.NeoRoot()
                },
                {
                    MockRequest.PostObjectAsJson("/cypher", cypherApiQuery),
                    MockResponse.Http((int) HttpStatusCode.OK)
                }
            })
            {
                var httpClient = testHarness.GenerateHttpClient(testHarness.BaseUri);
                var graphClient = new GraphClient(new Uri(testHarness.BaseUri), httpClient);
                graphClient.Connect();

                httpClient.ClearReceivedCalls();
                ((IRawGraphClient)graphClient).ExecuteCypher(cypherQuery);

                var call = httpClient.ReceivedCalls().Single();
                var requestMessage = (HttpRequestMessage)call.GetArguments()[0];
                Assert.IsFalse(requestMessage.Headers.Any(h => h.Key == "max-execution-time"));
            }
        }
    }
}
