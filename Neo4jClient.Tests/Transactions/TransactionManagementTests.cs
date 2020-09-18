using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;
using Neo4jClient.Transactions;
using Xunit;
using TransactionManager = Neo4jClient.Transactions.TransactionManager;
using TransactionScopeOption = Neo4jClient.Transactions.TransactionScopeOption;

namespace Neo4jClient.Tests.Transactions
{
    
    public class TransactionManagementTests : IClassFixture<CultureInfoSetupFixture>
    {

        [Theory]
        [InlineData(RestTestHarness.Neo4jVersion.Neo226)]
        [InlineData(RestTestHarness.Neo4jVersion.Neo23)]
        //https://github.com/Readify/Neo4jClient/issues/127
        public async Task ReturnsThe404_WhenVersionIs_2_2_6_Plus_WhenActuallyTimingOut(RestTestHarness.Neo4jVersion version)
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
                await client.ConnectAsync();
                try
                {
                    using (var transaction = client.BeginTransaction())
                    {
                        await client.Cypher.Match("n").Return(n => n.Count()).ExecuteWithoutResultsAsync();
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
        public async Task ReturnsCorrectError_WhenTransactionIsAutomaticallyRolledBack_ViaNeo4j_2_2_6_Plus(RestTestHarness.Neo4jVersion version)
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
                await client.ConnectAsync();
                using (var transaction = client.BeginTransaction())
                {
                    await Assert.ThrowsAsync<NeoException>(async () => await client.Cypher.Match("n").Return(n => n.Count()).ExecuteWithoutResultsAsync());
                }
            }
        }

