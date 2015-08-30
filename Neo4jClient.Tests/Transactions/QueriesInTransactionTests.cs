using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;
using Neo4jClient.Transactions;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using TransactionScopeOption = System.Transactions.TransactionScopeOption;

namespace Neo4jClient.Test.Transactions
{
    [TestFixture]
    public class QueriesInTransactionTests
    {
        private static string ResetTransactionTimer()
        {
            return new DateTime().AddSeconds(60).ToString("ddd, dd, MMM yyyy HH:mm:ss +0000");
        }

        private string GenerateInitTransactionResponse(int id, string results)
        {
            return string.Format(
                @"{{'commit': 'http://foo/db/data/transaction/{0}/commit', 'results': [{1}], 'errors': [], 'transaction': {{ 'expires': '{2}' }} }}",
                id,
                results,
                ResetTransactionTimer()
            );
        }

        private string GenerateInitTransactionResponse(int id)
        {
            return GenerateInitTransactionResponse(id, string.Empty);
        }

        [Test]
        public void CommitWithoutRequestsShouldNotGenerateMessage()
        {
            var initTransactionRequest = MockRequest.PostJson("/transaction", @"{'statements': []}");
            var commitTransactionRequest = MockRequest.PostJson("/transaction/1/commit", @"{'statements': []}");
            using (var testHarness = new RestTestHarness
            {
                {
                    initTransactionRequest, MockResponse.Json(201, GenerateInitTransactionResponse(1), "http://foo/db/data/transaction/1")
                },
                {
                    commitTransactionRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            }.ShouldNotBeCalled(initTransactionRequest, commitTransactionRequest))
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {
                    // no requests
                    transaction.Commit();
                }
            }
        }

        [Test]
        public void KeepAliveWithoutRequestsShouldNotGenerateMessage()
        {
            var initTransactionRequest = MockRequest.PostJson("/transaction", @"{'statements': []}");
            var keepAliveRequest = MockRequest.PostJson("/transaction/1", @"{'statements': []}");
            using (var testHarness = new RestTestHarness
            {
                {
                    initTransactionRequest,
                    MockResponse.Json(201, GenerateInitTransactionResponse(1), "http://foo/db/data/transaction/1")
                },
                {
                    keepAliveRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            }.ShouldNotBeCalled(initTransactionRequest, keepAliveRequest))
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {
                    // no requests
                    transaction.KeepAlive();
                }
            }
        }

        [Test]
        public void RollbackWithoutRequestsShouldNotGenerateMessage()
        {
            var initTransactionRequest = MockRequest.PostJson("/transaction", @"{'statements': []}");
            var rollbackTransactionRequest = MockRequest.Delete("/transaction/1");
            using (var testHarness = new RestTestHarness
            {
                {
                    initTransactionRequest,
                    MockResponse.Json(201, GenerateInitTransactionResponse(1), "http://foo/db/data/transaction/1")
                },
                {
                    rollbackTransactionRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            }.ShouldNotBeCalled(initTransactionRequest, rollbackTransactionRequest))
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {
                    // no requests
                    transaction.Rollback();
                }
            }
        }

