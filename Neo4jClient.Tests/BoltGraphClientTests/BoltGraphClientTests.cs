using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Neo4j.Driver;
using Neo4jClient.Tests.Extensions;
using Newtonsoft.Json;
using Xunit;

namespace Neo4jClient.Tests.BoltGraphClientTests
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
        public class ServerInfo : TestStatementResult
        {
            public ServerInfo(): base(new List<IRecord> {new TestRecord(new Dictionary<string, object>
            {
                {"name", "neo4j kernel"},
                {"versions", new List<object>{"3.2.3"} }
            })})
            {
            }

            public ServerInfo(IList<IRecord> records): base(records)
            {
            }
        }

        public class ClassWithSomeNeo4jIgnoreAttributes
        {
            public string Text { get; set; }
            [Neo4jIgnore]
            public string TextIgnore { get; set; }
            public int TestInt { get; set; }
            [Neo4jIgnore]
            public int TestNeo4jIntIgnore { get; set; }
            [JsonIgnore]
            public int TestJsonIntIgnore { get; set; }
        }

        [Fact]
        //[Description("test bolt part of https://github.com/Readify/Neo4jClient/issues/336  https://github.com/Readify/Neo4jClient/pull/337 - see UserSuppliedSerializationTests for https part")]
        public async Task JsonSerializerShouldNotSerializeNeo4jIgnoreAttribute()
        {
            var mockSession = new Mock<IAsyncSession>();
            mockSession.Setup(s => s.RunAsync("CALL dbms.components()")).Returns(Task.FromResult<IResultCursor>(new ServerInfo()));

            var mockDriver = new Mock<IDriver>();
            mockDriver.Setup(d => d.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>())).Returns(mockSession.Object);

            var bgc = new BoltGraphClient(mockDriver.Object);
            await bgc.ConnectAsync();

            var cwa = new ClassWithSomeNeo4jIgnoreAttributes { Text = "foo", TextIgnore = "fooignore", TestInt = 42, TestNeo4jIntIgnore = 42, TestJsonIntIgnore = 42 };

            var cfq = bgc.Cypher.Create("(c)").WithParam("testParam", cwa);

            var expectedParameters = new Dictionary<string, object>
            {
            {"testParam", new Dictionary<string, object> {{"Text", "foo"}, { "TestInt", 42} }
            }};

            var query = cfq.Query;
            query.ToNeo4jDriverParameters(bgc).IsEqualTo(expectedParameters).Should().BeTrue();
        }


        [Fact]
        public async Task SerializesDateTimesProperly()
        {
            var mockSession = new Mock<IAsyncSession>();
            mockSession.Setup(s => s.RunAsync("CALL dbms.components()")).Returns(Task.FromResult<IResultCursor>(new ServerInfo()));
            
            var mockDriver = new Mock<IDriver>();
            mockDriver.Setup(d => d.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>())).Returns(mockSession.Object);
            
            var bgc = new BoltGraphClient(mockDriver.Object);
            await bgc.ConnectAsync();

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
        public async Task SerializesDateTimeOffsetsProperly()
        {
            var mockSession = new Mock<IAsyncSession>();
            mockSession.Setup(s => s.RunAsync("CALL dbms.components()")).Returns(Task.FromResult<IResultCursor>(new ServerInfo()));

            var mockDriver = new Mock<IDriver>();
            mockDriver.Setup(d => d.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>())).Returns(mockSession.Object);

            var bgc = new BoltGraphClient(mockDriver.Object);
            await bgc.ConnectAsync();

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
        public async Task SerializesGuidsProperly()
        {
            var mockSession = new Mock<IAsyncSession>();
            mockSession.Setup(s => s.RunAsync("CALL dbms.components()")).Returns(Task.FromResult<IResultCursor>(new ServerInfo()));

            var mockDriver = new Mock<IDriver>();
            mockDriver.Setup(d => d.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>())).Returns(mockSession.Object);

            var bgc = new BoltGraphClient(mockDriver.Object);
            await bgc.ConnectAsync();

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
        public async Task SerializesGuidsProperlyWhenAutoGeneratingParams()
        {
            var mockSession = new Mock<IAsyncSession>();
            mockSession.Setup(s => s.RunAsync("CALL dbms.components()")).Returns(Task.FromResult<IResultCursor>(new ServerInfo()));

            var mockDriver = new Mock<IDriver>();
            mockDriver.Setup(d => d.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>())).Returns(mockSession.Object);

            var bgc = new BoltGraphClient( mockDriver.Object);
            await bgc.ConnectAsync();

            var cwg = new ClassWithGuid();

            var cfq = bgc.Cypher.Create("(c)").Where((ClassWithGuid c) => c.Id == cwg.Id);

            var expectedParameters = new Dictionary<string, object> {{"p0", $"{cwg.Id}"}};

            var query = cfq.Query;
            query.ToNeo4jDriverParameters(bgc).IsEqualTo(expectedParameters).Should().BeTrue();
        }
        //
        // [Fact]
        // public void RootNode_ThrowsInvalidOperationException()
        // {
        //     var bgc = new BoltGraphClient(DriverTestHelper.MockDriverWithConnectionSet().Object);
        //     var ex = Record.Exception(() => bgc.RootNode);
        //     ex.Should().NotBeNull();
        //     ex.Should().BeOfType<InvalidOperationException>();
        //     ex.Message.Should().Be(BoltGraphClient.NotValidForBolt);
        // }
        //
        // [Fact]
        // public async Task Create_ThrowsInvalidOperationException()
        // {
        //     var bgc = new BoltGraphClient(DriverTestHelper.MockDriverWithConnectionSet().Object);
        //     var ex = await Record.ExceptionAsync(async () => await bgc.CreateAsync("value", null));
        //     ex.Should().NotBeNull();
        //     ex.Should().BeOfType<InvalidOperationException>();
        //     ex.Message.Should().Be(BoltGraphClient.NotValidForBolt);
        // }

        // [Fact]
        // public async Task GetAsync_ThrowsInvalidOperationException()
        // {
        //     var bgc = new BoltGraphClient(DriverTestHelper.MockDriverWithConnectionSet().Object);
        //     var ex = await Record.ExceptionAsync(async () => await bgc.GetAsync<string>(new NodeReference(1)));
        //     ex.Should().NotBeNull();
        //     ex.Should().BeOfType<InvalidOperationException>();
        //     ex.Message.Should().Be(BoltGraphClient.NotValidForBolt);
        // }
        //
        // [Fact]
        // public async Task GetAsync_RelationReference_ThrowsInvalidOperationException()
        // {
        //     var bgc = new BoltGraphClient(DriverTestHelper.MockDriverWithConnectionSet().Object);
        //     var ex = await Record.ExceptionAsync(async () => await bgc.GetAsync<TestNode>(new RelationshipReference<TestNode>(1)));
        //     ex.Should().NotBeNull();
        //     ex.Should().BeOfType<InvalidOperationException>();
        //     ex.Message.Should().Be(BoltGraphClient.NotValidForBolt);
        // }

        public class Constructor : IClassFixture<CultureInfoSetupFixture>
        {
            [Fact]
            public void DoesntUseAddressResolverWhenPassingInOneUri()
            {
                var bgc = new BoltGraphClient($"neo4j://virtual.foo.com");
                bgc.AddressResolver.Should().BeNull();
            }

            [Theory]
            [InlineData("neo4j")]
            [InlineData("neo4j+s")]
            [InlineData("neo4j+ssc")]
            public void UsesAddressResolverWhenPassingInMultipleUris(string scheme)
            {
                var bgc = new BoltGraphClient($"{scheme}://virtual.foo.com", new[] {"x.foo.com", "y.foo.com"});
                var resolved = bgc.AddressResolver.Resolve(null);
                resolved.Should().HaveCount(2);
            }

            [Theory]
            [InlineData("bolt", true)]
            [InlineData("bolt+s", true)]
            [InlineData("bolt+ssc", true)]
            [InlineData("neo4j", true)]
            [InlineData("neo4j+s", true)]
            [InlineData("neo4j+ssc", true)]
            [InlineData("http", false)]
            [InlineData("ftp", false)]
            public void ValidityForVariousUriSchemes(string scheme, bool expectedValid)
            {
                var ex = Record.Exception(() => new BoltGraphClient($"{scheme}://virtual.foo.com"));
                if (expectedValid) ex.Should().BeNull();
                else ex.Should().NotBeNull();
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
            [InlineData("bolt+s")]
            [InlineData("bolt+ssc")]
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
            [InlineData("bolt+s")]
            [InlineData("bolt+ssc")]
            [InlineData("https")]
            [InlineData("http")]
            [InlineData("ftp")]
            public void WorksIfYouPassInWholeUris(string schema)
            {
                const string uri = "x.foo.com";
                
                var bgc = new BoltGraphClient($"neo4j://virtual.foo.com", new[] { $"{schema}://{uri}" });
                var resolved = bgc.AddressResolver.Resolve(null);
                resolved.Should().HaveCount(1);
                resolved.First().Host.Should().Be(uri);
            }

            [Fact]
            public void WorksIfYouPassInUrisWithoutScheme()
            {
                const string uri = "x.foo.com";

                var bgc = new BoltGraphClient($"neo4j://virtual.foo.com", new[] { uri });
                var resolved = bgc.AddressResolver.Resolve(null);
                resolved.Should().HaveCount(1);
                resolved.First().Host.Should().Be(uri);
            }
        }
    }
}