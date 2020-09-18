using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Neo4j.Driver;
using Xunit;

namespace Neo4jClient.Tests.BoltGraphClientTests
{
    internal static class DriverTestHelper
    {
        public static Mock<IDriver> MockDriverWithConnectionSet()
        {
            const string uri = "bolt://localhost";
            var recordMock = new Mock<IRecord>();
            recordMock.Setup(r => r["name"]).Returns("neo4j kernel");
            recordMock.Setup(r => r["versions"]).Returns(new List<object> {"3.2.3"});

            var testSr = new TestStatementResult(new[] {recordMock.Object});
            var sessionMock = new Mock<IAsyncSession>();
            sessionMock
                .Setup(s => s.RunAsync("CALL dbms.components()"))
                .Returns(Task.FromResult<IResultCursor>(testSr));

            var driverMock = new Mock<IDriver>();
            driverMock.Setup(d => d.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>())).Returns(sessionMock.Object);
            driverMock.Setup(d => d.AsyncSession()).Returns(sessionMock.Object);
            //driverMock.Setup(d => d.Uri).Returns(new Uri(uri));
            return driverMock;
        }
    }

    public class TestStatementResult : IResultCursor
    {
        private readonly IList<IRecord> records;
        private int pos = -1;

        public TestStatementResult(IEnumerable<string> keys, params IRecord[] records)
        {
            this.Keys = new List<string>(keys);
            this.records = new List<IRecord>(records);
        }

        public TestStatementResult(IEnumerable<IRecord> records)
        {
            this.records = new List<IRecord>(records);
        }

        public Task<IResultSummary> SummaryAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IRecord> PeekAsync()
        {
            return Task.FromResult(Current);
        }

        public Task<string[]> KeysAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IResultSummary> ConsumeAsync()
        {
            var countersMock = new Mock<ICounters>(MockBehavior.Loose);
            var mock = new Mock<IResultSummary>();
            mock.Setup(x => x.Counters).Returns(countersMock.Object);
            return Task.FromResult(mock.Object);
        }

        public Task<bool> FetchAsync()
        {
            if (pos == records.Count - 1) return Task.FromResult(false);
            else
            {
                pos++;
                return Task.FromResult(true);
            }
        }

        public IReadOnlyList<string> Keys { get; }
        public IRecord Current => records[pos];
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
            record2Mock.Setup(r => r["versions"]).Returns(new List<object> {"3.2.3"});

            var testSr = new TestStatementResult(new[] {record1Mock.Object, record2Mock.Object});
            var sessionMock = new Mock<IAsyncSession>();
            sessionMock
                .Setup(s => s.RunAsync("CALL dbms.components()"))
                .Returns(Task.FromResult<IResultCursor>(testSr));

            var driverMock = new Mock<IDriver>();
            driverMock.Setup(d => d.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>())).Returns(sessionMock.Object);
            

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