        [Test]
        public void UpdateTransactionEndpointAfterFirstRequest()
        {
            var initTransactionRequest = MockRequest.PostJson("/transaction", @"{
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
            var rollbackTransactionRequest = MockRequest.Delete("/transaction/1");
            using (var testHarness = new RestTestHarness
            {
                {
                    initTransactionRequest,
                    MockResponse.Json(201, GenerateInitTransactionResponse(1), "http://foo/db/data/transaction/1")
                },
                {
                    rollbackTransactionRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            })
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {
                    // dummy query to generate request
                    client.Cypher
                        .Match("n")
                        .Return(n => n.Count())
                        .ExecuteWithoutResults();

                    Assert.AreEqual(
                        new Uri("http://foo/db/data/transaction/1"),
                        ((INeo4jTransaction)((TransactionScopeProxy) transaction).TransactionContext).Endpoint);
                }
            }
        }

        [Test]
        public void ExecuteMultipleStatementInOneRequest()
        {
            var initTransactionRequest = MockRequest.PostJson("/transaction", @"{
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}, {'statement': 'MATCH t\r\nRETURN count(t)', 'resultDataContents':[], 'parameters': {}}]}");
            var commitRequest = MockRequest.PostJson("/transaction/1/commit", @"{'statements': []}");
            using (var testHarness = new RestTestHarness
            {
                {
                    initTransactionRequest,
                    MockResponse.Json(201, GenerateInitTransactionResponse(1), "http://foo/db/data/transaction/1")
                },
                {
                    commitRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            })
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {
                    // dummy query to generate request
                    var rawClient = client as IRawGraphClient;
                    if (rawClient == null)
                    {
                        Assert.Fail("ITransactionalGraphClient is not IRawGraphClient");
                    }

                    var queries = new List<CypherQuery>()
                    {
                        client.Cypher
                            .Match("n")
                            .Return(n => n.Count())
                            .Query,
                        client.Cypher
                            .Match("t")
                            .Return(t => t.Count())
                            .Query
                    };

                    rawClient.ExecuteMultipleCypherQueriesInTransaction(queries);
                    transaction.Commit();
                }
            }
        }

