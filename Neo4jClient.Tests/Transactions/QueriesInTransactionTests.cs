using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;
using Neo4jClient.Transactions;
using Xunit;

namespace Neo4jClient.Tests.Transactions
{

    public partial class QueriesInTransactionTests : IClassFixture<CultureInfoSetupFixture>
    {

        [Fact]
        public async Task CommitWithoutRequestsShouldNotGenerateMessage()
        {
            var initTransactionRequest = MockRequest.PostJson("/transaction", @"{'statements': []}");
            var commitTransactionRequest = MockRequest.PostJson("/transaction/1/commit", @"{'statements': []}");
            using (var testHarness = new RestTestHarness
            {
                {
                    initTransactionRequest, MockResponse.Json(201, TransactionRestResponseHelper.GenerateInitTransactionResponse(1), "http://foo/db/data/transaction/1")
                },
                {
                    commitTransactionRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            }.ShouldNotBeCalled(initTransactionRequest, commitTransactionRequest))
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {
                    // no requests
                    await transaction.CommitAsync();
                }
            }
        }

        [Fact]
        public async Task KeepAliveWithoutRequestsShouldNotGenerateMessage()
        {
            var initTransactionRequest = MockRequest.PostJson("/transaction", @"{'statements': []}");
            var keepAliveRequest = MockRequest.PostJson("/transaction/1", @"{'statements': []}");
            using (var testHarness = new RestTestHarness
            {
                {
                    initTransactionRequest,
                    MockResponse.Json(201, TransactionRestResponseHelper.GenerateInitTransactionResponse(1), "http://foo/db/data/transaction/1")
                },
                {
                    keepAliveRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            }.ShouldNotBeCalled(initTransactionRequest, keepAliveRequest))
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {
                    // no requests
                    await transaction.KeepAliveAsync();
                }
            }
        }

        [Fact]
        public async Task RollbackWithoutRequestsShouldNotGenerateMessage()
        {
            var initTransactionRequest = MockRequest.PostJson("/transaction", @"{'statements': []}");
            var rollbackTransactionRequest = MockRequest.Delete("/transaction/1");
            using (var testHarness = new RestTestHarness
            {
                {
                    initTransactionRequest,
                    MockResponse.Json(201, TransactionRestResponseHelper.GenerateInitTransactionResponse(1), "http://foo/db/data/transaction/1")
                },
                {
                    rollbackTransactionRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            }.ShouldNotBeCalled(initTransactionRequest, rollbackTransactionRequest))
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {
                    // no requests
                    await transaction.RollbackAsync();
                }
            }
        }

