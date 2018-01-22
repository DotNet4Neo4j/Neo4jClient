using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;
using Neo4jClient.Test.Fixtures;
using Neo4jClient.Transactions;
using Xunit;
using TransactionScopeOption = Neo4jClient.Transactions.TransactionScopeOption;

namespace Neo4jClient.Test.Transactions
{
    
    public class TransactionManagementTests : IClassFixture<CultureInfoSetupFixture>
    {

        [Theory]
        [InlineData(RestTestHarness.Neo4jVersion.Neo226)]
        [InlineData(RestTestHarness.Neo4jVersion.Neo23)]
        //https://github.com/Readify/Neo4jClient/issues/127
        public void ReturnsThe404_WhenVersionIs_2_2_6_Plus_WhenActuallyTimingOut(RestTestHarness.Neo4jVersion version)
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
                    throw new Exception("Should not reach this code, as there is an expected exception.");
                }
                catch (Exception ex)
                {
                    Assert.True(ex.Message.Contains("404"));
                }
            }
        }


        [Theory]
        [InlineData(RestTestHarness.Neo4jVersion.Neo226)]
        //https://github.com/Readify/Neo4jClient/issues/127
        public void ReturnsCorrectError_WhenTransactionIsAutomaticallyRolledBack_ViaNeo4j_2_2_6_Plus(RestTestHarness.Neo4jVersion version)
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
                    Assert.Throws<NeoException>(() => client.Cypher.Match("n").Return(n => n.Count()).ExecuteWithoutResults());
                }
            }
        }

        [Theory]
        [InlineData(RestTestHarness.Neo4jVersion.Neo20)]
        [InlineData(RestTestHarness.Neo4jVersion.Neo22)]
        [InlineData(RestTestHarness.Neo4jVersion.Neo225)]
        //https://github.com/Readify/Neo4jClient/issues/127
        public void ReturnsThe404_WhenVersionIsLessThan_2_2_6(RestTestHarness.Neo4jVersion version)
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
                    Assert.True(ex.Message.Contains("404"));
                }
            }
        }

        [Fact]
        public void EndTransaction_DoesntThrowAnyExceptions_WhenScopedTransactionsIsEmpty()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                client.Connect();

                var tm = new Neo4jClient.Transactions.TransactionManager(client)
                {
                    ScopedTransactions = new AsyncLocal<Stack<TransactionScopeProxy>> { Value = new Stack<TransactionScopeProxy>()}
                };
                Assert.Equal(0, tm.ScopedTransactions.Value.Count);
                tm.EndTransaction();
            }
        }

        [Fact]
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

        [Fact]
        public void CurrentInternalTransaction_ReturnsNullWhenEmpty()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                client.Connect();

                var tm = new Neo4jClient.Transactions.TransactionManager(client)
                {
                    ScopedTransactions = new AsyncLocal<Stack<TransactionScopeProxy>> { Value = new Stack<TransactionScopeProxy>()}
                };
                Assert.Equal(0, tm.ScopedTransactions.Value.Count);
                Assert.Null(tm.CurrentInternalTransaction);
            }
        }

        [Fact]
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
                Assert.Null(tm.CurrentInternalTransaction);
            }
        }

        [Fact]
        public void BeginTransactionShouldFailWithLower20Versions()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateGraphClient(RestTestHarness.Neo4jVersion.Neo19);
                client.Connect();
                Assert.Throws<NotSupportedException>(() => client.BeginTransaction());
            }
        }

        [Fact]
        public void BeginTransactionShouldFailWithoutConnectingFirst()
        {
            var client = new GraphClient(new Uri("http://foo/db/data"), null);
            Assert.Throws<InvalidOperationException>(() => client.BeginTransaction());
        }

        [Fact]
        public void ShouldBeAbleToGetTransactionObjectAfterBeginTransaction()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                client.Connect();
                using (var transaction = client.BeginTransaction())
                {
                    Assert.Same(transaction, client.Transaction);
                }
            }
        }

        [Fact]
        public void IgnoreSystemTransactionsIfInsideInternalTransaction()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {
                    using (var msTransaction = new TransactionScope())
                    {
                        Assert.True(client.InTransaction);
                    }
                    Assert.True(client.InTransaction);
                }

            }
        }

        [Fact]
        public void ShouldNotBeAbleToGetTransactionAfterTransactionScope()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {

                }

                Assert.Null(client.Transaction);
            }
        }

        private ITransaction GetRealTransaction(ITransaction proxiedTransaction)
        {
            var txContext = ((TransactionScopeProxy) proxiedTransaction).TransactionContext;
            return txContext == null ? null : txContext.Transaction;
        }

        [Fact]
        public void ShouldNotBeInATransactionScopeWhileSuppressed()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {
                    using (var tran2 = client.BeginTransaction(TransactionScopeOption.Suppress))
                    {
                        Assert.NotSame(GetRealTransaction(tran2), GetRealTransaction(transaction));
                        Assert.False(client.InTransaction);
                    }
                }
            }
        }

        [Fact]
        public void TransactionJoinedShouldBeTheSame()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {

                    using (var tran2 = client.BeginTransaction())
                    {
                        Assert.Same(GetRealTransaction(tran2), GetRealTransaction(transaction));
                    }
                }
            }
        }

        [Fact]
        public void RequiresNewCreateNewTransaction()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction())
                {
                    using (var tran2 = client.BeginTransaction(TransactionScopeOption.RequiresNew))
                    {
                        Assert.NotSame(GetRealTransaction(tran2), GetRealTransaction(transaction));
                        Assert.True(client.InTransaction);
                    }
                    Assert.True(client.InTransaction);
                }
                Assert.False(client.InTransaction);
            }
        }

        [Fact]
        public void JoinTransactionAfterSuppressCreatesNewTransaction()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var tran = client.BeginTransaction())
                {
                    using (var tran2 = client.BeginTransaction(TransactionScopeOption.Suppress))
                    {
                        Assert.NotSame(tran2, tran);
                        Assert.False(client.InTransaction);
                        using (var tran3 = client.BeginTransaction(TransactionScopeOption.Join))
                        {
                            Assert.NotSame(GetRealTransaction(tran2), GetRealTransaction(tran3));
                            Assert.NotSame(GetRealTransaction(tran3), GetRealTransaction(tran2));
                            Assert.True(client.InTransaction);
                        }
                    }
                    Assert.True(client.InTransaction);
                }
                Assert.False(client.InTransaction);
            }
        }

        [Fact]
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

                    Assert.True(tran.IsOpen);

                    using (var tran3 = client.BeginTransaction(TransactionScopeOption.Suppress))
                    {
                        Assert.NotSame(GetRealTransaction(tran3), GetRealTransaction(tran));
                        Assert.False(client.InTransaction);
                    }

                    Assert.True(client.InTransaction);
                    Assert.True(tran.IsOpen);

                    tran.Commit();
                    Assert.False(tran.IsOpen);
                }
                Assert.False(client.InTransaction);
            }
        }

        [Fact]
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

                    Assert.False(tran.IsOpen);
                }
                Assert.False(client.InTransaction);
            }
        }

        [Fact]
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
                        Assert.False(tran2.IsOpen);
                    }

                    Assert.True(tran.IsOpen);
                }
                Assert.False(client.InTransaction);
            }
        }

        [Fact]
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
                        Assert.False(tran2.IsOpen);
                    }

                    Assert.True(tran.IsOpen);
                }
                Assert.False(client.InTransaction);
            }
        }

        [Fact]
        public void CannotJoinAfterClosedTransaction()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var tran = client.BeginTransaction())
                {
                    tran.Commit();

                    Assert.False(tran.IsOpen);
                    // should fail here

                    Assert.Throws<ClosedTransactionException>(() => client.BeginTransaction());
                }
            }
        }

        [Fact]
        public void FailsForCommitInSuppressMode()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var tran = client.BeginTransaction(TransactionScopeOption.Suppress))
                {
                    Assert.Throws<InvalidOperationException>(() => tran.Commit());
                }
            }
        }

        [Fact]
        public void FailsForRollbackInSuppressMode()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var tran = client.BeginTransaction(TransactionScopeOption.Suppress))
                {
                    Assert.Throws<InvalidOperationException>(() => tran.Rollback());
                }
            }
        }

        [Fact]
        public void ShouldNotBeAbleToCommitTwice()
        {
            var transaction = new Neo4jRestTransaction(new GraphClient(new Uri("http://foo/db/data")));
            transaction.Commit();
            Assert.Throws<ClosedTransactionException>(() => transaction.Commit());
        }

        [Fact]
        public void ShouldNotBeAbleToRollbackTwice()
        {
            var transaction = new Neo4jRestTransaction(new GraphClient(new Uri("http://foo/db/data")));
            transaction.Rollback();
            Assert.Throws<ClosedTransactionException>(() => transaction.Rollback());
        }

        [Fact]
        public void ShouldNotBeAbleToCommitAfterRollback()
        {
            var transaction = new Neo4jRestTransaction(new GraphClient(new Uri("http://foo/db/data")));
            transaction.Rollback();
            Assert.Throws<ClosedTransactionException>(() => transaction.Commit());
        }

        [Fact]
        public void ShouldNotBeAbleToRollbackAfterCommit()
        {
            var transaction = new Neo4jRestTransaction(new GraphClient(new Uri("http://foo/db/data")));
            transaction.Commit();
            Assert.Throws<ClosedTransactionException>(() => transaction.Rollback());
        }

        [Fact]
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
                Assert.NotNull(transactionFromThread1);
                Assert.NotNull(transactionFromThread2);
                Assert.NotEqual(transactionFromThread1, transactionFromThread2);

            }
        }

        [Fact]
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
                    Assert.Equal("InvalidSyntax: Invalid input b: expected SingleStatement (line 1, column 1)\nThis is not a valid Cypher Statement.\n ^", ex.Message);
                    Assert.Equal("Invalid input b: expected SingleStatement (line 1, column 1)\nThis is not a valid Cypher Statement.\n ^", ex.NeoMessage);
                    Assert.Equal("InvalidSyntax", ex.NeoExceptionName);
                    Assert.Equal("Neo.ClientError.Statement.InvalidSyntax", ex.NeoFullName);
                   Assert.Equal(new String[] {}, ex.NeoStackTrace);
                }
            }
        }
    }
}