        [Test]
        public void TransactionCommit()
        {
            var initTransactionRequest = MockRequest.PostJson("/transaction", @"{
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
            var commitRequest = MockRequest.PostJson("/transaction/1/commit", @"{'statements': []}");
            using (var testHarness = new RestTestHarness
            {
                {
                    initTransactionRequest,
                    MockResponse.Json(201, GenerateInitTransactionResponse(1), "http://foo/db/data/transaction/1")
                },
                {
                    commitRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            })
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {
                    // dummy query to generate request
                    client.Cypher
                        .Match("n")
                        .Return(n => n.Count())
                        .ExecuteWithoutResults();

                    transaction.Commit();
                }
            }
        }

        [Test]
        public void PromoteDurableInAmbientTransaction()
        {
            // when two durables are registered they get promoted

            // this request will be made by the ForceKeepAlive() call when PSPE registration fails for the second client
            var afterPspeFailRequest = MockRequest.PostJson("/transaction", @"{'statements': []}");
            // this request will be mode in Promote() after second durable enlistment
            var promoteRequest = MockRequest.PostJson("/transaction/1", @"{'statements': []}");
            var initTransactionRequest = MockRequest.PostJson("/transaction", @"{
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");

            var secondClientRequest = MockRequest.PostJson("/transaction/2", @"{
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");

            // there are no delete requests because those will be made in another app domain

            using (var testHarness = new RestTestHarness
            {
                {
                    initTransactionRequest,
                    MockResponse.Json(201, GenerateInitTransactionResponse(1), "http://foo/db/data/transaction/1")
                },
                {
                    secondClientRequest,
                    MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                },
                {
                    afterPspeFailRequest,
                    MockResponse.Json(201, GenerateInitTransactionResponse(2), "http://foo/db/data/transaction/2")
                },
                {
                    promoteRequest,
                    MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            })
            {
                using (var scope = new TransactionScope())
                {
                    var client = testHarness.CreateAndConnectTransactionalGraphClient();

                    client.Cypher
                        .Match("n")
                        .Return(n => n.Count())
                        .ExecuteWithoutResults();

                    var client2 = testHarness.CreateAndConnectTransactionalGraphClient();

                    client2.Cypher
                        .Match("n")
                        .Return(n => n.Count())
                        .ExecuteWithoutResults();
                }

                // we sleep so that the app domain for the resource manager gets cleaned up
                Thread.Sleep(500);
            }
        }

        [Test]
        public void SuppressTransactionScopeShouldNotEmitTransactionalQuery()
        {
            var initTransactionRequest = MockRequest.PostJson("/transaction", @"{
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");

            var nonTransactionalRequest = MockRequest.PostJson("/cypher", @"{'query': 'MATCH n\r\nRETURN count(n)', 'params': {}}");

            var commitTransactionRequest = MockRequest.PostJson("/transaction/1/commit", @"{
                'statements': []}");
            var deleteRequest = MockRequest.Delete("/transaction/1");
            using (var testHarness = new RestTestHarness
            {
                {
                    initTransactionRequest,
                    MockResponse.Json(201, GenerateInitTransactionResponse(1), "http://foo/db/data/transaction/1")
                },
                {
                    commitTransactionRequest,
                    MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                },
                {
                    nonTransactionalRequest,
                    MockResponse.Json(200, @"{'columns':['count(n)'], 'data':[[0]] }")
                },
                {
                    deleteRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            }.ShouldNotBeCalled(deleteRequest))
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var tran = new TransactionScope())
                {
                    using (var tran2 = new TransactionScope(TransactionScopeOption.Suppress))
                    {
                        client.Cypher
                            .Match("n")
                            .Return(n => n.Count())
                            .ExecuteWithoutResults();

                        // no rollback should be generated
                    }

                    client.Cypher
                        .Match("n")
                        .Return(n => n.Count())
                        .ExecuteWithoutResults();

                    tran.Complete();
                }
            }
        }

        [Test]
        public void NestedRequiresNewTransactionScope()
        {
            var initTransactionRequest = MockRequest.PostJson("/transaction", @"{
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
            var commitTransactionRequest = MockRequest.PostJson("/transaction/1/commit", @"{
                'statements': []}");
            var deleteRequest = MockRequest.Delete("/transaction/1");
            using (var testHarness = new RestTestHarness
            {
                {
                    initTransactionRequest,
                    MockResponse.Json(201, GenerateInitTransactionResponse(1), "http://foo/db/data/transaction/1")
                },
                {
                    commitTransactionRequest,
                    MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                },
                {
                    deleteRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            })
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var scope = new TransactionScope())
                {
                    using (var scope2 = new TransactionScope(TransactionScopeOption.RequiresNew))
                    {
                        client.Cypher
                            .Match("n")
                            .Return(n => n.Count())
                            .ExecuteWithoutResults();

                        // this should commit
                        scope2.Complete();
                    }

                    client.Cypher
                            .Match("n")
                            .Return(n => n.Count())
                            .ExecuteWithoutResults();
                    // this should rollback
                }
            }
        }

        [Test]
        public void NestedJoinedTransactionScope()
        {
            var initTransactionRequest = MockRequest.PostJson("/transaction", @"{
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
            var secondRequest = MockRequest.PostJson("/transaction/1", @"{
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
            var deleteRequest = MockRequest.Delete("/transaction/1");
            using (var testHarness = new RestTestHarness
            {
                {
                    initTransactionRequest,
                    MockResponse.Json(201, GenerateInitTransactionResponse(1), "http://foo/db/data/transaction/1")
                },
                {
                    secondRequest,
                    MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                },
                {
                    deleteRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            })
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var scope = new TransactionScope())
                {
                    using (var scope2 = new TransactionScope())
                    {
                        client.Cypher
                            .Match("n")
                            .Return(n => n.Count())
                            .ExecuteWithoutResults();

                        // this will not commit
                        scope2.Complete();
                    }

                    // this should generate a request to the known transaction ID
                    client.Cypher
                        .Match("n")
                        .Return(n => n.Count())
                        .ExecuteWithoutResults();
                }
            }
        }

        [Test]
        public void TransactionRollbackInTransactionScope()
        {
            var initTransactionRequest = MockRequest.PostJson("/transaction", @"{
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
            var deleteRequest = MockRequest.Delete("/transaction/1");
            using (var testHarness = new RestTestHarness
            {
                {
                    initTransactionRequest,
                    MockResponse.Json(201, GenerateInitTransactionResponse(1), "http://foo/db/data/transaction/1")
                },
                {
                    deleteRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            })
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var scope = new TransactionScope())
                {
                    client.Cypher
                        .Match("n")
                        .Return(n => n.Count())
                        .ExecuteWithoutResults();
                }
            }
        }

        [Test]
        public void NestedTransactionWithTransactionScopeQueryFirst()
        {
            const string queryTextMsTransaction = @"MATCH (n) RETURN count(n)";
            const string queryTextTx = @"MATCH (t) RETURN count(t)";
            const string resultColumn = @"{'columns':['count(n)'], 'data':[{'row':[1]}]}";
            var cypherQueryMsTx = new CypherQuery(queryTextMsTransaction, new Dictionary<string, object>(), CypherResultMode.Projection);
            var cypherQueryMsTxStatement = new CypherStatementList { new CypherTransactionStatement(cypherQueryMsTx, false) };
            var cypherQueryTx = new CypherQuery(queryTextTx, new Dictionary<string, object>(), CypherResultMode.Projection);
            var cypherQueryTxStatement = new CypherStatementList { new CypherTransactionStatement(cypherQueryTx, false) };
            var deleteRequest = MockRequest.Delete("/transaction/1");
            var commitRequest = MockRequest.PostJson("/transaction/1/commit", @"{'statements': []}");
            var commitRequestTx = MockRequest.PostJson("/transaction/2/commit", @"{'statements': []}");

            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.PostObjectAsJson("/transaction", cypherQueryMsTxStatement),
                    MockResponse.Json(201, GenerateInitTransactionResponse(1, resultColumn), "http://foo/db/data/transaction/1")
                },
                {
                    MockRequest.PostObjectAsJson("/transaction", cypherQueryTxStatement),
                    MockResponse.Json(201, GenerateInitTransactionResponse(2, resultColumn), "http://foo/db/data/transaction/2")
                },
                {
                    commitRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                },
                {
                    commitRequestTx, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                },
                {
                    deleteRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            }.ShouldNotBeCalled(commitRequest))
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var msTransaction = new TransactionScope())
                {
                    Assert.IsTrue(client.InTransaction);

                    long totalMsTx = client.Cypher
                        .Match("(n)")
                        .Return(n => n.Count())
                        .Results
                        .SingleOrDefault();
                    Assert.AreEqual(1, totalMsTx);

                    using (var tx = client.BeginTransaction())
                    {
                        long total = client.Cypher
                            .Match("(t)")
                            .Return(t => t.Count())
                            .Results
                            .SingleOrDefault();

                        Assert.AreEqual(1, total);

                        // should not be called
                        tx.Commit();
                    }
                }
            }
        }

        [Test]
        public void NestedTransactionMixedBetweenTransactionScopeAndBeginTransaction()
        {
            const string queryText = @"MATCH (n) RETURN count(n)";
            const string resultColumn = @"{'columns':['count(n)'], 'data':[{'row':[1]}]}";
            var cypherQuery = new CypherQuery(queryText, new Dictionary<string, object>(), CypherResultMode.Projection);
            var cypherApiQuery = new CypherStatementList { new CypherTransactionStatement(cypherQuery, false) };
            var deleteRequest = MockRequest.Delete("/transaction/1");
            var commitRequest = MockRequest.PostJson("/transaction/1/commit", @"{'statements': []}");
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.PostObjectAsJson("/transaction", cypherApiQuery),
                    MockResponse.Json(201, GenerateInitTransactionResponse(1, resultColumn), "http://foo/db/data/transaction/1")
                },
                {
                    commitRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                },
                {
                    deleteRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            }.ShouldNotBeCalled(commitRequest))
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var parentTx = client.BeginTransaction())
                {
                    using (var msTransaction = new TransactionScope())
                    {
                        Assert.IsTrue(client.InTransaction);

                        using (var tx = client.BeginTransaction())
                        {
                            long total = client.Cypher
                                .Match("(n)")
                                .Return(n => n.Count())
                                .Results
                                .SingleOrDefault();

                            Assert.AreEqual(1, total);

                            // should not be called
                            tx.Commit();
                        }

                        msTransaction.Complete();
                    }
                }

                Assert.IsFalse(client.InTransaction);
            }
        }

        [Test]
        public void TestTransactionScopeWithSimpleDeserialization()
        {
            const string queryText = @"MATCH (n) RETURN count(n)";
            const string resultColumn = @"{'columns':['count(n)'], 'data':[{'row':[1]}]}";
            var cypherQuery = new CypherQuery(queryText, new Dictionary<string, object>(), CypherResultMode.Projection);
            var cypherApiQuery = new CypherStatementList { new CypherTransactionStatement(cypherQuery, false) };
            var commitRequest = MockRequest.PostJson("/transaction/1/commit", @"{'statements': []}");

            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.PostObjectAsJson("/transaction", cypherApiQuery),
                    MockResponse.Json(201, GenerateInitTransactionResponse(1, resultColumn), "http://foo/db/data/transaction/1")
                },
                {
                    commitRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            })
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var msTransaction = new TransactionScope())
                {
                    Assert.IsTrue(client.InTransaction);

                    long total = client.Cypher
                        .Match("(n)")
                        .Return(n => n.Count())
                        .Results
                        .SingleOrDefault();

                    Assert.AreEqual(1, total);

                    msTransaction.Complete();
                }

                Assert.IsFalse(client.InTransaction);
            }
        }

        [Test]
        public void TestTransactionScopeWithComplexDeserialization()
        {
            const string queryText = @"MATCH (dt:DummyTotal) RETURN dt";
            const string resultColumn = @"{'columns':['dt'],'data':[{'row':[{'total':1234}]}]}";
            var cypherQuery = new CypherQuery(queryText, new Dictionary<string, object>(), CypherResultMode.Projection);
            var cypherApiQuery = new CypherStatementList { new CypherTransactionStatement(cypherQuery, false) };
            var commitRequest = MockRequest.PostJson("/transaction/1/commit", @"{'statements': []}");

            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.PostObjectAsJson("/transaction", cypherApiQuery),
                    MockResponse.Json(201, GenerateInitTransactionResponse(1, resultColumn), "http://foo/db/data/transaction/1")
                },
                {
                    commitRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            })
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                client.JsonContractResolver = new CamelCasePropertyNamesContractResolver();
                using (var msTransaction = new TransactionScope())
                {
                    Assert.IsTrue(client.InTransaction);

                    var results = client.Cypher.Match("(dt:DummyTotal)")
                        .Return(dt => dt.As<DummyTotal>())
                        .Results
                        .ToList();

                    Assert.AreEqual(1, results.Count());
                    Assert.AreEqual(1234, results.First().Total);

                    msTransaction.Complete();
                }

                Assert.IsFalse(client.InTransaction);
            }
        }

        [Test]
        public void TransactionCommitInTransactionScope()
        {
            var initTransactionRequest = MockRequest.PostJson("/transaction", @"{
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
            var commitRequest = MockRequest.PostJson("/transaction/1/commit", @"{'statements': []}");
            using (var testHarness = new RestTestHarness
            {
                {
                    initTransactionRequest,
                    MockResponse.Json(201, GenerateInitTransactionResponse(1), "http://foo/db/data/transaction/1")
                },
                {
                    commitRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            })
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var scope = new TransactionScope())
                {
                    client.Cypher
                        .Match("n")
                        .Return(n => n.Count())
                        .ExecuteWithoutResults();
                    scope.Complete();
                }
            }
        }

        [Test]
        public void SecondRequestDoesntReturnCreateHttpStatus()
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
                    MockResponse.Json(201, GenerateInitTransactionResponse(1), "http://foo/db/data/transaction/1")
                },
                {
                    commitRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                },
                {
                    secondRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            })
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {
                    // dummy queries to generate request
                    client.Cypher
                        .Match("n")
                        .Return(n => n.Count())
                        .ExecuteWithoutResults();

                    client.Cypher
                        .Match("t")
                        .Return(t => t.Count())
                        .ExecuteWithoutResults();

                    transaction.Commit();
                }
            }
        }

        [Test]
        public void KeepAliveAfterFirstRequest()
        {
            var initTransactionRequest = MockRequest.PostJson("/transaction", @"{
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
            var rollbackTransactionRequest = MockRequest.Delete("/transaction/1");
            var keepAliveRequest = MockRequest.PostJson("/transaction/1", @"{'statements': []}");
            using (var testHarness = new RestTestHarness
            {
                {
                    initTransactionRequest, MockResponse.Json(201, GenerateInitTransactionResponse(1), "http://foo/db/data/transaction/1")
                },
                {
                    keepAliveRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                },
                {
                    rollbackTransactionRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            })
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {
                    // dummy query to generate request
                    client.Cypher
                         .Match("n")
                         .Return(n => n.Count())
                         .ExecuteWithoutResults();

                    transaction.KeepAlive();
                }
            }
        }

        [Test]
        public void DeserializeResultsFromTransaction()
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
                    MockResponse.Json(201, GenerateInitTransactionResponse(1), "http://foo/db/data/transaction/1")
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
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {
                    // dummy query to generate request
                    client.Cypher
                        .Match("n")
                        .Return(n => n.Count())
                        .ExecuteWithoutResults();

                    // this query will hit the deserializer
                    var count = client.Cypher
                        .Match("n")
                        .Return(n => n.Count())
                        .Results;

                    Assert.AreEqual(count.First(), 0);
                }
            }
        }

        [Test]
        public void OnTransactionDisposeCallRollback()
        {
            var initTransactionRequest = MockRequest.PostJson("/transaction", @"{
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
            var rollbackTransactionRequest = MockRequest.Delete("/transaction/1");
            using (var testHarness = new RestTestHarness
            {
                {
                    initTransactionRequest,
                    MockResponse.Json(201, GenerateInitTransactionResponse(1), "http://foo/db/data/transaction/1")
                },
                {
                    rollbackTransactionRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            })
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {
                    // dummy query to generate request
                    client.Cypher
                        .Match("n")
                        .Return(n => n.Count())
                        .ExecuteWithoutResults();
                }
            }
        }

        public class DummyTotal
        {
            public int Total { get; set; }
        }

        [Test]
        public void ExecuteAsyncRequestInTransaction()
        {
            const string queryText = @"MATCH (n) RETURN count(n) as Total";
            const string resultColumn = @"{'columns':['Total'], 'data':[{'row':[1]}]}";

            var cypherQuery = new CypherQuery(queryText, new Dictionary<string, object>(), CypherResultMode.Projection);
            var cypherApiQuery = new CypherStatementList { new CypherTransactionStatement(cypherQuery, false) };
            var commitRequest = MockRequest.PostJson("/transaction/1/commit", @"{'statements': []}");
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.PostObjectAsJson("/transaction", cypherApiQuery),
                    MockResponse.Json(201, GenerateInitTransactionResponse(1, resultColumn), "http://foo/db/data/transaction/1")
                },
                {
                    commitRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")
                }
            })
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                var rawClient = (IRawGraphClient) client;
                using (var tran = client.BeginTransaction())
                {
                    var totalObj = rawClient.ExecuteGetCypherResultsAsync<DummyTotal>(cypherQuery).Result.Single();
                    Assert.AreEqual(1, totalObj.Total);
                    tran.Commit();
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

            protected override HttpResponseMessage HandleRequest(HttpRequestMessage request, string baseUri)
            {
                if (request.Method == HttpMethod.Post)
                {
                    var content = request.Content.ReadAsString();
                    int totalIndex = content.IndexOf("RETURN ", StringComparison.InvariantCultureIgnoreCase);
                    if (totalIndex > 0)
                    {
                        totalIndex += "RETURN ".Length;
                        int spaceIndex = content.IndexOf(" ", totalIndex, StringComparison.InvariantCultureIgnoreCase);
                        Assert.Greater(spaceIndex, totalIndex);
                        Queue.Enqueue(int.Parse(content.Substring(totalIndex, spaceIndex - totalIndex)));
                    }
                }

                return base.HandleRequest(request, baseUri);
            }
        }

        [Test]
        public void AsyncRequestsInTransactionShouldBeExecutedInOrder()
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
                    CypherResultMode.Projection);
                apiQueries[i] = new CypherStatementList {new CypherTransactionStatement(queries[i], false)};
                responses[i] = MockResponse.Json(200,
                    @"{'results':[" + string.Format(resultColumnBase, i) + @"], 'errors':[] }");
                if (i > 0)
                {
                    testHarness.Add(MockRequest.PostObjectAsJson("/transaction/1", apiQueries[i]), responses[i]);
                }
            }

            testHarness.Add(
                MockRequest.PostObjectAsJson("/transaction", apiQueries[0]),
                MockResponse.Json(201, GenerateInitTransactionResponse(1, string.Format(resultColumnBase, 0)),
                    "http://foo/db/data/transaction/1")
                );
            testHarness.Add(
                MockRequest.PostJson("/transaction/1/commit", @"{'statements': []}"),
                MockResponse.Json(200, @"{'results':[], 'errors':[] }")
            );
            try
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                var rawClient = (IRawGraphClient)client;
                var tasks = new Task[asyncRequests];
                using (var tran = client.BeginTransaction())
                {
                    for (int i = 0; i < asyncRequests; i++)
                    {
                        int tmpResult = i;
                        tasks[i] = rawClient.ExecuteGetCypherResultsAsync<DummyTotal>(queries[i]).ContinueWith(task =>
                        {
                            Assert.AreEqual(tmpResult, task.Result.Single().Total);
                        });
                    }

                    Task.WaitAll(tasks);
                    tran.Commit();
                }
            }
            finally
            {
                testHarness.Dispose();
            }

            // check that we have a total order
            Assert.AreEqual(asyncRequests, testHarness.Queue.Count);
            int lastElement = -1;
            for (int i = 0; i < asyncRequests; i++)
            {
                int headItem;
                Assert.IsTrue(testHarness.Queue.TryDequeue(out headItem));
                Assert.Greater(headItem, lastElement);
                lastElement = headItem;
            }
        }

        /// <summary>
        /// This test is flakey. If run in Resharper as a group, it fails. If run by itself it passes.
        /// Appears to be a race condition where for the test to pass the ExecuteGetCypherResultsAsync() call needs to still be in progress before the Commit() is called.
        /// If stepped through, the test will fail since the call easily finishes. Flakeyness observable as early as [bdc1c45]
        /// Perhaps need to insert simulated delay into MockResponse?
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Cannot commit unless all tasks have been completed")]
        public void CommitFailsOnPendingAsyncRequests()
        {
            const string queryText = @"MATCH (n) RETURN count(n) as Total";
            const string resultColumn = @"{'columns':['Total'], 'data':[{'row':[1]}]}";

            var cypherQuery = new CypherQuery(queryText, new Dictionary<string, object>(), CypherResultMode.Projection);
            var cypherApiQuery = new CypherStatementList { new CypherTransactionStatement(cypherQuery, false) };

            using (var testHarness = new RestTestHarness(false)
            {
                {
                    MockRequest.PostObjectAsJson("/transaction", cypherApiQuery),
                    MockResponse.Json(201, GenerateInitTransactionResponse(1, resultColumn), "http://foo/db/data/transaction/1")
                }
            })
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                var rawClient = (IRawGraphClient) client;
                using (var tran = client.BeginTransaction())
                {
                    rawClient.ExecuteGetCypherResultsAsync<DummyTotal>(cypherQuery);
                    tran.Commit();
                }

            }
        }
    }
}
