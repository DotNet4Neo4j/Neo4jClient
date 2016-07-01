using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;
using Neo4jClient.Transactions;
using NUnit.Framework;
using TransactionScopeOption = Neo4jClient.Transactions.TransactionScopeOption;

namespace Neo4jClient.Test.Transactions
{
    [TestFixture]
    public class TransactionManagementTests
    {

        [Test]
        //https://github.com/Readify/Neo4jClient/issues/127
        public void ReturnsThe404_WhenVersionIs_2_2_6_Plus_WhenActuallyTimingOut([Values(RestTestHarness.Neo4jVersion.Neo226, RestTestHarness.Neo4jVersion.Neo23)] RestTestHarness.Neo4jVersion version)
        {
            var initTransactionRequest = MockRequest.PostJson("/transaction", @"{
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
            var rollbackTransactionRequest = MockRequest.Delete("/transaction/1");
            using (var testHarness = new RestTestHarness()
            {
                {
                    initTransactionRequest,
                    MockResponse.Json(201, TransactionRestResponseHelper.GenerateInitTransactionResponse(1), "http://foo/db/data/transaction/1")
                },
                {
                    rollbackTransactionRequest, MockResponse.Json(404, "{\"results\":[],\"errors\":[{\"code\":\"Neo.ClientError.Transaction.UnknownId\",\"message\":\"Unrecognized transaction id. Transaction may have timed out and been rolled back.\"}]}")
                }
            })
            {
                var client = testHarness.CreateGraphClient(version);
                client.Connect();
                try
                {
                    using (var transaction = client.BeginTransaction())
                    {
                        client.Cypher.Match("n").Return(n => n.Count()).ExecuteWithoutResults();
                    }
                    Assert.Fail("Should not reach this code, as there is an expected exception.");
                }
                catch (Exception ex)
                {
                    Assert.That(ex.Message.Contains("404"));
                }
            }
        }


        [Test]
        //https://github.com/Readify/Neo4jClient/issues/127
        public void ReturnsCorrectError_WhenTransactionIsAutomaticallyRolledBack_ViaNeo4j_2_2_6_Plus([Values(RestTestHarness.Neo4jVersion.Neo226/*, RestTestHarness.Neo4jVersion.Neo23*/)] RestTestHarness.Neo4jVersion version)
        {
            /* In 2.2.6 ClientErrors (Constraint Violations etc) were changed to Automatically rollback. This created a 404 error when *we* tried to rollback on an error, as the transaction no longer existed. */
            var initTransactionRequest = MockRequest.PostJson("/transaction", @"{
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
            var rollbackTransactionRequest = MockRequest.Delete("/transaction/1");
            using (var testHarness = new RestTestHarness
            {
                {
                    initTransactionRequest,
                    MockResponse.Json(201, TransactionRestResponseHelper.GenerateCypherErrorResponse(1, "{\"code\":\"Neo.ClientError.Schema.ConstraintViolation\",\"message\":\"Node 19572 already exists with label User and property.\"}"), "http://foo/db/data/transaction/1")
                },
                {
                    rollbackTransactionRequest, MockResponse.Json(404, "{\"results\":[],\"errors\":[{\"code\":\"Neo.ClientError.Transaction.UnknownId\",\"message\":\"Unrecognized transaction id. Transaction may have timed out and been rolled back.\"}]}")
                }
            })
            {
                var client = testHarness.CreateGraphClient(version);
                client.Connect();
                using (var transaction = client.BeginTransaction())
                {
                    Assert.That(() => client.Cypher.Match("n").Return(n => n.Count()).ExecuteWithoutResults(), Throws.TypeOf<NeoException>());
                }
            }
        }

        [Test]
        //https://github.com/Readify/Neo4jClient/issues/127
        public void ReturnsThe404_WhenVersionIsLessThan_2_2_6([Values(RestTestHarness.Neo4jVersion.Neo20, RestTestHarness.Neo4jVersion.Neo22, RestTestHarness.Neo4jVersion.Neo225)] RestTestHarness.Neo4jVersion version)
        {
            var initTransactionRequest = MockRequest.PostJson("/transaction", @"{
                'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
            var rollbackTransactionRequest = MockRequest.Delete("/transaction/1");
            using (var testHarness = new RestTestHarness
            {
                {
                    initTransactionRequest,
                    MockResponse.Json(201, TransactionRestResponseHelper.GenerateCypherErrorResponse(1, "{\"code\":\"Neo.ClientError.Schema.ConstraintViolation\",\"message\":\"Node 19572 already exists with label User and property.\"}"), "http://foo/db/data/transaction/1")
                },
                {
                    rollbackTransactionRequest, MockResponse.Json(404, "{\"results\":[],\"errors\":[{\"code\":\"Neo.ClientError.Transaction.UnknownId\",\"message\":\"Unrecognized transaction id. Transaction may have timed out and been rolled back.\"}]}")
                }
            })
            {
                var client = testHarness.CreateGraphClient(version);
                client.Connect();
                try
                {
                    using (var transaction = client.BeginTransaction())
                    {
                        client.Cypher.Match("n").Return(n => n.Count()).ExecuteWithoutResults();
                    }
                }
                catch (Exception ex)
                {
                    Assert.That(ex.Message.Contains("404"));
                }
            }
        }

        [Test]
        public void EndTransaction_DoesntThrowAnyExceptions_WhenScopedTransactionsIsEmpty()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                client.Connect();

                var tm = new Neo4jClient.Transactions.TransactionManager(client)
                {
                    ScopedTransactions = new Stack<TransactionScopeProxy>()
                };
                Assert.AreEqual(0, tm.ScopedTransactions.Count);
                tm.EndTransaction();
            }
        }

        [Test]
        public void EndTransaction_DoesntThrowAnyExceptions_WhenScopedTransactionsIsNull()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                client.Connect();

                var tm = new Neo4jClient.Transactions.TransactionManager(client)
                {
                    ScopedTransactions = null
                };

                tm.EndTransaction();
            }
        }

