using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Transactions;
using Neo4jClient.Transactions;
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

        private string GenerateInitTransactionResponse(int id)
        {
            return string.Format(
                @"{{'commit': 'http://foo/db/data/transaction/{0}/commit', 'results': [], 'errors': [], 'transaction': {{ 'expires': '{1}' }} }}",
                id,
                ResetTransactionTimer()
            );
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
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'parameters': {}}]}");
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
                        ((INeo4jTransaction)((TransactionScopeProxy) transaction).Transaction).Endpoint);
                }
            }
        }

        [Test]
        public void TransactionCommit()
        {
            var initTransactionRequest = MockRequest.PostJson("/transaction", @"{
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'parameters': {}}]}");
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
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'parameters': {}}]}");

            // there are no delete requests because those will be made in another app domain

            using (var testHarness = new RestTestHarness
            {
                {
                    initTransactionRequest,
                    MockResponse.Json(201, GenerateInitTransactionResponse(1), "http://foo/db/data/transaction/1")
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
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'parameters': {}}]}");

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
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'parameters': {}}]}");
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
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'parameters': {}}]}");
            var secondRequest = MockRequest.PostJson("/transaction/1", @"{
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'parameters': {}}]}");
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
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'parameters': {}}]}");
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
        public void TransactionCommitInTransactionScope()
        {
            var initTransactionRequest = MockRequest.PostJson("/transaction", @"{
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'parameters': {}}]}");
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
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'parameters': {}}]}");
            var secondRequest = MockRequest.PostJson("/transaction/1", @"{
                'statements': [{'statement': 'MATCH t\r\nRETURN count(t)', 'parameters': {}}]}");
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
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'parameters': {}}]}");
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
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'parameters': {}}]}");
            var deserializationRequest = MockRequest.PostJson("/transaction/1", @"{
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'parameters': {}}]}");
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
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'parameters': {}}]}");
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
                        .Return(n =>  n.Count())
                        .ExecuteWithoutResults();
                }
            }
        }
    }
}
