using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Neo4j.Driver.V1;
using Neo4jClient.Tests.BoltGraphClientTests;
using Neo4jClient.Transactions;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.Transactions
{

    public class BoltQueriesInTransactionTests : IClassFixture<CultureInfoSetupFixture>
    {
        private class Foo { }
        #region Helper Methods

        private static IStatementResultCursor GetDbmsComponentsResponse()
        {
            var record = Substitute.For<IRecord>();
            record["name"].Returns("neo4j kernel");
            record["versions"].Returns(new List<object> {"3.1.0"});

            var response = new List<IRecord> {record};

            var statementResult = new TestStatementResult(response);
            return statementResult;
        }

        private static void GetDriverAndSession(out IDriver driver, out ISession session, out Neo4j.Driver.V1.ITransaction transaction)
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

            var mockTransaction = Substitute.For<Neo4j.Driver.V1.ITransaction>();
            mockTransaction.RunAsync(Arg.Any<string>(), Arg.Any<IDictionary<string, object>>()).Returns(mockStatementResult);

            var mockSession = Substitute.For<ISession>();
            var dbmsReturn = GetDbmsComponentsResponse();
            mockSession.RunAsync("CALL dbms.components()").Returns(dbmsReturn);
            mockSession.BeginTransaction().Returns(mockTransaction);
            
            var mockDriver = Substitute.For<IDriver>();
            mockDriver.Session().Returns(mockSession);
            mockDriver.Session(Arg.Any<AccessMode>()).Returns(mockSession);
            mockDriver.Session(Arg.Any<AccessMode>(), Arg.Any<IEnumerable<string>>()).Returns(mockSession);
            mockDriver.Session(Arg.Any<IEnumerable<string>>()).Returns(mockSession);
            mockDriver.Uri.Returns(new Uri("bolt://localhost"));

            driver = mockDriver;
            session = mockSession;
            transaction = mockTransaction;
        }

        private static void GetAndConnectGraphClient(out IGraphClient graphClient, out IDriver driver, out ISession session, out Neo4j.Driver.V1.ITransaction transaction)
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
            public async Task SimpleTransaction_AsTransactionalGc_1Query()
            {
                ISession session;
                IDriver driver;
                Neo4j.Driver.V1.ITransaction transaction;
                IGraphClient graphClient;

                GetAndConnectGraphClient(out graphClient, out driver, out session, out transaction);

                ITransactionalGraphClient txGc = (ITransactionalGraphClient) graphClient;
                using (var tx = txGc.BeginTransaction())
                {
                    await txGc.Cypher.Match("(n)").Set("n.Value = 'test'").ExecuteWithoutResultsAsync();
                    await tx.CommitAsync();
                }

                driver.Received(1).Session((IEnumerable<string>)null);
                session.Received(1).BeginTransaction();
                transaction.Received(1).CommitAsync();
            }

            private class MockNode
            {
                public string Name { get; set; }
            }

            [Fact]
            public async Task SimpleTransaction_RetrieveAndSerializeAnonymousResult()
            {
                ISession session;
                IDriver driver;
                Neo4j.Driver.V1.ITransaction transaction;
                IGraphClient graphClient;

                GetAndConnectGraphClient(out graphClient, out driver, out session, out transaction);

                ITransactionalGraphClient txGc = (ITransactionalGraphClient)graphClient;
                using (var tx = txGc.BeginTransaction())
                {
                    var node = (await txGc.Cypher.Match("(n:Node)").Return(n => new {Node = n.As<MockNode>()}).ResultsAsync).SingleOrDefault();

                    node.Node.Name.Should().Be("Value");

                    await tx.CommitAsync();
                }

                driver.Received(1).Session((IEnumerable<string>) null);
                session.Received(1).BeginTransaction();
                transaction.Received(1).CommitAsync();
            }
        }
    }
}

