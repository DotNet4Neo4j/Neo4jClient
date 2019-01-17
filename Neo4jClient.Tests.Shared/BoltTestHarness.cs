using System;
using System.Collections.Generic;
using Moq;
using Neo4j.Driver.V1;

namespace Neo4jClient.Test
{
    public class BoltTestHarness : IDisposable
    {
        public BoltTestHarness()
        {
            MockSession.Setup(s => s.Run("CALL dbms.components()")).Returns(new Extensions.BoltGraphClientTests.ServerInfo());
            MockDriver.Setup(d => d.Session(It.IsAny<AccessMode>())).Returns(MockSession.Object);
            MockDriver.Setup(d => d.Session(It.IsAny<AccessMode>(), It.IsAny<IEnumerable<string>>())).Returns(MockSession.Object);
            MockDriver.Setup(d => d.Session(It.IsAny<IEnumerable<string>>())).Returns(MockSession.Object);
            MockDriver.Setup(d => d.Uri).Returns(new Uri("bolt://localhost"));
        }

        public Mock<IDriver> MockDriver { get; } = new Mock<IDriver>();
        public Mock<ISession> MockSession { get; } = new Mock<ISession>();

        public void Dispose()
        {
        }

        public void SetupCypherRequestResponse(string request, IDictionary<string, object> cypherQueryQueryParameters, IStatementResult response)
        {
            MockSession.Setup(s => s.Run(request, It.IsAny<IDictionary<string, object>>())).Returns(response);
            MockSession.Setup(s => s.WriteTransaction(It.IsAny<Func<ITransaction, IStatementResult>>())).Returns(response);
        }

        public IRawGraphClient CreateAndConnectBoltGraphClient()
        {
            var bgc = new BoltGraphClient(MockDriver.Object);
            bgc.Connect();
            return bgc;
        }
    }
}