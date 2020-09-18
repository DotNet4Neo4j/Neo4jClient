using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Neo4j.Driver;

namespace Neo4jClient.Tests
{
    public class BoltTestHarness : IDisposable
    {
        public BoltTestHarness()
        {
            var mockResultCursor = new Mock<IResultCursor>();
            
            var mockTransaction = new Mock<IAsyncTransaction>();
            mockTransaction
                .Setup(t => t.RunAsync(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()))
                .Returns(Task.FromResult(mockResultCursor.Object));

            var mockSession = new Mock<IAsyncSession>(MockBehavior.Loose);
            mockSession
                .Setup(s => s.RunAsync("CALL dbms.components()"))
                .Returns(Task.FromResult<IResultCursor>(new BoltGraphClientTests.BoltGraphClientTests.ServerInfo()));
            mockSession
                .Setup(s => s.RunAsync(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()))
                .Throws(new Exception("Should never use synchronous method"));
            mockSession
                .Setup(s => s.WriteTransactionAsync(It.IsAny<Func<IAsyncTransaction, Task>>()))
                .Throws(new Exception("Should never use synchronous method"));
            mockSession
                .Setup(s => s.ReadTransactionAsync(It.IsAny<Func<IAsyncTransaction, Task>>()))
                .Throws(new Exception("Should never use synchronous method"));
            mockSession
                .Setup(s => s.BeginTransactionAsync(It.IsAny<Action<TransactionConfigBuilder>>()))
                .Returns(Task.FromResult(mockTransaction.Object));
            mockSession
                .Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(mockTransaction.Object));

            var mockDriver = new Mock<IDriver>(MockBehavior.Strict);
            mockDriver
                .Setup(d => d.AsyncSession())
                .Returns(mockSession.Object);
            mockDriver
                .Setup(d => d.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>()))
                .Returns(mockSession.Object);

            MockSession = mockSession;
            MockDriver = mockDriver;
        }


        public Mock<IDriver> MockDriver { get; }
        public Mock<IAsyncSession> MockSession { get; }

        public void Dispose()
        {
        }

        public void SetupCypherRequestResponse(string request, IDictionary<string, object> cypherQueryQueryParameters, IResultCursor response)
        {
            MockSession.Setup(s => s.RunAsync(request, It.IsAny<IDictionary<string, object>>())).Returns(Task.FromResult(response));
            var mockTransaction = new Mock<IAsyncTransaction>();
            mockTransaction.Setup(s => s.RunAsync(request, It.IsAny<IDictionary<string, object>>())).Returns(Task.FromResult(response));
            
            MockSession.Setup(s => s.WriteTransactionAsync(It.IsAny<Func<IAsyncTransaction, Task<List<IRecord>>>>()))
                .Returns<Func<IAsyncTransaction, Task<List<IRecord>>>>(async param => await param(mockTransaction.Object));

            MockSession.Setup(s => s.ReadTransactionAsync(It.IsAny<Func<IAsyncTransaction, Task<List<IRecord>>>>()))
                .Returns<Func<IAsyncTransaction, Task<List<IRecord>>>>(async param => await param(mockTransaction.Object));

            MockSession
                .Setup(s => s.BeginTransactionAsync(It.IsAny<Action<TransactionConfigBuilder>>()))
                .Returns(Task.FromResult(mockTransaction.Object));
            MockSession
                .Setup(s => s.BeginTransactionAsync())
                .Returns(Task.FromResult(mockTransaction.Object));
        }
        public async Task<IRawGraphClient> CreateAndConnectBoltGraphClient()
        {
            var bgc = new BoltGraphClient(MockDriver.Object);
            await bgc.ConnectAsync();
            MockDriver.Invocations.Clear();
            return bgc;
        }
    }
}