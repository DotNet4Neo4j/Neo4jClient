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
        [ExpectedException(typeof (NotSupportedException))]
        public void BeginTransactionShouldFailWithLower20Versions()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateGraphClient(false);
                client.Connect();
                client.BeginTransaction();
            }
        }

        [Test]
        [ExpectedException(typeof (InvalidOperationException))]
        public void BeginTransactionShouldFailWithoutConnectingFirst()
        {
            var client = new GraphClient(new Uri("http://foo/db/data"), null);
            client.BeginTransaction();
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
        [ExpectedException(typeof(ClosedTransactionException))]
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
                    using (var tran2 = client.BeginTransaction())
                    {
                    }
                }
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void FailsForCommitInSuppressMode()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var tran = client.BeginTransaction(TransactionScopeOption.Suppress))
                {
                    tran.Commit();
                }
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void FailsForRollbackInSuppressMode()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var tran = client.BeginTransaction(TransactionScopeOption.Suppress))
                {
                    tran.Rollback();
                }
            }
        }

        [Test]
        [ExpectedException(typeof (ClosedTransactionException))]
        public void ShouldNotBeAbleToCommitTwice()
        {
            var transaction = new Neo4jTransaction(new GraphClient(new Uri("http://foo/db/data")));
            transaction.Commit();
            transaction.Commit();
        }

        [Test]
        [ExpectedException(typeof (ClosedTransactionException))]
        public void ShouldNotBeAbleToRollbackTwice()
        {
            var transaction = new Neo4jTransaction(new GraphClient(new Uri("http://foo/db/data")));
            transaction.Rollback();
            transaction.Rollback();
        }

        [Test]
        [ExpectedException(typeof (ClosedTransactionException))]
        public void ShouldNotBeAbleToCommitAfterRollback()
        {
            var transaction = new Neo4jTransaction(new GraphClient(new Uri("http://foo/db/data")));
            transaction.Rollback();
            transaction.Commit();
        }

        [Test]
        [ExpectedException(typeof (ClosedTransactionException))]
        public void ShouldNotBeAbleToRollbackAfterCommit()
        {
            var transaction = new Neo4jTransaction(new GraphClient(new Uri("http://foo/db/data")));
            transaction.Commit();
            transaction.Rollback();
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
