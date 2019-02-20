using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Moq;
using Neo4j.Driver.V1;
using Neo4jClient.Test.Extensions;
using Neo4jClient.Test.Fixtures;
using Newtonsoft.Json;
using Xunit;

namespace Neo4jClient.Tests.Shared.Serialization
{
    public class SerializesDateTimeAsNeoDateTimeTests : IClassFixture<CultureInfoSetupFixture>
    {
        private class ClassWithNeo4jDateTime
        {
            [Neo4jDateTime]
            public DateTime Dt { get; set; }
        }

        [Fact]
        public void SerializesDateTimeAsNeoDateInBolt()
        {
            var mockSession = new Mock<ISession>();
            mockSession.Setup(s => s.Run("CALL dbms.components()")).Returns(new BoltGraphClientTests.ServerInfo());
            var dt = new DateTime(2000, 1, 1, 0, 0, 0);

            var mockDriver = new Mock<IDriver>();
            mockDriver.Setup(d => d.Session(It.IsAny<AccessMode>())).Returns(mockSession.Object);
            mockDriver.Setup(d => d.Session(It.IsAny<AccessMode>(), It.IsAny<IEnumerable<string>>())).Returns(mockSession.Object);
            mockDriver.Setup(d => d.Uri).Returns(new Uri("bolt://localhost"));

            var bgc = new BoltGraphClient(mockDriver.Object);
            bgc.Connect();

            var cwd = new ClassWithNeo4jDateTime { Dt = dt }; ;

            var cfq = bgc.Cypher.Create("(c)").WithParam("testParam", cwd);

            var expectedParameters = new Dictionary<string, object>
            {
                {
                    "testParam", new Dictionary<string, object> {{"Dt", dt}}
                }
            };

            var query = cfq.Query;
            var parameters = query.ToNeo4jDriverParameters(bgc);
            parameters.IsEqualTo(expectedParameters).Should().BeTrue();
        }

    }
}
