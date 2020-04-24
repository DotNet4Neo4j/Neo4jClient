using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Neo4j.Driver;
using Neo4jClient.Tests.BoltGraphClientTests;
using Neo4jClient.Transactions;
using NSubstitute;
using NSubstitute.Core.Arguments;
using Xunit;

namespace Neo4jClient.Tests.Transactions
{

    public class BoltQueriesInTransactionTests : IClassFixture<CultureInfoSetupFixture>
    {
        private class Foo { }
        #region Helper Methods

        private static IResultCursor GetDbmsComponentsResponse()
        {
            var record = Substitute.For<IRecord>();
            record["name"].Returns("neo4j kernel");
            record["versions"].Returns(new List<object> {"3.1.0"});

            var response = new List<IRecord> {record};

            var statementResult = new TestStatementResult(response);
            return statementResult;
        }

        private static void GetDriverAndSession(out IDriver driver, out IAsyncSession session, out IAsyncTransaction transaction)
        {
            var mockNode = Substitute.For<INode>();
            mockNode["Name"].Returns("Value");
            mockNode.Labels.Returns(new List<string>() {"Node"});
            mockNode.Properties.Returns(new Dictionary<string, object>() {{"Name", "Value"}});

            var mockRecord = Substitute.For<IRecord>();
            mockRecord.Keys.Returns(new List<string>(){"Node"});
            mockRecord["Node"].Returns(mockNode);
            mockRecord.Values["Node"].Returns(mockNode);

            var mockStatementResult = new TestStatementResult(new List<IRecord>(new[] {mockRecord}));

            var mockTransaction = Substitute.For<IAsyncTransaction>();
            mockTransaction.RunAsync(Arg.Any<string>(), Arg.Any<IDictionary<string, object>>()).Returns(mockStatementResult);

            var task = Task.FromResult(mockTransaction);
            task.Result.Should().NotBe(null);

            var mockSession = Substitute.For<IAsyncSession>();
            var dbmsReturn = GetDbmsComponentsResponse();
            mockSession.RunAsync("CALL dbms.components()").Returns(dbmsReturn);
            mockSession.BeginTransactionAsync().Returns(Task.FromResult(mockTransaction));
            mockSession.BeginTransactionAsync(Arg.Any<Action<TransactionConfigBuilder>>()).Returns(Task.FromResult(mockTransaction));

            var mockDriver = Substitute.For<IDriver>();
            mockDriver.AsyncSession().Returns(mockSession);
            mockDriver.AsyncSession(Arg.Any<Action<SessionConfigBuilder>>()).Returns(mockSession);
            // mockDriver.Uri.Returns(new Uri("bolt://localhost"));

            driver = mockDriver;
            session = mockSession;
            transaction = mockTransaction;
        }

        private static void GetAndConnectGraphClient(out IGraphClient graphClient, out IDriver driver, out IAsyncSession session, out IAsyncTransaction transaction)
        {
            GetDriverAndSession(out driver, out session, out transaction);
            var client = new BoltGraphClient(driver);
            client.ConnectAsync().Wait();

            driver.ClearReceivedCalls();
            session.ClearReceivedCalls();
            graphClient = client;
        }

        #endregion Helper Methods

        public class TransactionGraphClientTests : IClassFixture<CultureInfoSetupFixture>
        {
            [Fact]
            public async Task SimpleTransaction_AsTransactionalGc_1Query_Moq()
            {
                using (var harness = new BoltTestHarness())
                {
                    var graphClient = await harness.CreateAndConnectBoltGraphClient();
                    
                    

                    ITransactionalGraphClient txGc = (ITransactionalGraphClient)graphClient;
                    using (var tx = txGc.BeginTransaction())
                    {
                        var query = txGc.Cypher.Match("(n)").Set("n.Value = 'test'");
                        await query.ExecuteWithoutResultsAsync();
                        await tx.CommitAsync();
                    }

                    harness.MockDriver.Verify(md => md.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>()), Times.Once);
                }
            }


            [Fact]
            public async Task SimpleTransaction_AsTransactionalGc_1Query()
            {
                GetAndConnectGraphClient(out var graphClient, out var driver, out var session, out var transaction);

                ITransactionalGraphClient txGc = (ITransactionalGraphClient) graphClient;
                using (var tx = txGc.BeginTransaction())
                {
                    await txGc.Cypher.Match("(n)").Set("n.Value = 'test'").ExecuteWithoutResultsAsync();
                    await tx.CommitAsync();
                }

                driver.Received(1).AsyncSession(Arg.Any<Action<SessionConfigBuilder>>());
                await session.Received(1).BeginTransactionAsync();
                await transaction.Received(1).CommitAsync();
            }

            private class MockNode
            {
                public string Name { get; set; }
            }

            [Fact]
            public async Task SimpleTransaction_RetrieveAndSerializeAnonymousResult()
            {
                GetAndConnectGraphClient(out var graphClient, out var driver, out var session, out var transaction);

                ITransactionalGraphClient txGc = (ITransactionalGraphClient)graphClient;
                using (var tx = txGc.BeginTransaction())
                {
                    var node = (await txGc.Cypher.Match("(n:Node)").Return(n => new {Node = n.As<MockNode>()}).ResultsAsync).SingleOrDefault();

                    node.Node.Name.Should().Be("Value");

                    await tx.CommitAsync();
                }

                driver.Received(1).AsyncSession();
                await session.Received(1).BeginTransactionAsync();
                await transaction.Received(1).CommitAsync();
            }
        }
    }
}