        [Fact]
        public async Task UpdateTransactionEndpointAfterFirstRequest()
        {
            var initTransactionRequest = MockRequest.PostJson("/transaction", @"{
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
            var rollbackTransactionRequest = MockRequest.Delete("/transaction/1");
            using (var testHarness = new RestTestHarness
            {
                {
                    initTransactionRequest,
                    MockResponse.Json(201, TransactionRestResponseHelper.GenerateInitTransactionResponse(1), "http://foo/db/data/transaction/1")
                },
                {
                    rollbackTransactionRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            })
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {
                    // dummy query to generate request
                    await client.Cypher
                        .Match("n")
                        .Return(n => n.Count())
                        .ExecuteWithoutResultsAsync();

                    Assert.Equal(
                        new Uri("http://foo/db/data/transaction/1"),
                        ((INeo4jTransaction)((TransactionScopeProxy) transaction).TransactionContext).Endpoint);
                }
            }
        }

        [Fact]
        public async Task TransactionCommit()
        {
            var initTransactionRequest = MockRequest.PostJson("/transaction", @"{
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
            var commitRequest = MockRequest.PostJson("/transaction/1/commit", @"{'statements': []}");
            using (var testHarness = new RestTestHarness
            {
                {
                    initTransactionRequest,
                    MockResponse.Json(201, TransactionRestResponseHelper.GenerateInitTransactionResponse(1), "http://foo/db/data/transaction/1")
                },
                {
                    commitRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            })
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {
                    // dummy query to generate request
                    await client.Cypher
                        .Match("n")
                        .Return(n => n.Count())
                        .ExecuteWithoutResultsAsync();

                    await transaction.CommitAsync();
                }
            }
        }

        private class DateHolder {  public DateTime Date { get; set; } }


        [Fact]
        public async Task SecondRequestDoesntReturnCreateHttpStatus()
        {
            var initTransactionRequest = MockRequest.PostJson("/transaction", @"{
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
            var secondRequest = MockRequest.PostJson("/transaction/1", @"{
                'statements': [{'statement': 'MATCH t\r\nRETURN count(t)', 'resultDataContents':[], 'parameters': {}}]}");
            var commitRequest = MockRequest.PostJson("/transaction/1/commit", @"{'statements': []}");
            using (var testHarness = new RestTestHarness
            {
                {
                    initTransactionRequest,
                    MockResponse.Json(201, TransactionRestResponseHelper.GenerateInitTransactionResponse(1), "http://foo/db/data/transaction/1")
                },
                {
                    commitRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                },
                {
                    secondRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            })
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {
                    // dummy queries to generate request
                    await client.Cypher
                        .Match("n")
                        .Return(n => n.Count())
                        .ExecuteWithoutResultsAsync();

                    await client.Cypher
                        .Match("t")
                        .Return(t => t.Count())
                        .ExecuteWithoutResultsAsync();

                    await transaction.CommitAsync();
                }
            }
        }

        [Fact]
        public async Task KeepAliveAfterFirstRequest()
        {
            var initTransactionRequest = MockRequest.PostJson("/transaction", @"{
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
            var rollbackTransactionRequest = MockRequest.Delete("/transaction/1");
            var keepAliveRequest = MockRequest.PostJson("/transaction/1", @"{'statements': []}");
            using (var testHarness = new RestTestHarness
            {
                {
                    initTransactionRequest, MockResponse.Json(201, TransactionRestResponseHelper.GenerateInitTransactionResponse(1), "http://foo/db/data/transaction/1")
                },
                {
                    keepAliveRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                },
                {
                    rollbackTransactionRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            })
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {
                    // dummy query to generate request
                    await client.Cypher
                        .Match("n")
                        .Return(n => n.Count())
                        .ExecuteWithoutResultsAsync();

                    await transaction.KeepAliveAsync();
                }
            }
        }

        [Fact]
        public async Task DeserializeResultsFromTransaction()
        {
            var initTransactionRequest = MockRequest.PostJson("/transaction", @"{
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
            var deserializationRequest = MockRequest.PostJson("/transaction/1", @"{
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
            var rollbackTransactionRequest = MockRequest.Delete("/transaction/1");
            using (var testHarness = new RestTestHarness
            {
                {
                    initTransactionRequest,
                    MockResponse.Json(201, TransactionRestResponseHelper.GenerateInitTransactionResponse(1), "http://foo/db/data/transaction/1")
                },
                {
                    deserializationRequest,
                    MockResponse.Json(200, @"{'results':[{'columns': ['count(n)'], 'data': [{'row': [0]}]}]}")
                },
                {
                    rollbackTransactionRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            })
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {
                    // dummy query to generate request
                    await client.Cypher
                        .Match("n")
                        .Return(n => n.Count())
                        .ExecuteWithoutResultsAsync();

                    // this query will hit the deserializer
                    var count = await client.Cypher
                        .Match("n")
                        .Return(n => n.Count())
                        .ResultsAsync;

                    Assert.Equal(count.First(), 0);
                }
            }
        }

        [Fact]
        public async Task OnTransactionDisposeCallRollback()
        {
            var initTransactionRequest = MockRequest.PostJson("/transaction", @"{
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
            var rollbackTransactionRequest = MockRequest.Delete("/transaction/1");
            using (var testHarness = new RestTestHarness
            {
                {
                    initTransactionRequest,
                    MockResponse.Json(201, TransactionRestResponseHelper.GenerateInitTransactionResponse(1), "http://foo/db/data/transaction/1")
                },
                {
                    rollbackTransactionRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            })
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {
                    // dummy query to generate request
                    await client.Cypher
                        .Match("n")
                        .Return(n => n.Count())
                        .ExecuteWithoutResultsAsync();
                }
            }
        }

        public class DummyTotal
        {
            public int Total { get; set; }
        }

        [Fact]
        public async Task ExecuteAsyncRequestInTransaction()
        {
            const string queryText = @"MATCH (n) RETURN count(n) as Total";
            const string resultColumn = @"{'columns':['Total'], 'data':[{'row':[1]}]}";

            var cypherQuery = new CypherQuery(queryText, new Dictionary<string, object>(), CypherResultMode.Projection, "neo4j");
            var cypherApiQuery = new CypherStatementList { new CypherTransactionStatement(cypherQuery) };
            var commitRequest = MockRequest.PostJson("/transaction/1/commit", @"{'statements': []}");
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.PostObjectAsJson("/transaction", cypherApiQuery),
                    MockResponse.Json(201, TransactionRestResponseHelper.GenerateInitTransactionResponse(1, resultColumn), "http://foo/db/data/transaction/1")
                },
                {
                    commitRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            })
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
                var rawClient = (IRawGraphClient) client;
                using (var tran = client.BeginTransaction())
                {
                    var totalObj = rawClient.ExecuteGetCypherResultsAsync<DummyTotal>(cypherQuery).Result.Single();
                    Assert.Equal(1, totalObj.Total);
                    await tran.CommitAsync();
                }

            }
        }

        private class RestHarnessWithCounter : RestTestHarness
        {
            public ConcurrentQueue<int> Queue { get; set; }

            public RestHarnessWithCounter()
            {
                Queue = new ConcurrentQueue<int>();
            }

            protected override async Task<HttpResponseMessage> HandleRequest(HttpRequestMessage request, string baseUri)
            {
                if (request.Method == HttpMethod.Post)
                {
                    var content = await request.Content.ReadAsStringAsync();
                    int totalIndex = content.IndexOf("RETURN ", StringComparison.InvariantCultureIgnoreCase);
                    if (totalIndex > 0)
                    {
                        totalIndex += "RETURN ".Length;
                        int spaceIndex = content.IndexOf(" ", totalIndex, StringComparison.InvariantCultureIgnoreCase);
                        spaceIndex.Should().BeGreaterThan(totalIndex);
                        Queue.Enqueue(int.Parse(content.Substring(totalIndex, spaceIndex - totalIndex)));
                    }
                }

                return await base.HandleRequest(request, baseUri);
            }
        }

        [Fact]
        public async Task AsyncRequestsInTransactionShouldBeExecutedInOrder()
        {
            const string queryTextBase = @"MATCH (n) RETURN {0} as Total";
            const string resultColumnBase = @"{{'columns':['Total'], 'data':[{{'row':[{0}]}}]}}";
            const int asyncRequests = 15;

            var queries = new CypherQuery[asyncRequests];
            var apiQueries = new CypherStatementList[asyncRequests];
            var responses = new MockResponse[asyncRequests];
            var testHarness = new RestHarnessWithCounter();

            for (int i = 0; i < asyncRequests; i++)
            {
                queries[i] = new CypherQuery(string.Format(queryTextBase, i), new Dictionary<string, object>(),
                    CypherResultMode.Projection, "neo4j");
                apiQueries[i] = new CypherStatementList {new CypherTransactionStatement(queries[i])};
                responses[i] = MockResponse.Json(200,
                    @"{'results':[" + string.Format(resultColumnBase, i) + @"], 'errors':[] }");
                if (i > 0)
                {
                    testHarness.Add(MockRequest.PostObjectAsJson("/transaction/1", apiQueries[i]), responses[i]);
                }
            }

            testHarness.Add(
                MockRequest.PostObjectAsJson("/transaction", apiQueries[0]),
                MockResponse.Json(201, TransactionRestResponseHelper.GenerateInitTransactionResponse(1, string.Format(resultColumnBase, 0)),
                    "http://foo/db/data/transaction/1")
                );
            testHarness.Add(
                MockRequest.PostJson("/transaction/1/commit", @"{'statements': []}"),
                MockResponse.Json(200, @"{'results':[], 'errors':[] }")
            );
            try
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
                var rawClient = (IRawGraphClient)client;
                var tasks = new Task[asyncRequests];
                using (var tran = client.BeginTransaction())
                {
                    for (int i = 0; i < asyncRequests; i++)
                    {
                        int tmpResult = i;
                        tasks[i] = rawClient.ExecuteGetCypherResultsAsync<DummyTotal>(queries[i]).ContinueWith(task =>
                        {
                            Assert.Equal(tmpResult, task.Result.Single().Total);
                        });
                    }

                    await Task.WhenAll(tasks);
                    await tran.CommitAsync();
                }
            }
            finally
            {
                testHarness.Dispose();
            }

            // check that we have a total order
            Assert.Equal(asyncRequests, testHarness.Queue.Count);
            int lastElement = -1;
            for (int i = 0; i < asyncRequests; i++)
            {
                int headItem;
                Assert.True(testHarness.Queue.TryDequeue(out headItem));
                headItem.Should().BeGreaterThan(lastElement);
                lastElement = headItem;
            }
        }

        /// <summary>
        /// This test is flakey. If run in Resharper as a group, it fails. If run by itself it passes.
        /// Appears to be a race condition where for the test to pass the ExecuteGetCypherResultsAsync() call needs to still be in progress before the Commit() is called.
        /// If stepped through, the test will fail since the call easily finishes. Flakeyness observable as early as [bdc1c45]
        /// Perhaps need to insert simulated delay into MockResponse?
        /// </summary>
        [Fact(Skip="Flakey")]
        public async Task CommitFailsOnPendingAsyncRequests()
        {
            const string queryText = @"MATCH (n) RETURN count(n) as Total";
            const string resultColumn = @"{'columns':['Total'], 'data':[{'row':[1]}]}";

            var cypherQuery = new CypherQuery(queryText, new Dictionary<string, object>(), CypherResultMode.Projection, "neo4j");
            var cypherApiQuery = new CypherStatementList { new CypherTransactionStatement(cypherQuery) };

            using (var testHarness = new RestTestHarness(false)
            {
                {
                    MockRequest.PostObjectAsJson("/transaction", cypherApiQuery),
                    MockResponse.Json(201, TransactionRestResponseHelper.GenerateInitTransactionResponse(1, resultColumn), "http://foo/db/data/transaction/1")
                }
            })
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
                var rawClient = (IRawGraphClient) client;
                using (var tran = client.BeginTransaction())
                {
                    await rawClient.ExecuteGetCypherResultsAsync<DummyTotal>(cypherQuery);
                    var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () => await tran.CommitAsync());
                    Assert.Equal("Cannot commit unless all tasks have been completed", ex.Message);
                }

            }
        }
    }
}
