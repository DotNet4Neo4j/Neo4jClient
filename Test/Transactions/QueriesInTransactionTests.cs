using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Neo4jClient.Transactions;
using NUnit.Framework;

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

                    Assert.AreEqual(new Uri("http://foo/db/data/transaction/1"), ((Transaction) transaction).Endpoint);
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
