using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Neo4j.Driver.V1;
using Neo4jClient.Test.BoltGraphClientTests;
using Neo4jClient.Test.Fixtures;
using Neo4jClient.Tests.Shared;
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
            mockDriver.Setup(d => d.Session(It.IsAny<AccessMode>(), It.IsAny<IEnumerable<string>>())).Returns(mockSession.Object);
            mockDriver.Setup(d => d.Uri).Returns(new Uri("bolt://localhost"));

            var bgc = new BoltGraphClient(mockDriver.Object);
            bgc.Connect();

            var cwd = new ClassWithDateTime{Dt = new DateTime(2000, 1, 1)};;

            var cfq = bgc.Cypher.Create("(c)").WithParam("testParam", cwd);

            var expectedParameters = new Dictionary<string, object>
            {
                {
                    "testParam", new Dictionary<string, object> {{"Dt", JsonConvert.SerializeObject(cwd.Dt).Trim('\"')}}
                }
            };

            var query = cfq.Query;
            query.ToNeo4jDriverParameters(bgc).IsEqualTo(expectedParameters).Should().BeTrue();
        }

        [Fact]
        public void SerializesDateTimeOffsetsProperly()
        {
            var mockSession = new Mock<ISession>();
            mockSession.Setup(s => s.Run("CALL dbms.components()")).Returns(new ServerInfo());

            var mockDriver = new Mock<IDriver>();
            mockDriver.Setup(d => d.Session(It.IsAny<AccessMode>())).Returns(mockSession.Object);
            mockDriver.Setup(d => d.Session(It.IsAny<AccessMode>(), It.IsAny<IEnumerable<string>>())).Returns(mockSession.Object);
            mockDriver.Setup(d => d.Uri).Returns(new Uri("bolt://localhost"));

            var bgc = new BoltGraphClient(mockDriver.Object);
            bgc.Connect();

            var cwd = new ClassWithDateTimeOffset { Dt = new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(1)) }; ;

            var cfq = bgc.Cypher.Create("(c)").WithParam("testParam", cwd);

            var expectedParameters = new Dictionary<string, object>
            {
                {
                    "testParam", new Dictionary<string, object> {{"Dt", JsonConvert.SerializeObject(cwd.Dt).Trim('\"')}}
                }
            };

            var query = cfq.Query;
            query.ToNeo4jDriverParameters(bgc).IsEqualTo(expectedParameters).Should().BeTrue();
        }

        [Fact]
        public void SerializesGuidsProperly()
        {
            var mockSession = new Mock<ISession>();
            mockSession.Setup(s => s.Run("CALL dbms.components()")).Returns(new ServerInfo());

            var mockDriver = new Mock<IDriver>();
            mockDriver.Setup(d => d.Session(It.IsAny<AccessMode>())).Returns(mockSession.Object);
            mockDriver.Setup(d => d.Session(It.IsAny<AccessMode>(), It.IsAny<IEnumerable<string>>())).Returns(mockSession.Object);
            mockDriver.Setup(d => d.Uri).Returns(new Uri("bolt://localhost"));

            var bgc = new BoltGraphClient(mockDriver.Object);
            bgc.Connect();

            var cwg = new ClassWithGuid();

            var cfq = bgc.Cypher.Create("(c)").WithParam("testParam", cwg);
            
            var expectedParameters = new Dictionary<string, object>
            {
            {"testParam", new Dictionary<string, object>{{"Id", cwg.Id.ToString()} }
            }};

            var query = cfq.Query;
            query.ToNeo4jDriverParameters(bgc).IsEqualTo(expectedParameters).Should().BeTrue();
        }

        [Fact]
        public void SerializesGuidsProperlyWhenAutoGeneratingParams()
        {
            var mockSession = new Mock<ISession>();
            mockSession.Setup(s => s.Run("CALL dbms.components()")).Returns(new ServerInfo());

            var mockDriver = new Mock<IDriver>();
            mockDriver.Setup(d => d.Session(It.IsAny<AccessMode>())).Returns(mockSession.Object);
            mockDriver.Setup(d => d.Session(It.IsAny<AccessMode>(), It.IsAny<IEnumerable<string>>())).Returns(mockSession.Object);
            mockDriver.Setup(d => d.Uri).Returns(new Uri("bolt://localhost"));

            var bgc = new BoltGraphClient( mockDriver.Object);
            bgc.Connect();

            var cwg = new ClassWithGuid();

            var cfq = bgc.Cypher.Create("(c)").Where((ClassWithGuid c) => c.Id == cwg.Id);

            var expectedParameters = new Dictionary<string, object> {{"p0", $"{cwg.Id}"}};

            var query = cfq.Query;
            query.ToNeo4jDriverParameters(bgc).IsEqualTo(expectedParameters).Should().BeTrue();
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

        public class Constructor : IClassFixture<CultureInfoSetupFixture>
        {
            [Fact]
            public void DoesntUseAddressResolverWhenPassingInOneUri()
            {
                var bgc = new BoltGraphClient($"bolt+routing://virtual.foo.com");
                bgc.AddressResolver.Should().BeNull();
            }

            [Fact]
            public void UsesAddressResolverWhenPassingInMultipleUris()
            {
                var bgc = new BoltGraphClient($"bolt+routing://virtual.foo.com", new[] {"x.foo.com", "y.foo.com"});
                var resolved = bgc.AddressResolver.Resolve(null);
                resolved.Should().HaveCount(2);
            }


            [Fact]
            public void ValidForBoltPlusRoutingUris()
            {
                var ex = Record.Exception(() => new BoltGraphClient($"bolt+routing://virtual.foo.com", new[] {"x.foo.com", "y.foo.com"}));
                ex.Should().BeNull();
            }

            [Fact]
            public void DoesntNeedVirtualUriToBeSupplied()
            {
                const string uri = "x.foo.com";

                var bgc = new BoltGraphClient( new[] { $"{uri}" });
                var resolved = bgc.AddressResolver.Resolve(null);
                resolved.Should().HaveCount(1);
                resolved.First().Host.Should().Be(uri);
            }

            [Theory]
            [InlineData("bolt")]
            [InlineData("https")]
            [InlineData("http")]
            [InlineData("ftp")]
            public void NotValidForOtherUriSchemes(string scheme)
            {
                var ex = Record.Exception(() => new BoltGraphClient($"{scheme}://virtual.foo.com", new [] {"x.foo.com", "y.foo.com"} ));
                ex.Should().NotBeNull();
                ex.Should().BeOfType<NotSupportedException>();
            }

            [Theory]
            [InlineData("bolt")]
            [InlineData("https")]
            [InlineData("http")]
            [InlineData("ftp")]
            public void WorksIfYouPassInWholeUris(string schema)
            {
                const string uri = "x.foo.com";
                
                var bgc = new BoltGraphClient($"bolt+routing://virtual.foo.com", new[] { $"{schema}://{uri}" });
                var resolved = bgc.AddressResolver.Resolve(null);
                resolved.Should().HaveCount(1);
                resolved.First().Host.Should().Be(uri);
            }

            [Fact]
            public void WorksIfYouPassInUrisWithoutScheme()
            {
                const string uri = "x.foo.com";

                var bgc = new BoltGraphClient($"bolt+routing://virtual.foo.com", new[] { uri });
                var resolved = bgc.AddressResolver.Resolve(null);
                resolved.Should().HaveCount(1);
                resolved.First().Host.Should().Be(uri);
            }
        }
    }
}