using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Neo4j.Driver;
using Xunit;

namespace Neo4jClient.Tests.Serialization
{
    public class SerializesDateTimeAsNeoDateTimeTests : IClassFixture<CultureInfoSetupFixture>
    {
        private class ClassWithNeo4jDateTime
        {
            [Neo4jDateTime]
            public DateTime Dt { get; set; }
        }

        [Fact]
        public async Task SerializesDateTimeAsNeoDateInBolt()
        {
            var mockSession = new Mock<IAsyncSession>();
            mockSession.Setup(s => s.RunAsync("CALL dbms.components()")).Returns(Task.FromResult<IResultCursor>(new BoltGraphClientTests.BoltGraphClientTests.ServerInfo()));
            var dt = new DateTime(2000, 1, 1, 0, 0, 0);

            var mockDriver = new Mock<IDriver>();
            mockDriver.Setup(d => d.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>())).Returns(mockSession.Object);
            
            // mockDriver.Setup(d => d.Uri).Returns(new Uri("bolt://localhost"));

            var bgc = new BoltGraphClient(mockDriver.Object);
            await bgc.ConnectAsync();

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
