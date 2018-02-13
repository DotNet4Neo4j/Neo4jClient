using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Neo4j.Driver.V1;
using Neo4jClient.Test.BoltGraphClientTests;
using Neo4jClient.Test.Fixtures;
using Newtonsoft.Json;
using Xunit;

namespace Neo4jClient.Test.Extensions
{
    public class BoltGraphClientTests : IClassFixture<CultureInfoSetupFixture>
    {
        private class TestNode { }
        private class TestRecord : IRecord
        {
            private IDictionary<string, object> _contents = new Dictionary<string, object>();

            public TestRecord()
            {
                
            }

            public TestRecord(IDictionary<string, object> items)
            {
                _contents = items;
            }

            object IRecord.this[int index]
            {
                get { throw new NotImplementedException(); }
            }

            object IRecord.this[string key] => _contents[key];

            public IReadOnlyDictionary<string, object> Values { get; }
            public IReadOnlyList<string> Keys { get; }

           
        }
        public class ServerInfo : IStatementResult
        {
            private IList<IRecord> _list = new List<IRecord>();

            public IEnumerator<IRecord> GetEnumerator()
            {
                return _list.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public ServerInfo()
            {
                _list.Add(new TestRecord(new Dictionary<string, object>
                {
                    {"name", "neo4j kernel"},
                    {"versions", new List<object>{"3.2.3"} }
                }));
            }

            public ServerInfo(IList<IRecord> records)
            {
                _list = records;
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

        [Fact]
        public void SerializesDateTimesProperly()
        {
            var mockSession = new Mock<ISession>();
            mockSession.Setup(s => s.Run("CALL dbms.components()")).Returns(new ServerInfo());

            var mockDriver = new Mock<IDriver>();
            mockDriver.Setup(d => d.Session(It.IsAny<AccessMode>())).Returns(mockSession.Object);
            mockDriver.Setup(d => d.Uri).Returns(new Uri("bolt://localhost"));

            var bgc = new BoltGraphClient(mockDriver.Object);
            bgc.Connect();

            var cwd = new ClassWithDateTime{Dt = new DateTime(2000, 1, 1)};;

            bgc.Cypher.Create("(c)").WithParam("testParam", cwd).ExecuteWithoutResults();

            var expectedParameters = new Dictionary<string, object>
            {
                {
                    "testParam", new Dictionary<string, object> {{"Dt", JsonConvert.SerializeObject(cwd.Dt).Trim('\"')}}
                }
            };

            mockSession.Verify(x => x.Run("CREATE (c)", It.Is<IDictionary<string, object>>(c => CompareDictionaries(c, expectedParameters))), Times.Once);
        }

        [Fact]
        public void SerializesDateTimeOffsetsProperly()
        {
            var mockSession = new Mock<ISession>();
            mockSession.Setup(s => s.Run("CALL dbms.components()")).Returns(new ServerInfo());

            var mockDriver = new Mock<IDriver>();
            mockDriver.Setup(d => d.Session(It.IsAny<AccessMode>())).Returns(mockSession.Object);
            mockDriver.Setup(d => d.Uri).Returns(new Uri("bolt://localhost"));

            var bgc = new BoltGraphClient(mockDriver.Object);
            bgc.Connect();

            var cwd = new ClassWithDateTimeOffset { Dt = new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(1)) }; ;

            bgc.Cypher.Create("(c)").WithParam("testParam", cwd).ExecuteWithoutResults();

            var expectedParameters = new Dictionary<string, object>
            {
                {
                    "testParam", new Dictionary<string, object> {{"Dt", JsonConvert.SerializeObject(cwd.Dt).Trim('\"')}}
                }
            };

            mockSession.Verify(x => x.Run("CREATE (c)", It.Is<IDictionary<string, object>>(c => CompareDictionaries(c, expectedParameters))), Times.Once);
        }

        [Fact]
        public void SerializesGuidsProperly()
        {
            var mockSession = new Mock<ISession>();
            mockSession.Setup(s => s.Run("CALL dbms.components()")).Returns(new ServerInfo());

            var mockDriver = new Mock<IDriver>();
            mockDriver.Setup(d => d.Session(It.IsAny<AccessMode>())).Returns(mockSession.Object);
            mockDriver.Setup(d => d.Uri).Returns(new Uri("bolt://localhost"));

            var bgc = new BoltGraphClient(mockDriver.Object);
            bgc.Connect();

            var cwg = new ClassWithGuid();

            bgc.Cypher.Create("(c)").WithParam("testParam", cwg).ExecuteWithoutResults();
            
            var expectedParameters = new Dictionary<string, object>
            {
            {"testParam", new Dictionary<string, object>{{"Id", cwg.Id.ToString()} }
            }};

            mockSession.Verify(x => x.Run("CREATE (c)", It.Is<IDictionary<string, object>>(c => CompareDictionaries(c, expectedParameters))), Times.Once);
        }

        [Fact]
        public void SerializesGuidsProperlyWhenAutoGeneratingParams()
        {
            var mockSession = new Mock<ISession>();
            mockSession.Setup(s => s.Run("CALL dbms.components()")).Returns(new ServerInfo());

            var mockDriver = new Mock<IDriver>();
            mockDriver.Setup(d => d.Session(It.IsAny<AccessMode>())).Returns(mockSession.Object);
            mockDriver.Setup(d => d.Uri).Returns(new Uri("bolt://localhost"));

            var bgc = new BoltGraphClient( mockDriver.Object);
            bgc.Connect();

            var cwg = new ClassWithGuid();

            bgc.Cypher.Create("(c)").Where((ClassWithGuid c) => c.Id == cwg.Id).ExecuteWithoutResults();

            var expectedParameters = new Dictionary<string, object> {{"p0", $"\"{cwg.Id}\""}};

            mockSession.Verify(x => x.Run("CREATE (c)\r\nWHERE (c.Id = {p0})", It.Is<IDictionary<string, object>>(c => CompareDictionaries(c, expectedParameters))), Times.Once);
        }

        private static bool CompareDictionaries<TKey, TValue>(IDictionary<TKey, TValue> d1, IDictionary<TKey, TValue> d2)
        {
            if (d1 == null && d2 == null)
                return true;
            if (d1 == null || d2 == null)
                return false;

            if (d1.Count != d2.Count)
                return false;

            foreach (var d1Key in d1.Keys)
            {
                if (!d2.ContainsKey(d1Key))
                    return false;

                var v1 = d1[d1Key];
                var v2 = d2[d1Key];
                if (v1.GetType() == typeof(Dictionary<TKey, TValue>))
                    return CompareDictionaries((IDictionary<TKey, TValue>)v1, (IDictionary<TKey, TValue>)v2);

                if (!d1[d1Key].Equals(d2[d1Key]))
                    return false;
            }
            return true;
        }

        [Fact]
        public void RootNode_ThrowsInvalidOperationException()
        {
            var bgc = new BoltGraphClient(DriverTestHelper.MockDriverWithConnectionSet().Object);
            var ex = Record.Exception(() => bgc.RootNode);
            ex.Should().NotBeNull();
            ex.Should().BeOfType<InvalidOperationException>();
            ex.Message.Should().Be(BoltGraphClient.NotValidForBolt);
        }

        [Fact]
        public void Create_ThrowsInvalidOperationException()
        {
            var bgc = new BoltGraphClient(DriverTestHelper.MockDriverWithConnectionSet().Object);
            var ex = Record.Exception(() => bgc.Create("value", null));
            ex.Should().NotBeNull();
            ex.Should().BeOfType<InvalidOperationException>();
            ex.Message.Should().Be(BoltGraphClient.NotValidForBolt);
        }

        [Fact]
        public void Get_ThrowsInvalidOperationException()
        {
            var bgc = new BoltGraphClient(DriverTestHelper.MockDriverWithConnectionSet().Object);
            var ex = Record.Exception(() => bgc.Get<string>(new NodeReference(1)));
            ex.Should().NotBeNull();
            ex.Should().BeOfType<InvalidOperationException>();
            ex.Message.Should().Be(BoltGraphClient.NotValidForBolt);
        }

        [Fact]
        public void GetNode_ThrowsInvalidOperationException()
        {
            var bgc = new BoltGraphClient(DriverTestHelper.MockDriverWithConnectionSet().Object);
            var ex = Record.Exception(() => bgc.Get(new NodeReference<string>(1)));
            ex.Should().NotBeNull();
            ex.Should().BeOfType<InvalidOperationException>();
            ex.Message.Should().Be(BoltGraphClient.NotValidForBolt);
        }

        [Fact]
        public void Get_RelationReferenceTNode_ThrowsInvalidOperationException()
        {
            var bgc = new BoltGraphClient(DriverTestHelper.MockDriverWithConnectionSet().Object);
            var ex = Record.Exception(() => bgc.Get<TestNode>(new RelationshipReference(1)));
            ex.Should().NotBeNull();
            ex.Should().BeOfType<InvalidOperationException>();
            ex.Message.Should().Be(BoltGraphClient.NotValidForBolt);
        }

        [Fact]
        public void Get_RelationReference_ThrowsInvalidOperationException()
        {
            var bgc = new BoltGraphClient(DriverTestHelper.MockDriverWithConnectionSet().Object);
            var ex = Record.Exception(() => bgc.Get(new RelationshipReference<TestNode>(1)));
            ex.Should().NotBeNull();
            ex.Should().BeOfType<InvalidOperationException>();
            ex.Message.Should().Be(BoltGraphClient.NotValidForBolt);
        }

        [Fact]
        public async Task GetAsync_ThrowsInvalidOperationException()
        {
            var bgc = new BoltGraphClient(DriverTestHelper.MockDriverWithConnectionSet().Object);
            var ex = await Record.ExceptionAsync(() => bgc.GetAsync<string>(new NodeReference(1)));
            ex.Should().NotBeNull();
            ex.Should().BeOfType<InvalidOperationException>();
            ex.Message.Should().Be(BoltGraphClient.NotValidForBolt);
        }

        [Fact]
        public async Task GetAsync_RelationReference_ThrowsInvalidOperationException()
        {
            var bgc = new BoltGraphClient(DriverTestHelper.MockDriverWithConnectionSet().Object);
            var ex = await Record.ExceptionAsync(() => bgc.GetAsync<TestNode>(new RelationshipReference<TestNode>(1)));
            ex.Should().NotBeNull();
            ex.Should().BeOfType<InvalidOperationException>();
            ex.Message.Should().Be(BoltGraphClient.NotValidForBolt);
        }


    }
}