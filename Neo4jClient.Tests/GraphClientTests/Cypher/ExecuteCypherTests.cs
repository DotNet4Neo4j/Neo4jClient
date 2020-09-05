﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.GraphClientTests.Cypher
{
    
    public class ExecuteCypherTests : IClassFixture<CultureInfoSetupFixture>
    {
        /// <summary>
        ///     When executing cypher queries when no parameters are needed, the REST interface doesn't care if we don't send parameters.
        /// </summary>
        [Fact]
        public async Task SendingNullParametersShouldNotRaiseExceptionWhenExecutingCypher()
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
                var graphClient = await testHarness.CreateAndConnectGraphClient();

                // execute cypher with "null" parameters
                await graphClient.ExecuteCypherAsync(cypherQuery);
            }
        }

        [Fact]
        public async Task ShouldSendCommandAndNotCareAboutResults()
        {
            // Arrange
            const string queryText = @"START d=node($p0), e=node($p1) CREATE UNIQUE d-[:foo]->e";
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
                var graphClient = await testHarness.CreateAndConnectGraphClient();

                //Act
                await graphClient.ExecuteCypherAsync(cypherQuery);
            }
        }

        [Fact]
        public async Task ShouldSendCommandAndNotCareAboutResultsAsync()
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
                var graphClient = await testHarness.CreateAndConnectGraphClient();

                bool raisedEvent = false;

                graphClient.OperationCompleted += (sender, e) => { raisedEvent = true; };

                //Act
                var task = graphClient.ExecuteCypherAsync(cypherQuery);
                task.Wait();

                Assert.True(raisedEvent, "Raised OperationCompleted");
            }
        }

        /// <summary>
        /// This predates #106. Given Tatham's guidance that the event should fire irrespective is this test proving correct behaviour?
        /// In any case the sync method calls async so it might be hard to avoid double firing an event.
        ///
        /// Update: At the time of removing all non-async calls I've made the async behaviour in line with the old
        /// sync behaviour (which is to fire the event with an exception). There is already a test making sure this
        /// happens elsewhere, and I thing that's probably the desired behaviour rather than this. Commented out for
        /// now pending removal. ~tobymiller1
        /// </summary>
        [Fact]
        public async Task WhenAsyncCommandFails_ShouldNotRaiseCompleted()
        {
//            // Arrange
//            const string queryText = @"return 1";
//            var parameters = new Dictionary<string, object>();
//
//            var cypherQuery = new CypherQuery(queryText, parameters, CypherResultMode.Set);
//            var cypherApiQuery = new CypherApiQuery(cypherQuery);
//
//            using (var testHarness = new RestTestHarness
//            {
//                {
//                    MockRequest.PostObjectAsJson("/cypher", cypherApiQuery),
//                    MockResponse.Throws()
//                }
//            })
//            {
//                var graphClient = await testHarness.CreateAndConnectGraphClient();
//
//                bool raisedEvent = false;
//
//                graphClient.OperationCompleted += (sender, e) => { raisedEvent = true; };
//
//                //Act
//                var task = graphClient.ExecuteCypherAsync(cypherQuery)
//                    .ContinueWith(t =>
//                    {
//                        Assert.True(t.IsFaulted);
//                        Assert.IsAssignableFrom<MockResponseThrowsException>(t.Exception.Flatten().InnerException);
//                    });
//                task.Wait();
//
//                Assert.False(raisedEvent, "Raised OperationCompleted");
//            }
        }

        /// <summary>
        /// #106
        /// </summary>
        [Fact]
        public async Task WhenExecuteGetCypherResultsFails_ShouldRaiseCompletedWithException()
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
                var graphClient = await testHarness.CreateAndConnectGraphClient();

                OperationCompletedEventArgs eventArgs = null;

                graphClient.OperationCompleted += (sender, e) => { eventArgs = e; };

                //Act
                await Assert.ThrowsAsync<MockResponseThrowsException>(async () =>
                {
                    await graphClient.ExecuteGetCypherResultsAsync<ExecuteGetCypherResultsTests.SimpleResultDto>(cypherQuery);
                });
                
                Assert.NotNull(eventArgs);
                Assert.True(eventArgs.HasException);
                Assert.Equal(typeof(MockResponseThrowsException), eventArgs.Exception.GetType());
                Assert.Equal(-1, eventArgs.ResourcesReturned);
            }
        }

        /// <summary>
        /// #106
        /// </summary>
        [Fact]
        public async Task WhenExecuteCypherFails_ShouldRaiseCompletedWithException()
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
                var graphClient = await testHarness.CreateAndConnectGraphClient();

                OperationCompletedEventArgs eventArgs = null;

                graphClient.OperationCompleted += (sender, e) => { eventArgs = e; };

                //Act
                await Assert.ThrowsAsync<MockResponseThrowsException>(async () => { await graphClient.ExecuteCypherAsync(cypherQuery); });

                Assert.NotNull(eventArgs);
                Assert.True(eventArgs.HasException);
                Assert.Equal(typeof(MockResponseThrowsException), eventArgs.Exception.GetType());
                Assert.Equal(-1, eventArgs.ResourcesReturned);
            }
        }

        /// <summary>
        /// #75
        /// </summary>
        [Fact]
        public async Task SendsCommandWithCorrectTimeout()
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
                await graphClient.ConnectAsync();

                httpClient.ClearReceivedCalls();
                await ((IRawGraphClient)graphClient).ExecuteCypherAsync(cypherQuery);

                var call = httpClient.ReceivedCalls().Single();
                var requestMessage = (HttpRequestMessage)call.GetArguments()[0];
                var maxExecutionTimeHeader = requestMessage.Headers.Single(h => h.Key == "max-execution-time");
                Assert.Equal(expectedMaxExecutionTime.ToString(CultureInfo.InvariantCulture), maxExecutionTimeHeader.Value.Single());
            }
        }

        /// <summary>
        /// #75
        /// </summary>
        [Fact]
        public async Task DoesntSetMaxExecutionTime_WhenNotSet()
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
                await graphClient.ConnectAsync();

                httpClient.ClearReceivedCalls();
                await ((IRawGraphClient)graphClient).ExecuteCypherAsync(cypherQuery);

                var call = httpClient.ReceivedCalls().Single();
                var requestMessage = (HttpRequestMessage)call.GetArguments()[0];
                Assert.False(requestMessage.Headers.Any(h => h.Key == "max-execution-time"));
            }
        }

        /// <summary>
        /// #141
        /// </summary>
        [Fact]
        public async Task SendsCommandWithCustomHeaders()
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
                await graphClient.ConnectAsync();

                httpClient.ClearReceivedCalls();
                await ((IRawGraphClient)graphClient).ExecuteCypherAsync(cypherQuery);

                var call = httpClient.ReceivedCalls().Single();
                var requestMessage = (HttpRequestMessage)call.GetArguments()[0];
                var maxExecutionTimeHeader = requestMessage.Headers.Single(h => h.Key == "max-execution-time");
                Assert.Equal(expectedMaxExecutionTime.ToString(CultureInfo.InvariantCulture), maxExecutionTimeHeader.Value.Single());
                var customHeader = requestMessage.Headers.Single(h => h.Key == headerName);
                Assert.NotNull(customHeader);
                Assert.Equal(headerValue, customHeader.Value.Single());
            }
        }


        /// <summary>
        /// #141
        /// </summary>
        [Fact]
        public async Task DoesntSetHeaders_WhenNotSet()
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
                await graphClient.ConnectAsync();

                httpClient.ClearReceivedCalls();
                await ((IRawGraphClient)graphClient).ExecuteCypherAsync(cypherQuery);

                var call = httpClient.ReceivedCalls().Single();
                var requestMessage = (HttpRequestMessage)call.GetArguments()[0];
                Assert.False(requestMessage.Headers.Any(h => h.Key == "max-execution-time"));
            }
        }
    }
}
