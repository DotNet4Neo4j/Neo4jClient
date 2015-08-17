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

            var cypherQuery = new CypherQuery(queryText, parameters, CypherResultMode.Set, CypherResultFormat.Rest);
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

        /// <summary>
        /// This predates #106. Given Tatham's guidance that the event should fire irrespective is this test proving correct behaviour?
        /// In any case the sync method calls async so it might be hard to avoid double firing an event.
        /// </summary>
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

        /// <summary>
        /// #106
        /// </summary>
        [Test]
        public void WhenExecuteGetCypherResultsFails_ShouldRaiseCompletedWithException()
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

                OperationCompletedEventArgs eventArgs = null;

                graphClient.OperationCompleted += (sender, e) => { eventArgs = e; };

                //Act
                Assert.Throws<MockResponseThrowsException>(() =>
                {
                    graphClient.ExecuteGetCypherResults<ExecuteGetCypherResultsTests.SimpleResultDto>(cypherQuery);
                }, "We should expect an exception");
                
                Assert.IsNotNull(eventArgs, "but we should also have received the completion event");
                Assert.IsTrue(eventArgs.HasException);
                Assert.AreEqual(typeof(MockResponseThrowsException), eventArgs.Exception.GetType());
                Assert.AreEqual(-1, eventArgs.ResourcesReturned);
            }
        }

        /// <summary>
        /// #106
        /// </summary>
        [Test]
        public void WhenExecuteCypherFails_ShouldRaiseCompletedWithException()
        {
            // Arrange
            const string queryText = @"bad cypher";
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

                OperationCompletedEventArgs eventArgs = null;

                graphClient.OperationCompleted += (sender, e) => { eventArgs = e; };

                //Act
                Assert.Throws<MockResponseThrowsException>(() =>
                {
                    graphClient.ExecuteCypher(cypherQuery);
                }, "We should expect an exception");

                Assert.IsNotNull(eventArgs, "but we should also have received the completion event");
                Assert.IsTrue(eventArgs.HasException);
                Assert.AreEqual(typeof(MockResponseThrowsException), eventArgs.Exception.GetType());
                Assert.AreEqual(-1, eventArgs.ResourcesReturned);
            }
        }
    }
}
