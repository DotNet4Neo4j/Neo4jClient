using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Neo4j.Driver;
using Neo4jClient.Tests.BoltGraphClientTests;
using NSubstitute;

namespace Neo4jClient.Tests.Transactions
{
    public static class BoltClientTestHelper
    {
        public static IResultCursor GetDbmsComponentsResponse()
        {
            var record = Substitute.For<IRecord>();
            record["name"].Returns("neo4j kernel");
            record["versions"].Returns(new List<object> { "3.1.0" });

            var response = new List<IRecord> { record };

            var statementResult = new TestStatementResult(response);
            return statementResult;
        }

        public static void GetDriverAndSession(out IDriver driver, out IAsyncSession session, out IAsyncTransaction transaction)
        {
            var mockNode = Substitute.For<INode>();
            mockNode["Name"].Returns("Value");
            mockNode.Labels.Returns(new List<string> { "Node" });
            mockNode.Properties.Returns(new Dictionary<string, object> { { "Name", "Value" } });

            var mockRecord = Substitute.For<IRecord>();
            mockRecord.Keys.Returns(new List<string> { "Node" });
            mockRecord["Node"].Returns(mockNode);
            mockRecord.Values["Node"].Returns(mockNode);

            var mockStatementResult = new TestStatementResult(new List<IRecord>(new[] { mockRecord }));

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


            driver = mockDriver;
            session = mockSession;
            transaction = mockTransaction;
        }

        public static void GetAndConnectGraphClient(out IGraphClient graphClient, out IDriver driver, out IAsyncSession session, out IAsyncTransaction transaction)
        {
            GetDriverAndSession(out driver, out session, out transaction);
            var client = new BoltGraphClient(driver);
            client.ConnectAsync().Wait();

            driver.ClearReceivedCalls();
            session.ClearReceivedCalls();
            graphClient = client;
        }
    }
}