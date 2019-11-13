using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Neo4j.Driver.V1;

namespace Neo4jClient.Test
{
    public class BoltTestHarness : IDisposable
    {
        public BoltTestHarness()
        {
            MockSession.Setup(s => s.RunAsync("CALL dbms.components()")).Returns(Task.FromResult<IStatementResultCursor>(new Extensions.BoltGraphClientTests.ServerInfo()));
            MockDriver.Setup(d => d.Session(It.IsAny<AccessMode>())).Returns(MockSession.Object);
            MockDriver.Setup(d => d.Session(It.IsAny<AccessMode>(), It.IsAny<IEnumerable<string>>())).Returns(MockSession.Object);
            MockDriver.Setup(d => d.Session(It.IsAny<IEnumerable<string>>())).Returns(MockSession.Object);
            MockDriver.Setup(d => d.Uri).Returns(new Uri("bolt://localhost"));
            MockSession.Setup(s => s.Run(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()))
                .Throws(new Exception("Should never use synchronous method"));
            MockSession.Setup(s => s.WriteTransaction(It.IsAny<Func<ITransaction, IStatementResult>>()))
                .Throws(new Exception("Should never use synchronous method"));
            MockSession.Setup(s => s.ReadTransaction(It.IsAny<Func<ITransaction, IStatementResult>>()))
                .Throws(new Exception("Should never use synchronous method"));
        }

        public Mock<IDriver> MockDriver { get; } = new Mock<IDriver>();
        public Mock<ISession> MockSession { get; } = new Mock<ISession>();

        public void Dispose()
        {
        }

        public void SetupCypherRequestResponse(string request, IDictionary<string, object> cypherQueryQueryParameters, IStatementResultCursor response)
        {
            MockSession.Setup(s => s.RunAsync(request, It.IsAny<IDictionary<string, object>>())).Returns(Task.FromResult(response));
            MockSession.Setup(s => s.WriteTransactionAsync(It.IsAny<Func<ITransaction, Task<IStatementResultCursor>>>())).Returns(Task.FromResult(response));
        }

        public IRawGraphClient CreateAndConnectBoltGraphClient()
        {
            var bgc = new BoltGraphClient(MockDriver.Object);
            bgc.ConnectAsync().Wait();
            return bgc;
        }
    }
}