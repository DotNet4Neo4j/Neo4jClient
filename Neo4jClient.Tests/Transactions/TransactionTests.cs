using System;
using System.Threading.Tasks;
using FluentAssertions;
using Neo4jClient.Transactions;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.Transactions
{
    public class TransactionTests
    {
        /// <summary>
        ///     Transaction tests against the HTTP API
        /// </summary>
        public class Http : IClassFixture<CultureInfoSetupFixture>
        {
            internal static MockResponse EmptyOkResponse = MockResponse.Json(200, @"{'results':[], 'errors':[] }");
            internal static string EmptyStatements = "{'statements': []}";

            [Fact]
            public async Task QueryInTransaction_ThrowsExceptionWhen_ExecutingCypherQueryAgainstADifferentDatabaseName()
            {
                const string database = "neo4jclient";

                var initTransactionRequest = MockRequest.PostJson($"/db/{database}/tx", "{'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
                var commitTransactionRequest = MockRequest.PostJson($"/db/{database}/tx/1/commit", EmptyStatements);
                using (var testHarness = new RestTestHarness(false, "http://foo:7474")
                {
                    {
                        initTransactionRequest,
                        MockResponse.Json(201, TransactionRestResponseHelper.GenerateInitTransactionResponse(1), $"http://foo:7474/db/{database}/tx/1")
                    },
                    {commitTransactionRequest, EmptyOkResponse}
                })
                {
                    var client = await testHarness.CreateAndConnectTransactionalGraphClient(RestTestHarness.Neo4jVersion.Neo40);
                    using (var transaction = client.BeginTransaction(TransactionScopeOption.Join, null, database))
                    {
                        // dummy query to generate request
                        var ex = Assert.Throws<InvalidOperationException>(
                            () => client.Cypher
                                .WithDatabase("neo4j")
                                .Match("n")
                                .Return(n => n.Count()));

                        ex.Should().NotBeNull();
                    }
                }
            }

            public class KeepAliveAsyncMethod : IClassFixture<CultureInfoSetupFixture>
            {
                [Fact]
                public async Task ThrowsClosedTransactionException_WhenTransactionAlreadyClosed()
                {
                    const string database = "neo4jclient";

                    var initTransactionRequest = MockRequest.PostJson($"/db/{database}/tx", "{'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
                    var commitTransactionRequest = MockRequest.PostJson($"/db/{database}/tx/1/commit", EmptyStatements);
                    using (var testHarness = new RestTestHarness(false, "http://foo:7474")
                    {
                        {
                            initTransactionRequest,
                            MockResponse.Json(201, TransactionRestResponseHelper.GenerateInitTransactionResponse(1), $"http://foo:7474/db/{database}/tx/1")
                        },
                        {commitTransactionRequest, EmptyOkResponse}
                    })
                    {
                        var client = await testHarness.CreateAndConnectTransactionalGraphClient(RestTestHarness.Neo4jVersion.Neo40);
                        using (var transaction = client.BeginTransaction(TransactionScopeOption.Join, null, database))
                        {
                            // dummy query to generate request
                            await client.Cypher
                                .Match("n")
                                .Return(n => n.Count())
                                .ExecuteWithoutResultsAsync().ConfigureAwait(false);

                            await transaction.CommitAsync().ConfigureAwait(false);
                            var ex = await Assert.ThrowsAsync<ClosedTransactionException>(async () => await transaction.KeepAliveAsync());
                            ex.Should().NotBeNull();
                        }
                    }
                }

                [Fact]
                public async Task UsesTheSetDatabase()
                {
                    const string database = "neo4jclient";

                    var initTransactionRequest = MockRequest.PostJson($"/db/{database}/tx", "{'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
                    var rollbackTransactionRequest = MockRequest.Delete($"/db/{database}/tx/1");
                    var commitTransactionRequest = MockRequest.PostJson($"/db/{database}/tx/1/commit", EmptyStatements);
                    var keepAliveTransactionRequest = MockRequest.PostJson($"/db/{database}/tx/1", EmptyStatements);
                    using (var testHarness = new RestTestHarness(false, "http://foo:7474")
                    {
                        {
                            initTransactionRequest,
                            MockResponse.Json(201, TransactionRestResponseHelper.GenerateInitTransactionResponse(1), $"http://foo:7474/db/{database}/tx/1")
                        },
                        {commitTransactionRequest, EmptyOkResponse},
                        {rollbackTransactionRequest, EmptyOkResponse},
                        {keepAliveTransactionRequest, EmptyOkResponse}
                    })
                    {
                        var client = await testHarness.CreateAndConnectTransactionalGraphClient(RestTestHarness.Neo4jVersion.Neo40);
                        using (var transaction = client.BeginTransaction(TransactionScopeOption.Join, null, database))
                        {
                            // dummy query to generate request
                            await client.Cypher
                                .Match("n")
                                .Return(n => n.Count())
                                .ExecuteWithoutResultsAsync().ConfigureAwait(false);

                            await transaction.KeepAliveAsync();
                        }
                    }
                }

                [Fact]
                public async Task UsesTheDefaultDatabase_WhenNoneSet()
                {
                    const string database = "neo4j";

                    var initTransactionRequest = MockRequest.PostJson($"/db/{database}/tx", "{'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
                    var rollbackTransactionRequest = MockRequest.Delete($"/db/{database}/tx/1");
                    var commitTransactionRequest = MockRequest.PostJson($"/db/{database}/tx/1/commit", EmptyStatements);
                    var keepAliveTransactionRequest = MockRequest.PostJson($"/db/{database}/tx/1", EmptyStatements);
                    using (var testHarness = new RestTestHarness(false, "http://foo:7474")
                    {
                        {
                            initTransactionRequest,
                            MockResponse.Json(201, TransactionRestResponseHelper.GenerateInitTransactionResponse(1), $"http://foo:7474/db/{database}/tx/1")
                        },
                        {commitTransactionRequest, EmptyOkResponse},
                        {rollbackTransactionRequest, EmptyOkResponse},
                        {keepAliveTransactionRequest, EmptyOkResponse}
                    })
                    {
                        var client = await testHarness.CreateAndConnectTransactionalGraphClient(RestTestHarness.Neo4jVersion.Neo40);
                        using (var transaction = client.BeginTransaction())
                        {
                            // dummy query to generate request
                            await client.Cypher
                                .Match("n")
                                .Return(n => n.Count())
                                .ExecuteWithoutResultsAsync().ConfigureAwait(false);

                            await transaction.KeepAliveAsync();
                        }
                    }
                }
            }

            public class DisposeMethod : IClassFixture<CultureInfoSetupFixture>
            {
                [Fact]
                public async Task CallsRollsback_IfTransactionIsOpen()
                {
                    const string database = "neo4j";

                    var initTransactionRequest = MockRequest.PostJson($"/db/{database}/tx", "{'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
                    var commitTransactionRequest = MockRequest.PostJson($"/db/{database}/tx/1/commit", EmptyStatements);
                    var rollbackTransactionRequest = MockRequest.Delete($"/db/{database}/tx/1");
                    using (var testHarness = new RestTestHarness(false, "http://foo:7474")
                    {
                        {
                            initTransactionRequest,
                            MockResponse.Json(201, TransactionRestResponseHelper.GenerateInitTransactionResponse(1), $"http://foo:7474/db/{database}/tx/1")
                        },
                        {commitTransactionRequest, EmptyOkResponse},
                        {rollbackTransactionRequest, EmptyOkResponse}
                    })
                    {
                        var client = await testHarness.CreateAndConnectTransactionalGraphClient(RestTestHarness.Neo4jVersion.Neo40);
                        using (var transaction = client.BeginTransaction())
                        {
                            // dummy query to generate request
                            await client.Cypher
                                .Match("n")
                                .Return(n => n.Count())
                                .ExecuteWithoutResultsAsync().ConfigureAwait(false);
                        }
                    }
                }

                [Fact]
                public async Task DoesntCallRollback_IfTransactionIsntOpen()
                {
                    const string database = "neo4j";

                    var initTransactionRequest = MockRequest.PostJson($"/db/{database}/tx", "{'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
                    var commitTransactionRequest = MockRequest.PostJson($"/db/{database}/tx/1/commit", EmptyStatements);

                    using (var testHarness = new RestTestHarness(false, "http://foo:7474")
                    {
                        {
                            initTransactionRequest,
                            MockResponse.Json(201, TransactionRestResponseHelper.GenerateInitTransactionResponse(1), $"http://foo:7474/db/{database}/tx/1")
                        },
                        {commitTransactionRequest, EmptyOkResponse}
                    })
                    {
                        var client = await testHarness.CreateAndConnectTransactionalGraphClient(RestTestHarness.Neo4jVersion.Neo40);
                        using (var transaction = client.BeginTransaction())
                        {
                            // dummy query to generate request
                            await client.Cypher
                                .Match("n")
                                .Return(n => n.Count())
                                .ExecuteWithoutResultsAsync().ConfigureAwait(false);
                            await transaction.CommitAsync();
                        }
                    }
                }
            }

            public class RollbackAsyncMethod : IClassFixture<CultureInfoSetupFixture>
            {
                [Fact]
                public async Task ThrowsClosedTransactionException_WhenTransactionAlreadyClosed()
                {
                    const string database = "neo4jclient";

                    var initTransactionRequest = MockRequest.PostJson($"/db/{database}/tx", "{'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
                    var rollbackTransactionRequest = MockRequest.Delete($"/db/{database}/tx/1");
                    var commitTransactionRequest = MockRequest.PostJson($"/db/{database}/tx/1/commit", EmptyStatements);
                    using (var testHarness = new RestTestHarness(false, "http://foo:7474")
                    {
                        {
                            initTransactionRequest,
                            MockResponse.Json(201, TransactionRestResponseHelper.GenerateInitTransactionResponse(1), $"http://foo:7474/db/{database}/tx/1")
                        },
                        {commitTransactionRequest, EmptyOkResponse},
                        {rollbackTransactionRequest, EmptyOkResponse}
                    })
                    {
                        var client = await testHarness.CreateAndConnectTransactionalGraphClient(RestTestHarness.Neo4jVersion.Neo40);
                        using (var transaction = client.BeginTransaction(TransactionScopeOption.Join, null, database))
                        {
                            // dummy query to generate request
                            await client.Cypher
                                .Match("n")
                                .Return(n => n.Count())
                                .ExecuteWithoutResultsAsync().ConfigureAwait(false);

                            await transaction.CommitAsync().ConfigureAwait(false);
                            var ex = await Assert.ThrowsAsync<ClosedTransactionException>(async () => await transaction.RollbackAsync());
                            ex.Should().NotBeNull();
                        }
                    }
                }

                [Fact]
                public async Task UsesTheSetDatabase()
                {
                    const string database = "neo4jclient";

                    var initTransactionRequest = MockRequest.PostJson($"/db/{database}/tx", "{'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
                    var rollbackTransactionRequest = MockRequest.Delete($"/db/{database}/tx/1");
                    using (var testHarness = new RestTestHarness(true, "http://foo:7474")
                    {
                        {
                            initTransactionRequest,
                            MockResponse.Json(201, TransactionRestResponseHelper.GenerateInitTransactionResponse(1), $"http://foo:7474/db/{database}/tx/1")
                        },
                        {
                            rollbackTransactionRequest, EmptyOkResponse
                        }
                    })
                    {
                        var client = await testHarness.CreateAndConnectTransactionalGraphClient(RestTestHarness.Neo4jVersion.Neo40);
                        using (var transaction = client.BeginTransaction(TransactionScopeOption.Join, null, database))
                        {
                            // dummy query to generate request
                            await client.Cypher
                                .Match("n")
                                .Return(n => n.Count())
                                .ExecuteWithoutResultsAsync();

                            await transaction.RollbackAsync();
                        }
                    }
                }

                [Fact]
                public async Task UsesTheDefaultDatabase_WhenNoneSet()
                {
                    const string database = "neo4j";

                    var initTransactionRequest = MockRequest.PostJson($"/db/{database}/tx", "{'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
                    var rollbackTransactionRequest = MockRequest.Delete($"/db/{database}/tx/1");
                    using (var testHarness = new RestTestHarness(true, "http://foo:7474")
                    {
                        {
                            initTransactionRequest,
                            MockResponse.Json(201, TransactionRestResponseHelper.GenerateInitTransactionResponse(1), $"http://foo:7474/db/{database}/tx/1")
                        },
                        {
                            rollbackTransactionRequest, EmptyOkResponse
                        }
                    })
                    {
                        var client = await testHarness.CreateAndConnectTransactionalGraphClient(RestTestHarness.Neo4jVersion.Neo40);
                        using (var transaction = client.BeginTransaction())
                        {
                            // dummy query to generate request
                            await client.Cypher
                                .Match("n")
                                .Return(n => n.Count())
                                .ExecuteWithoutResultsAsync();

                            await transaction.RollbackAsync();
                        }
                    }
                }
            }

            public class IsOpenProperty : IClassFixture<CultureInfoSetupFixture>
            {
                [Fact]
                public async Task IsTrueWhen_Open()
                {
                    const string database = "neo4j";

                    var initTransactionRequest = MockRequest.PostJson($"/db/{database}/tx", "{'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
                    var commitTransactionRequest = MockRequest.PostJson($"/db/{database}/tx/1/commit", EmptyStatements);
                    var rollbackTransactionRequest = MockRequest.Delete($"/db/{database}/tx/1");
                    using (var testHarness = new RestTestHarness(false, "http://foo:7474")
                    {
                        {
                            initTransactionRequest,
                            MockResponse.Json(201, TransactionRestResponseHelper.GenerateInitTransactionResponse(1), $"http://foo:7474/db/{database}/tx/1")
                        },
                        {commitTransactionRequest, EmptyOkResponse},
                        {rollbackTransactionRequest, EmptyOkResponse}
                    })
                    {
                        var client = await testHarness.CreateAndConnectTransactionalGraphClient(RestTestHarness.Neo4jVersion.Neo40);
                        using (var transaction = client.BeginTransaction())
                        {
                            // dummy query to generate request
                            await client.Cypher
                                .Match("n")
                                .Return(n => n.Count())
                                .ExecuteWithoutResultsAsync().ConfigureAwait(false);

                            transaction.IsOpen.Should().BeTrue();
                        }
                    }
                }

                [Fact]
                public async Task IsFalseWhen_Rolledback()
                {
                    const string database = "neo4j";

                    var initTransactionRequest = MockRequest.PostJson($"/db/{database}/tx", "{'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
                    var commitTransactionRequest = MockRequest.PostJson($"/db/{database}/tx/1/commit", EmptyStatements);
                    var rollbackTransactionRequest = MockRequest.Delete($"/db/{database}/tx/1");
                    using (var testHarness = new RestTestHarness(false, "http://foo:7474")
                    {
                        {
                            initTransactionRequest,
                            MockResponse.Json(201, TransactionRestResponseHelper.GenerateInitTransactionResponse(1), $"http://foo:7474/db/{database}/tx/1")
                        },
                        {commitTransactionRequest, EmptyOkResponse},
                        {rollbackTransactionRequest, EmptyOkResponse}
                    })
                    {
                        var client = await testHarness.CreateAndConnectTransactionalGraphClient(RestTestHarness.Neo4jVersion.Neo40);
                        using (var transaction = client.BeginTransaction())
                        {
                            // dummy query to generate request
                            await client.Cypher
                                .Match("n")
                                .Return(n => n.Count())
                                .ExecuteWithoutResultsAsync().ConfigureAwait(false);

                            await transaction.RollbackAsync();
                            transaction.IsOpen.Should().BeFalse();
                        }
                    }
                }

                [Fact]
                public async Task IsFalseWhen_Committed()
                {
                    const string database = "neo4j";

                    var initTransactionRequest = MockRequest.PostJson($"/db/{database}/tx", "{'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
                    var commitTransactionRequest = MockRequest.PostJson($"/db/{database}/tx/1/commit", EmptyStatements);
                    using (var testHarness = new RestTestHarness(false, "http://foo:7474")
                    {
                        {
                            initTransactionRequest,
                            MockResponse.Json(201, TransactionRestResponseHelper.GenerateInitTransactionResponse(1), $"http://foo:7474/db/{database}/tx/1")
                        },
                        {commitTransactionRequest, EmptyOkResponse}
                    })
                    {
                        var client = await testHarness.CreateAndConnectTransactionalGraphClient(RestTestHarness.Neo4jVersion.Neo40);
                        using (var transaction = client.BeginTransaction())
                        {
                            // dummy query to generate request
                            await client.Cypher
                                .Match("n")
                                .Return(n => n.Count())
                                .ExecuteWithoutResultsAsync().ConfigureAwait(false);

                            await transaction.CommitAsync();
                            transaction.IsOpen.Should().BeFalse();
                        }
                    }
                }
            }

            public class CommitAsyncMethod : IClassFixture<CultureInfoSetupFixture>
            {
                [Fact]
                public async Task ThrowsClosedTransactionException_WhenTransactionAlreadyClosed()
                {
                    const string database = "neo4jclient";

                    var initTransactionRequest = MockRequest.PostJson($"/db/{database}/tx", "{'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
                    var commitTransactionRequest = MockRequest.PostJson($"/db/{database}/tx/1/commit", EmptyStatements);
                    using (var testHarness = new RestTestHarness(false, "http://foo:7474")
                    {
                        {
                            initTransactionRequest,
                            MockResponse.Json(201, TransactionRestResponseHelper.GenerateInitTransactionResponse(1), $"http://foo:7474/db/{database}/tx/1")
                        },
                        {commitTransactionRequest, EmptyOkResponse}
                    })
                    {
                        var client = await testHarness.CreateAndConnectTransactionalGraphClient(RestTestHarness.Neo4jVersion.Neo40);
                        using (var transaction = client.BeginTransaction(TransactionScopeOption.Join, null, database))
                        {
                            // dummy query to generate request
                            await client.Cypher
                                .Match("n")
                                .Return(n => n.Count())
                                .ExecuteWithoutResultsAsync().ConfigureAwait(false);

                            await transaction.CommitAsync().ConfigureAwait(false);
                            var ex = await Assert.ThrowsAsync<ClosedTransactionException>(async () => await transaction.CommitAsync());
                            ex.Should().NotBeNull();
                        }
                    }
                }

                [Fact]
                public async Task UsesTheSetDatabase()
                {
                    const string database = "neo4jclient";

                    var initTransactionRequest = MockRequest.PostJson($"/db/{database}/tx", "{'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
                    var commitTransactionRequest = MockRequest.PostJson($"/db/{database}/tx/1/commit", EmptyStatements);
                    using (var testHarness = new RestTestHarness(false, "http://foo:7474")
                    {
                        {
                            initTransactionRequest,
                            MockResponse.Json(201, TransactionRestResponseHelper.GenerateInitTransactionResponse(1), $"http://foo:7474/db/{database}/tx/1")
                        },
                        {commitTransactionRequest, EmptyOkResponse}
                    })
                    {
                        var client = await testHarness.CreateAndConnectTransactionalGraphClient(RestTestHarness.Neo4jVersion.Neo40);
                        using (var transaction = client.BeginTransaction(TransactionScopeOption.Join, null, database))
                        {
                            // dummy query to generate request
                            await client.Cypher
                                .Match("n")
                                .Return(n => n.Count())
                                .ExecuteWithoutResultsAsync().ConfigureAwait(false);

                            await transaction.CommitAsync().ConfigureAwait(false);
                        }
                    }
                }

                [Fact]
                public async Task UsesTheDefaultDatabase_WhenNoneSet()
                {
                    const string database = "neo4j";

                    var initTransactionRequest = MockRequest.PostJson($"/db/{database}/tx", "{'statements': [{'statement': 'MATCH n\r\nRETURN count(n)', 'resultDataContents':[], 'parameters': {}}]}");
                    var commitTransactionRequest = MockRequest.PostJson($"/db/{database}/tx/1/commit", EmptyStatements);
                    using (var testHarness = new RestTestHarness(false, "http://foo:7474")
                    {
                        {
                            initTransactionRequest,
                            MockResponse.Json(201, TransactionRestResponseHelper.GenerateInitTransactionResponse(1), $"http://foo:7474/db/{database}/tx/1")
                        },
                        {commitTransactionRequest, EmptyOkResponse}
                    })
                    {
                        var client = await testHarness.CreateAndConnectTransactionalGraphClient(RestTestHarness.Neo4jVersion.Neo40);
                        using (var transaction = client.BeginTransaction())
                        {
                            // dummy query to generate request
                            await client.Cypher
                                .Match("n")
                                .Return(n => n.Count())
                                .ExecuteWithoutResultsAsync().ConfigureAwait(false);

                            await transaction.CommitAsync().ConfigureAwait(false);
                            var ex = await Assert.ThrowsAsync<ClosedTransactionException>(async () => await transaction.CommitAsync());
                            ex.Should().NotBeNull();
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Transaction tests against the Bolt API
        /// </summary>
        public class Bolt : IClassFixture<CultureInfoSetupFixture>
        {
            [Fact]
            public async Task QueryInTransaction_ThrowsExceptionWhen_ExecutingCypherQueryAgainstADifferentDatabaseName()
            {
                BoltClientTestHelper.GetDriverAndSession(out var mockDriver, out _, out var mockTransaction);

                var client = new BoltGraphClient(mockDriver);
                await client.ConnectAsync();
                using (var tx = client.BeginTransaction())
                {
                    var ex = Assert.Throws<InvalidOperationException>(() => client.Cypher.WithDatabase("foo").Match("n").Return(n => n.Count()));
                    ex.Should().NotBeNull();
                }
            }

            public class KeepAliveAsyncMethod : IClassFixture<CultureInfoSetupFixture>
            {
                [Fact]
                public async Task DoesntDoAnything()
                {
                    BoltClientTestHelper.GetDriverAndSession(out var mockDriver, out _, out _);

                    var client = new BoltGraphClient(mockDriver);
                    await client.ConnectAsync();
                    using (var tx = client.BeginTransaction())
                    {
                        await tx.KeepAliveAsync();
                    }
                }
            }

            public class DisposeMethod : IClassFixture<CultureInfoSetupFixture>
            {
                [Fact]
                public async Task CallsRollsback_IfTransactionIsOpen()
                {
                    BoltClientTestHelper.GetDriverAndSession(out var mockDriver, out _, out var mockTransaction);

                    var client = new BoltGraphClient(mockDriver);
                    await client.ConnectAsync();
                    using (var tx = client.BeginTransaction())
                    {
                        await client.Cypher.Match("n").Return(n => n.Count()).ExecuteWithoutResultsAsync();
                    }

                    await mockTransaction.Received(1).RollbackAsync();
                }

                [Fact]
                public async Task DoesntCallRollback_IfTransactionIsntOpen()
                {
                    BoltClientTestHelper.GetDriverAndSession(out var mockDriver, out _, out var mockTransaction);

                    var client = new BoltGraphClient(mockDriver);
                    await client.ConnectAsync();
                    using (var tx = client.BeginTransaction())
                    {
                        await client.Cypher.Match("n").Return(n => n.Count()).ExecuteWithoutResultsAsync();
                        await tx.CommitAsync();
                    }

                    await mockTransaction.Received(0).RollbackAsync();
                }
            }

            public class RollbackAsyncMethod : IClassFixture<CultureInfoSetupFixture>
            {
                [Fact]
                public async Task ThrowsClosedTransactionException_WhenTransactionAlreadyClosed()
                {
                    BoltClientTestHelper.GetDriverAndSession(out var mockDriver, out _, out var mockTransaction);

                    var client = new BoltGraphClient(mockDriver);
                    await client.ConnectAsync();
                    using (var tx = client.BeginTransaction())
                    {
                        await client.Cypher.Match("n").Return(n => n.Count()).ExecuteWithoutResultsAsync();
                        await tx.CommitAsync();
                        var ex = await Assert.ThrowsAsync<ClosedTransactionException>(async () => await tx.RollbackAsync());
                        ex.Should().NotBeNull();
                    }
                }

                [Fact(Skip = "I can't figure a way to check that the session is created correctly.")]
                public async Task UsesTheSetDatabase()
                {
                    //I need a proper async session.... not a fake one.


                    // BoltClientTestHelper.GetDriverAndSession(out IDriver mockDriver, out IAsyncSession mockSession, out IAsyncTransaction mockTransaction);
                    //
                    // const string database = "foo";
                    //
                    // var client = new BoltGraphClient(mockDriver);
                    // await client.ConnectAsync();
                    // mockDriver.ClearReceivedCalls();
                    // using (var tx = client.BeginTransaction(TransactionScopeOption.Join, null, database))
                    // {
                    //     await client.Cypher.Match("n").Return(n => n.Count()).ExecuteWithoutResultsAsync();
                    // }

                    // mockDriver.Received(1).AsyncSession(Arg.Is<Action<SessionConfigBuilder>>(x => CheckSessionConfig(x)));
                }

                [Fact(Skip = "I can't figure a way to check that the session is created correctly.")]
                public async Task UsesTheSetDatabase2()
                {
                    //     //I need a proper async session.... not a fake one.
                    //     const string database = "foo";
                    //
                    //     var realDriver = GraphDatabase.Driver("neo4j://foo:7687");
                    //     BoltClientTestHelper.GetDriverAndSession(out IDriver mockDriver, out IAsyncSession mockSession, out IAsyncTransaction mockTransaction);
                    //
                    //     var client = new BoltGraphClient(mockDriver);
                    //     await client.ConnectAsync();
                    //     
                    //     mockDriver.AsyncSession(Arg.Any<Action<SessionConfigBuilder>>()).ClearSubstitute();
                    //     mockDriver.AsyncSession(Arg.Any<Action<SessionConfigBuilder>>()).Returns(x => realDriver.AsyncSession(x.Arg<Action<SessionConfigBuilder>>()));
                    //
                    //
                    //     try
                    //     {
                    //         client.BeginTransaction(TransactionScopeOption.Join, null, database);
                    //     }
                    //     catch(AggregateException)
                    //     {
                    //         /**/
                    //     }
                    //
                    //     
                    //
                    //     //mockDriver.ClearReceivedCalls();
                    //     using (var tx = client.BeginTransaction(TransactionScopeOption.Join, null, database))
                    //     {
                    //         
                    //         await client.Cypher.Match("n").Return(n => n.Count()).ExecuteWithoutResultsAsync();
                    //     }
                    //
                    //     mockDriver.Received(1).AsyncSession(Arg.Is<Action<SessionConfigBuilder>>(x => CheckSessionConfig(x)));
                }
                //
                //
                // private static bool CheckSessionConfig(Action<SessionConfigBuilder> action)
                // {
                //     
                //     return true;
                // }


                [Fact(Skip = "I can't figure a way to check that the session is created correctly.")]
                public async Task UsesTheDefaultDatabase_WhenNoneSet()
                {
                    throw new NotImplementedException();
                }
            }

            public class IsOpenProperty : IClassFixture<CultureInfoSetupFixture>
            {
                [Fact]
                public async Task IsTrueWhen_Open()
                {
                    BoltClientTestHelper.GetDriverAndSession(out var mockDriver, out _, out _);

                    var client = new BoltGraphClient(mockDriver);
                    await client.ConnectAsync();
                    using (var tx = client.BeginTransaction())
                    {
                        await client.Cypher.Match("n").Return(n => n.Count()).ExecuteWithoutResultsAsync();
                        tx.IsOpen.Should().BeTrue();
                    }
                }

                [Fact]
                public async Task IsFalseWhen_Rolledback()
                {
                    BoltClientTestHelper.GetDriverAndSession(out var mockDriver, out _, out var mockTransaction);

                    var client = new BoltGraphClient(mockDriver);
                    await client.ConnectAsync();
                    using (var tx = client.BeginTransaction())
                    {
                        await client.Cypher.Match("n").Return(n => n.Count()).ExecuteWithoutResultsAsync();
                        await tx.RollbackAsync();
                        tx.IsOpen.Should().BeFalse();
                    }
                }

                [Fact]
                public async Task IsFalseWhen_Committed()
                {
                    BoltClientTestHelper.GetDriverAndSession(out var mockDriver, out _, out var mockTransaction);

                    var client = new BoltGraphClient(mockDriver);
                    await client.ConnectAsync();
                    using (var tx = client.BeginTransaction())
                    {
                        await client.Cypher.Match("n").Return(n => n.Count()).ExecuteWithoutResultsAsync();
                        await tx.CommitAsync();
                        tx.IsOpen.Should().BeFalse();
                    }
                }
            }

            public class CommitAsyncMethod : IClassFixture<CultureInfoSetupFixture>
            {
                [Fact]
                public async Task ThrowsClosedTransactionException_WhenTransactionAlreadyClosed()
                {
                    BoltClientTestHelper.GetDriverAndSession(out var mockDriver, out _, out var mockTransaction);

                    var client = new BoltGraphClient(mockDriver);
                    await client.ConnectAsync();
                    using (var tx = client.BeginTransaction())
                    {
                        await client.Cypher.Match("n").Return(n => n.Count()).ExecuteWithoutResultsAsync();
                        await tx.CommitAsync();
                        var ex = await Assert.ThrowsAsync<ClosedTransactionException>(async () => await tx.CommitAsync());
                        ex.Should().NotBeNull();
                    }
                }

                [Fact(Skip = "I can't figure a way to check that the session is created correctly.")]
                public async Task UsesTheSetDatabase()
                {
                    throw new NotImplementedException();
                }

                [Fact(Skip = "I can't figure a way to check that the session is created correctly.")]
                public async Task UsesTheDefaultDatabase_WhenNoneSet()
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}