        [Test]
        public void CurrentInternalTransaction_ReturnsNullWhenEmpty()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                client.Connect();

                var tm = new Neo4jClient.Transactions.TransactionManager(client)
                {
                    ScopedTransactions = new Stack<TransactionScopeProxy>()
                };
                Assert.AreEqual(0, tm.ScopedTransactions.Count);
                Assert.IsNull(tm.CurrentInternalTransaction);
            }
        }

        [Test]
        public void CurrentInternalTransaction_ReturnsNullWhenScopedTransactionsIsNull()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                client.Connect();

                var tm = new Neo4jClient.Transactions.TransactionManager(client)
                {
                    ScopedTransactions = null
                };
                Assert.IsNull(tm.CurrentInternalTransaction);
            }
        }

        [Test]
        public void BeginTransactionShouldFailWithLower20Versions()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateGraphClient(RestTestHarness.Neo4jVersion.Neo19);
                client.Connect();
                Assert.That(() => client.BeginTransaction(), Throws.TypeOf<NotSupportedException>());
            }
        }

        [Test]
        public void BeginTransactionShouldFailWithoutConnectingFirst()
        {
            var client = new GraphClient(new Uri("http://foo/db/data"), null);
            Assert.That(() => client.BeginTransaction(), Throws.InvalidOperationException);
        }

        [Test]
        public void ShouldBeAbleToGetTransactionObjectAfterBeginTransaction()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                client.Connect();
                using (var transaction = client.BeginTransaction())
                {
                    Assert.AreSame(transaction, client.Transaction);
                }
            }
        }

        [Test]
        public void IgnoreSystemTransactionsIfInsideInternalTransaction()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {
                    using (var msTransaction = new TransactionScope())
                    {
                        Assert.IsTrue(client.InTransaction);
                    }
                    Assert.IsTrue(client.InTransaction);
                }

            }
        }

        [Test]
        public void ShouldNotBeAbleToGetTransactionAfterTransactionScope()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {

                }

                Assert.IsNull(client.Transaction);
            }
        }

        private ITransaction GetRealTransaction(ITransaction proxiedTransaction)
        {
            var txContext = ((TransactionScopeProxy) proxiedTransaction).TransactionContext;
            return txContext == null ? null : txContext.Transaction;
        }

        [Test]
        public void ShouldNotBeInATransactionScopeWhileSuppressed()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {
                    using (var tran2 = client.BeginTransaction(TransactionScopeOption.Suppress))
                    {
                        Assert.AreNotSame(GetRealTransaction(tran2), GetRealTransaction(transaction));
                        Assert.IsFalse(client.InTransaction);
                    }
                }
            }
        }

        [Test]
        public void TransactionJoinedShouldBeTheSame()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {

                    using (var tran2 = client.BeginTransaction())
                    {
                        Assert.AreSame(GetRealTransaction(tran2), GetRealTransaction(transaction));
                    }
                }
            }
        }

        [Test]
        public void RequiresNewCreateNewTransaction()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {
                    using (var tran2 = client.BeginTransaction(TransactionScopeOption.RequiresNew))
                    {
                        Assert.AreNotSame(GetRealTransaction(tran2), GetRealTransaction(transaction));
                        Assert.IsTrue(client.InTransaction);
                    }
                    Assert.IsTrue(client.InTransaction);
                }
                Assert.IsFalse(client.InTransaction);
            }
        }

        [Test]
        public void JoinTransactionAfterSuppressCreatesNewTransaction()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var tran = client.BeginTransaction())
                {
                    using (var tran2 = client.BeginTransaction(TransactionScopeOption.Suppress))
                    {
                        Assert.AreNotSame(tran2, tran);
                        Assert.IsFalse(client.InTransaction);
                        using (var tran3 = client.BeginTransaction(TransactionScopeOption.Join))
                        {
                            Assert.AreNotSame(GetRealTransaction(tran2), GetRealTransaction(tran3));
                            Assert.AreNotSame(GetRealTransaction(tran3), GetRealTransaction(tran2));
                            Assert.IsTrue(client.InTransaction);
                        }
                    }
                    Assert.IsTrue(client.InTransaction);
                }
                Assert.IsFalse(client.InTransaction);
            }
        }

        [Test]
        public void JoinedTransactionsCommitAfterAllEmitVote()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var tran = client.BeginTransaction())
                {
                    using (var tran2 = client.BeginTransaction())
                    {
                        tran2.Commit();
                    }

                    Assert.IsTrue(tran.IsOpen);

                    using (var tran3 = client.BeginTransaction(TransactionScopeOption.Suppress))
                    {
                        Assert.AreNotSame(GetRealTransaction(tran3), GetRealTransaction(tran));
                        Assert.IsFalse(client.InTransaction);
                    }

                    Assert.IsTrue(client.InTransaction);
                    Assert.IsTrue(tran.IsOpen);

                    tran.Commit();
                    Assert.IsFalse(tran.IsOpen);
                }
                Assert.IsFalse(client.InTransaction);
            }
        }

        [Test]
        public void RollbackInJoinedTransactionClosesAllJoinedScopes()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var tran = client.BeginTransaction())
                {
                    using (var tran2 = client.BeginTransaction())
                    {
                        tran2.Rollback();
                    }

                    Assert.IsFalse(tran.IsOpen);
                }
                Assert.IsFalse(client.InTransaction);
            }
        }

        [Test]
        public void CommitInRequiresNewDoesntAffectParentScope()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var tran = client.BeginTransaction())
                {
                    using (var tran2 = client.BeginTransaction(TransactionScopeOption.RequiresNew))
                    {
                        tran2.Commit();
                        Assert.IsFalse(tran2.IsOpen);
                    }

                    Assert.IsTrue(tran.IsOpen);
                }
                Assert.IsFalse(client.InTransaction);
            }
        }

        [Test]
        public void RollbackInRequiresNewDoesntAffectParentScope()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var tran = client.BeginTransaction())
                {
                    using (var tran2 = client.BeginTransaction(TransactionScopeOption.RequiresNew))
                    {
                        tran2.Rollback();
                        Assert.IsFalse(tran2.IsOpen);
                    }

                    Assert.IsTrue(tran.IsOpen);
                }
                Assert.IsFalse(client.InTransaction);
            }
        }

        [Test]
        public void CannotJoinAfterClosedTransaction()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var tran = client.BeginTransaction())
                {
                    tran.Commit();

                    Assert.IsFalse(tran.IsOpen);
                    // should fail here

                    Assert.That(() => client.BeginTransaction(), Throws.TypeOf<ClosedTransactionException>());
                }
            }
        }

        [Test]
        public void FailsForCommitInSuppressMode()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var tran = client.BeginTransaction(TransactionScopeOption.Suppress))
                {
                    Assert.That(() => tran.Commit(), Throws.InvalidOperationException);
                }
            }
        }

        [Test]
        public void FailsForRollbackInSuppressMode()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var tran = client.BeginTransaction(TransactionScopeOption.Suppress))
                {
                    Assert.That(() => tran.Rollback(), Throws.InvalidOperationException);
                }
            }
        }

        [Test]
        public void ShouldNotBeAbleToCommitTwice()
        {
            var transaction = new Neo4jTransaction(new GraphClient(new Uri("http://foo/db/data")));
            transaction.Commit();
            Assert.That(() => transaction.Commit(), Throws.TypeOf<ClosedTransactionException>());
        }

        [Test]
        public void ShouldNotBeAbleToRollbackTwice()
        {
            var transaction = new Neo4jTransaction(new GraphClient(new Uri("http://foo/db/data")));
            transaction.Rollback();
            Assert.That(() => transaction.Rollback(), Throws.TypeOf<ClosedTransactionException>());
        }

        [Test]
        public void ShouldNotBeAbleToCommitAfterRollback()
        {
            var transaction = new Neo4jTransaction(new GraphClient(new Uri("http://foo/db/data")));
            transaction.Rollback();
            Assert.That(() => transaction.Commit(), Throws.TypeOf<ClosedTransactionException>());
        }

        [Test]
        public void ShouldNotBeAbleToRollbackAfterCommit()
        {
            var transaction = new Neo4jTransaction(new GraphClient(new Uri("http://foo/db/data")));
            transaction.Commit();
            Assert.That(() => transaction.Rollback(), Throws.TypeOf<ClosedTransactionException>());
        }

        [Test]
        public void TwoThreadsShouldNotHaveTheSameTransactionObject()
        {
            // if thread support is not well implemented then the t2's BeginTransaction will fail with NotSupportedException
            ITransaction transactionFromThread1 = null;
            ITransaction transactionFromThread2 = null;
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                var firstTransactionSet = new EventWaitHandle(false, EventResetMode.AutoReset);
                var secondTransactionSet = new EventWaitHandle(false, EventResetMode.AutoReset);
                var t1 = new Task(() =>
                {
                    try
                    {
                        using (var transaction = client.BeginTransaction())
                        {
                            transactionFromThread1 = transaction;
                            firstTransactionSet.Set();
                            secondTransactionSet.WaitOne();
                        }
                    }
                    catch (Exception e)
                    {
                        firstTransactionSet.Set();
                        throw;
                    }
                });

                var t2 = new Task(() =>
                {
                    firstTransactionSet.WaitOne();
                    try
                    {
                        using (var transaction = client.BeginTransaction())
                        {
                            transactionFromThread2 = transaction;
                            secondTransactionSet.Set();
                        }
                    }
                    catch (Exception e)
                    {
                        secondTransactionSet.Set();
                        throw;
                    }
                });

                t1.Start();
                t2.Start();
                Task.WaitAll(t1, t2);
                Assert.IsNotNull(transactionFromThread1);
                Assert.IsNotNull(transactionFromThread2);
                Assert.AreNotEqual(transactionFromThread1, transactionFromThread2);

            }
        }

        [Test]
        public void ShouldPromoteBadQueryResponseToNiceException()
        {
            // Arrange
            const string queryText = @"broken query";
            var cypherQuery = new CypherQuery(queryText, new Dictionary<string, object>(), CypherResultMode.Projection);
            var cypherApiQuery = new CypherStatementList {new CypherTransactionStatement(cypherQuery, false)};

            using (var testHarness = new RestTestHarness
                {
                    {
                        MockRequest.PostObjectAsJson("/transaction", cypherApiQuery),
                        MockResponse.Json(HttpStatusCode.OK, @"{'results':[], 'errors': [{
    'code' : 'Neo.ClientError.Statement.InvalidSyntax',
    'message' : 'Invalid input b: expected SingleStatement (line 1, column 1)\nThis is not a valid Cypher Statement.\n ^'
  }]}")
                    }
                })
            {
                var graphClient = testHarness.CreateAndConnectTransactionalGraphClient();
                var rawClient = (IRawGraphClient) graphClient;

                using (graphClient.BeginTransaction())
                {

                    var ex = Assert.Throws<NeoException>(() => rawClient.ExecuteCypher(cypherQuery));
                    Assert.AreEqual("InvalidSyntax: Invalid input b: expected SingleStatement (line 1, column 1)\nThis is not a valid Cypher Statement.\n ^", ex.Message);
                    Assert.AreEqual("Invalid input b: expected SingleStatement (line 1, column 1)\nThis is not a valid Cypher Statement.\n ^", ex.NeoMessage);
                    Assert.AreEqual("InvalidSyntax", ex.NeoExceptionName);
                    Assert.AreEqual("Neo.ClientError.Statement.InvalidSyntax", ex.NeoFullName);
                    CollectionAssert.AreEqual(new String[] {}, ex.NeoStackTrace);
                }
            }
        }
    }
}
