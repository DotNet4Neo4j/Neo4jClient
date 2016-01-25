using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using NUnit.Framework;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;
using NSubstitute;

namespace Neo4jClient.Test.GraphClientTests.Cypher
{
    [TestFixture]
    public class ExecuteCypherTests
    {
        /// <summary>
        ///     When executing cypher queries when no parameters are needed, the REST interface doesn't care if we don't send parameters.
        /// </summary>
        [Test]
        public void SendingNullParametersShouldNotRaiseExceptionWhenExecutingCypher()
        {
            const string queryText = @"MATCH (d) RETURN d";
            
            var cypherQuery = new CypherQuery(queryText, null, CypherResultMode.Set, CypherResultFormat.Rest);
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

                // execute cypher with "null" parameters
                graphClient.ExecuteCypher(cypherQuery);
            }
        }

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

        /// <summary>
        /// #75
        /// </summary>
        [Test]
        public void SendsCommandWithCorrectTimeout()
        {
            const string queryText = "MATCH n SET n.Value = 'value'";
            const int expectedMaxExecutionTime = 100;

            var cypherQuery = new CypherQuery(queryText, new Dictionary<string, object>(), CypherResultMode.Set,CypherResultFormat.DependsOnEnvironment , maxExecutionTime: expectedMaxExecutionTime);
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

        /// <summary>
        /// #75
        /// </summary>
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

        /// <summary>
        /// #141
        /// </summary>
        [Test]
        public void SendsCommandWithCustomHeaders()
        {
            const string queryText = "MATCH n SET n.Value = 'value'";
            const int expectedMaxExecutionTime = 100;
            const string headerName = "MyTestHeader";
            const string headerValue = "myTestHeaderValue";
            var customHeaders = new NameValueCollection();
            customHeaders.Add(headerName, headerValue);

            var cypherQuery = new CypherQuery(queryText, new Dictionary<string, object>(), CypherResultMode.Set, CypherResultFormat.DependsOnEnvironment, maxExecutionTime: expectedMaxExecutionTime, customHeaders: customHeaders);
            
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
                var customHeader = requestMessage.Headers.Single(h => h.Key == headerName);
                Assert.IsNotNull(customHeader);
                Assert.AreEqual(headerValue, customHeader.Value.Single());
            }
        }


        /// <summary>
        /// #141
        /// </summary>
        [Test]
        public void DoesntSetHeaders_WhenNotSet()
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
