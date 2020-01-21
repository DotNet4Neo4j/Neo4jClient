using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Neo4j.Driver;
using Neo4jClient.Transactions;

namespace Neo4jClient.Tests
{
    public class BoltTestHarness : IDisposable
    {
        public BoltTestHarness()
        {
            MockSession.Setup(s => s.RunAsync("CALL dbms.components()")).Returns(Task.FromResult<IResultCursor>(new BoltGraphClientTests.BoltGraphClientTests.ServerInfo()));
            MockDriver.Setup(d => d.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>())).Returns(MockSession.Object);
            // MockDriver.Setup(d => d.Uri).Returns(new Uri("bolt://localhost"));
            MockSession.Setup(s => s.RunAsync(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()))
                .Throws(new Exception("Should never use synchronous method"));
            MockSession.Setup(s => s.WriteTransactionAsync(It.IsAny<Func<IAsyncTransaction, Task>>()))
                .Throws(new Exception("Should never use synchronous method"));
            MockSession.Setup(s => s.ReadTransactionAsync(It.IsAny<Func<IAsyncTransaction, Task>>()))
                .Throws(new Exception("Should never use synchronous method"));
        }

        public Mock<IDriver> MockDriver { get; } = new Mock<IDriver>();
        public Mock<IAsyncSession> MockSession { get; } = new Mock<IAsyncSession>();

        public void Dispose()
        {
        }

        public void SetupCypherRequestResponse(string request, IDictionary<string, object> cypherQueryQueryParameters, IResultCursor response)
        {
            MockSession.Setup(s => s.RunAsync(request, It.IsAny<IDictionary<string, object>>())).Returns(Task.FromResult(response));
            MockSession.Setup(s => s.WriteTransactionAsync(It.IsAny<Func<IAsyncTransaction, Task<IResultCursor>>>())).Returns(Task.FromResult(response));
        }

        public async Task<IRawGraphClient> CreateAndConnectBoltGraphClient()
        {
            var bgc = new BoltGraphClient(MockDriver.Object);
            await bgc.ConnectAsync();
            return bgc;
        }
    }
}