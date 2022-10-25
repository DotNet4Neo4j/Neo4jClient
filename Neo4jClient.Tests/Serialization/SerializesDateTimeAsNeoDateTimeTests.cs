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

        private class ClassWithoutNeo4jDateTime
        {
            public DateTime Dt1 { get; set; }

            public DateTimeOffset Dt2 { get; set; }
        }

        [Fact]
        public async Task SerializesDateTimeAsNeoDateInBolt()
        {
            var mockSession = new Mock<IAsyncSession>();
            mockSession.Setup(s => s.RunAsync("CALL dbms.components()", null)).Returns(Task.FromResult<IResultCursor>(new BoltGraphClientTests.BoltGraphClientTests.ServerInfo()));
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

        [Fact]
        public async Task CannotUseNativeGlobalSettingWithOlderVersions()
        {
            var mockSession = new Mock<IAsyncSession>();

            // native datetime types were introduced in 3.4
            var oldVersion = new Version(3, 0, 0);

            // for ConnectAsync()
            mockSession.Setup(s => s.RunAsync("CALL dbms.components()", null)).Returns(Task.FromResult<IResultCursor>(new BoltGraphClientTests.BoltGraphClientTests.ServerInfo(oldVersion)));

            var mockDriver = new Mock<IDriver>();
            mockDriver.Setup(d => d.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>())).Returns(mockSession.Object);

            var bgc = new BoltGraphClient(mockDriver.Object, useDriverDateTypes: true);
            await Assert.ThrowsAsync<NotSupportedException>(async () => await bgc.ConnectAsync());
        }


        [Fact]
        public async Task SerializesDateTimeAsNeoDateWithGlobalSetting()
        {
            var mockSession = new Mock<IAsyncSession>();
            // for ConnectAsync()
            mockSession.Setup(s => s.RunAsync("CALL dbms.components()", null)).Returns(Task.FromResult<IResultCursor>(new BoltGraphClientTests.BoltGraphClientTests.ServerInfo()));
            
            var dt1 = new DateTime(2000, 1, 1, 0, 0, 0);
            var dt2 = new DateTimeOffset(dt1);

            var mockDriver = new Mock<IDriver>();
            mockDriver.Setup(d => d.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>())).Returns(mockSession.Object);


            var bgc = new BoltGraphClient(mockDriver.Object, useDriverDateTypes: true);
            await bgc.ConnectAsync();

            var cwd = new ClassWithoutNeo4jDateTime { Dt1 = dt1, Dt2 = dt2 }; ;

            var cfq = bgc.Cypher.Create("(c)").WithParam("testParam", cwd);

            var expectedParameters = new Dictionary<string, object>
            {
                {
                    "testParam", new Dictionary<string, object> {{"Dt1", dt1}, {"Dt2", dt2}}
                }
            };

            var query = cfq.Query;
            var parameters = query.ToNeo4jDriverParameters(bgc);
            parameters.IsEqualTo(expectedParameters).Should().BeTrue();
        }

        [Fact]
        public async Task SerializesDateTimeGeneratedParametersAsNeoDateWithGlobalSetting()
        {
            var mockSession = new Mock<IAsyncSession>();
            // for ConnectAsync()
            mockSession.Setup(s => s.RunAsync("CALL dbms.components()", null)).Returns(Task.FromResult<IResultCursor>(new BoltGraphClientTests.BoltGraphClientTests.ServerInfo()));

            var mockDriver = new Mock<IDriver>();
            mockDriver.Setup(d => d.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>())).Returns(mockSession.Object);


            var bgc = new BoltGraphClient(mockDriver.Object, useDriverDateTypes: true);
            await bgc.ConnectAsync();

            var comparedDt = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero);

            var cfq = bgc.Cypher.Match("(c)").Where<ClassWithoutNeo4jDateTime>(c => c.Dt2 >= comparedDt);
            var query = cfq.Query;
            var parameters = query.ToNeo4jDriverParameters(bgc);
            parameters.Should().HaveCount(1).And.ContainValue(comparedDt);
        }

    }
}