        [Theory]
        [InlineData(RestTestHarness.Neo4jVersion.Neo20)]
        [InlineData(RestTestHarness.Neo4jVersion.Neo22)]
        [InlineData(RestTestHarness.Neo4jVersion.Neo225)]
        //https://github.com/Readify/Neo4jClient/issues/127
        public async Task ReturnsThe404_WhenVersionIsLessThan_2_2_6(RestTestHarness.Neo4jVersion version)
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
                await client.ConnectAsync();
                try
                {
                    using (var transaction = client.BeginTransaction())
                    {
                        await client.Cypher.Match("n").Return(n => n.Count()).ExecuteWithoutResultsAsync();
                    }
                }
                catch (Exception ex)
                {
                    Assert.True(ex.Message.Contains("404"));
                }
            }
        }

        [Fact]
        public async Task EndTransaction_DoesntThrowAnyExceptions_WhenScopedTransactionsIsEmpty()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
                await client.ConnectAsync();

                var tm = new TransactionManager(client);
                TransactionManager.ScopedTransactions = new ThreadContextWrapper<TransactionScopeProxy>();
                
                Assert.Equal(0, TransactionManager.ScopedTransactions.Count);
                tm.EndTransaction();
            }
        }

        [Fact]
        public async Task EndTransaction_DoesntThrowAnyExceptions_WhenScopedTransactionsIsNull()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
                await client.ConnectAsync();

                var tm = new TransactionManager(client);
                TransactionManager.ScopedTransactions = null;

                tm.EndTransaction();
            }
        }

        [Fact]
        public async Task CurrentInternalTransaction_ReturnsNullWhenEmpty()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
                await client.ConnectAsync();

                var tm = new TransactionManager(client);
                TransactionManager.ScopedTransactions = new ThreadContextWrapper<TransactionScopeProxy>();

                Assert.Equal(0, TransactionManager.ScopedTransactions.Count);
                Assert.Null(tm.CurrentInternalTransaction);
            }
        }

        [Fact]
        public async Task CurrentInternalTransaction_ReturnsNullWhenScopedTransactionsIsNull()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
                await client.ConnectAsync();

                var tm = new TransactionManager(client);
                TransactionManager.ScopedTransactions = null;

                Assert.Null(tm.CurrentInternalTransaction);
            }
        }

        [Fact]
        public async Task BeginTransactionShouldFailWithLower20Versions()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = testHarness.CreateGraphClient(RestTestHarness.Neo4jVersion.Neo19);
                await client.ConnectAsync();
                Assert.Throws<NotSupportedException>(() => client.BeginTransaction());
            }
        }

        [Fact]
        public async Task BeginTransactionShouldFailWithoutConnectingFirst()
        {
            var client = new GraphClient(new Uri("http://foo/db/data"), null);
            Assert.Throws<InvalidOperationException>(() => client.BeginTransaction());
        }

        [Fact]
        public async Task ShouldBeAbleToGetTransactionObjectAfterBeginTransaction()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
                await client.ConnectAsync();
                using (var transaction = client.BeginTransaction())
                {
                    Assert.Same(transaction, client.Transaction);
                }
            }
        }

        [Fact]
        public async Task IgnoreSystemTransactionsIfInsideInternalTransaction()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
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
        public async Task ShouldNotBeAbleToGetTransactionAfterTransactionScope()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
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
        public async Task ShouldNotBeInATransactionScopeWhileSuppressed()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
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
        public async Task TransactionJoinedShouldBeTheSame()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
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
        public async Task RequiresNewCreateNewTransaction()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
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
        public async Task JoinTransactionAfterSuppressCreatesNewTransaction()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
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
        public async Task JoinedTransactionsCommitAfterAllEmitVote()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
                using (var tran = client.BeginTransaction())
                {
                    using (var tran2 = client.BeginTransaction())
                    {
                        await tran2.CommitAsync();
                    }

                    Assert.True(tran.IsOpen);

                    using (var tran3 = client.BeginTransaction(TransactionScopeOption.Suppress))
                    {
                        Assert.NotSame(GetRealTransaction(tran3), GetRealTransaction(tran));
                        Assert.False(client.InTransaction);
                    }

                    Assert.True(client.InTransaction);
                    Assert.True(tran.IsOpen);

                    await tran.CommitAsync();
                    Assert.False(tran.IsOpen);
                }
                Assert.False(client.InTransaction);
            }
        }

        [Fact]
        public async Task RollbackInJoinedTransactionClosesAllJoinedScopes()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
                using (var tran = client.BeginTransaction())
                {
                    using (var tran2 = client.BeginTransaction())
                    {
                        await tran2.RollbackAsync();
                    }

                    Assert.False(tran.IsOpen);
                }
                Assert.False(client.InTransaction);
            }
        }

        [Fact]
        public async Task CommitInRequiresNewDoesntAffectParentScope()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
                using (var tran = client.BeginTransaction())
                {
                    using (var tran2 = client.BeginTransaction(TransactionScopeOption.RequiresNew))
                    {
                        await tran2.CommitAsync();
                        Assert.False(tran2.IsOpen);
                    }

                    Assert.True(tran.IsOpen);
                }
                Assert.False(client.InTransaction);
            }
        }

        [Fact]
        public async Task RollbackInRequiresNewDoesntAffectParentScope()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
                using (var tran = client.BeginTransaction())
                {
                    using (var tran2 = client.BeginTransaction(TransactionScopeOption.RequiresNew))
                    {
                        await tran2.RollbackAsync();
                        Assert.False(tran2.IsOpen);
                    }

                    Assert.True(tran.IsOpen);
                }
                Assert.False(client.InTransaction);
            }
        }

        [Fact]
        public async Task CannotJoinAfterClosedTransaction()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
                using (var tran = client.BeginTransaction())
                {
                    await tran.CommitAsync();

                    Assert.False(tran.IsOpen);
                    // should fail here

                    Assert.Throws<ClosedTransactionException>(() => client.BeginTransaction());
                }
            }
        }

        [Fact]
        public async Task FailsForCommitInSuppressMode()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
                using (var tran = client.BeginTransaction(TransactionScopeOption.Suppress))
                {
                    await Assert.ThrowsAsync<InvalidOperationException>(async () => await tran.CommitAsync());
                }
            }
        }

        [Fact]
        public async Task FailsForRollbackInSuppressMode()
        {
            using (var testHarness = new RestTestHarness())
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
                using (var tran = client.BeginTransaction(TransactionScopeOption.Suppress))
                {
                    await Assert.ThrowsAsync<InvalidOperationException>(async () => await tran.RollbackAsync());
                }
            }
        }

    

        [Fact]
        public async Task TwoThreadsShouldNotHaveTheSameTransactionObject()
        {
            // if thread support is not well implemented then the t2's BeginTransaction will fail with NotSupportedException
            ITransaction transactionFromThread1 = null;
            ITransaction transactionFromThread2 = null;
            using (var testHarness = new RestTestHarness())
            {
                var client = await testHarness.CreateAndConnectTransactionalGraphClient();
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
        public async Task ShouldPromoteBadQueryResponseToNiceException()
        {
            // Arrange
            const string queryText = @"broken query";
            var cypherQuery = new CypherQuery(queryText, new Dictionary<string, object>(), CypherResultMode.Projection, "neo4j");
            var cypherApiQuery = new CypherStatementList {new CypherTransactionStatement(cypherQuery)};

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
                var graphClient = await testHarness.CreateAndConnectTransactionalGraphClient();
                var rawClient = (IRawGraphClient) graphClient;

                using (graphClient.BeginTransaction())
                {

                    var ex = await Assert.ThrowsAsync<NeoException>(async () => await rawClient.ExecuteCypherAsync(cypherQuery));
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
