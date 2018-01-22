using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Neo4j.Driver.V1;
using Neo4jClient.Test.Fixtures;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Test.BoltGraphClientTests
{
    internal static class DriverTestHelper
    {
        public static Mock<IDriver> MockDriverWithConnectionSet()
        {
            const string uri = "bolt://localhost";
            var recordMock = new Mock<IRecord>();
            recordMock.Setup(r => r["name"]).Returns("neo4j kernel");
            recordMock.Setup(r => r["versions"]).Returns(new List<object> { "3.2.3" });

            var testSr = new TestStatementResult(new[] { recordMock.Object });
            var sessionMock = new Mock<ISession>();
            sessionMock
                .Setup(s => s.Run("CALL dbms.components()"))
                .Returns(testSr);

            var driverMock = new Mock<IDriver>();
            driverMock.Setup(d => d.Session(It.IsAny<AccessMode>())).Returns(sessionMock.Object);
            driverMock.Setup(d => d.Session()).Returns(sessionMock.Object);
            driverMock.Setup(d => d.Uri).Returns(new Uri(uri));
            return driverMock;
        }
    }

    internal class TestStatementResult : IStatementResult
    {
        private readonly IList<IRecord> records;

        public TestStatementResult(IEnumerable<string> keys, params IRecord[] records)
        {
            this.Keys = new List<string>(keys);
            this.records = new List<IRecord>(records);
        }

        public TestStatementResult(IEnumerable<IRecord> records)
        {
            this.records = new List<IRecord>(records);
        }

        public IEnumerator<IRecord> GetEnumerator()
        {
            return records.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public IRecord Peek()
        {
            throw new NotImplementedException();
        }

        public IResultSummary Consume()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<string> Keys { get; }
        public IResultSummary Summary { get; }
    }

    public class ConnectAsyncTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public async Task SkipsOverExtraFieldsInDbmsComponents()
        {
            const string uri = "bolt://localhost";

            var record1Mock = new Mock<IRecord>();
            record1Mock.Setup(r => r["name"]).Returns("another-value");

            var record2Mock = new Mock<IRecord>();
            record2Mock.Setup(r => r["name"]).Returns("neo4j kernel");
            record2Mock.Setup(r => r["versions"]).Returns(new List<object> { "3.2.3" });

            var testSr = new TestStatementResult(new[] { record1Mock.Object, record2Mock.Object });
            var sessionMock = new Mock<ISession>();
            sessionMock
                .Setup(s => s.Run("CALL dbms.components()"))
                .Returns(testSr);

            var driverMock = new Mock<IDriver>();
            driverMock.Setup(d => d.Session(It.IsAny<AccessMode>())).Returns(sessionMock.Object);
            driverMock.Setup(d => d.Session()).Returns(sessionMock.Object);
            driverMock.Setup(d => d.Uri).Returns(new Uri(uri));

            var bgc = new BoltGraphClient(driverMock.Object);
            await bgc.ConnectAsync();
            bgc.ServerVersion.Should().Be(new Version(3, 2, 3));
        }



        [Fact]
        public async Task SetsTheVersionOfTheServer()
        {
            var driverMock = DriverTestHelper.MockDriverWithConnectionSet();

            var bgc = new BoltGraphClient(driverMock.Object);
            await bgc.ConnectAsync();
            bgc.ServerVersion.Should().Be(new Version(3, 2, 3));
        }

        [Fact]
        public async Task SetIsConnected()
        {
            var driverMock = DriverTestHelper.MockDriverWithConnectionSet();

            var bgc = new BoltGraphClient(driverMock.Object);
            await bgc.ConnectAsync();
            bgc.IsConnected.Should().BeTrue();
        }
    }
}