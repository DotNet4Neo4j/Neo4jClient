using System;
using System.Collections.Generic;
using System.Net;
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
        public void ShouldSendCommandAndNotCareAboutResultsAsync()
        {
            // Arrange
            const string queryText = @"return 1";
            var parameters = new Dictionary<string, object>();

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

                bool raisedEvent = false;

                graphClient.OperationCompleted += (sender, e) => { raisedEvent = true; };

                //Act
                var task = graphClient.ExecuteCypherAsync(cypherQuery);
                task.Wait();

                Assert.IsTrue(raisedEvent, "Raised OperationCompleted");
            }
        }

        [Test]
        public void WhenAsyncCommandFails_ShouldNotRaiseCompleted()
        {
            // Arrange
            const string queryText = @"return 1";
            var parameters = new Dictionary<string, object>();

            var cypherQuery = new CypherQuery(queryText, parameters, CypherResultMode.Set);
            var cypherApiQuery = new CypherApiQuery(cypherQuery);

            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.PostObjectAsJson("/cypher", cypherApiQuery),
                    MockResponse.Throws()
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();

                bool raisedEvent = false;

                graphClient.OperationCompleted += (sender, e) => { raisedEvent = true; };

                //Act
                var task = graphClient.ExecuteCypherAsync(cypherQuery)
                    .ContinueWith(t =>
                    {
                        Assert.IsTrue(t.IsFaulted);
                        Assert.IsInstanceOf<MockResponseThrowsException>(t.Exception.Flatten().InnerException);
                    });
                task.Wait();

                Assert.IsFalse(raisedEvent, "Raised OperationCompleted");
            }
        }
    }
}
