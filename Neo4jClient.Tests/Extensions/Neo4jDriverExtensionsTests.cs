using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using Neo4jClient.Cypher;
using Newtonsoft.Json;
using Xunit;

namespace Neo4jClient.Tests.Extensions
{
    internal class ClassWithGuid
    {
        public ClassWithGuid()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
    }

    internal class ClassWithDateTime
    {
        public DateTime Dt { get; set; }
    }

    internal class ClassWithDateTimeOffset
    {
        public DateTimeOffset Dt { get; set; }
    }

    public class Neo4jDriverExtensionsTests
    {
        public class ToNeo4jDriverParametersMethod : IClassFixture<CultureInfoSetupFixture>
        {
            private Mock<IRawGraphClient> MockGc
            {
                get
                {
                    var mockGc = new Mock<IRawGraphClient>();
                    mockGc.Setup(x => x.JsonConverters).Returns(new List<JsonConverter>());
                    return mockGc;
                }
            }

            private class NeoDateTimeSerializer : JsonConverter
            {
                public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
                {
                    var dt = (DateTime) value;
                    var ticks = dt.ToUniversalTime().Ticks;
                    writer.WriteValue(ticks);
                }

                public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
                {
                    throw new NotImplementedException();
                }

                public override bool CanConvert(Type objectType)
                {
                    return objectType == typeof(DateTime);
                }
            }

            private class DateTimeContainer
            {
                public DateTime Dt { get; set; }
            }

            private class Foo
            {
                public string Bar { get; set; }
            }

            [Fact]
            public void SerializeObjectWithArrays()
            {
                var list = new[] {"foo", "bar"};

                var mockGc = MockGc;
                var query = new CypherFluentQuery(mockGc.Object)
                    .Create("(n:Node {p})")
                    .WithParam("p", new {Collection = list});

                var actual = query.Query.ToNeo4jDriverParameters(mockGc.Object);
                actual.Keys.Should().Contain("p");
                actual["p"].Should().BeOfType<Dictionary<string, object>>();
                var serializedObj = (Dictionary<string, object>) actual["p"];
                serializedObj.Keys.Should().Contain("Collection");
                (serializedObj["Collection"] as IEnumerable).Should().NotBeNull();
                var expectedCollection = ((IEnumerable) serializedObj["Collection"]).Cast<string>().ToArray();
                expectedCollection.Length.Should().Be(2);
                expectedCollection[0].Should().Be("foo");
                expectedCollection[1].Should().Be("bar");
            }

            [Fact]
            public void SerializesArraysOfSimpleTypesCorrectly()
            {
                var list = new[] {"foo", "bar"};

                var mockGc = MockGc;
                var query = new CypherFluentQuery(mockGc.Object)
                    .Match("(n)")
                    .Where("n.Id IN $listParam")
                    .WithParam("listParam", list);

                var actual = query.Query.ToNeo4jDriverParameters(mockGc.Object);
                actual.Keys.Should().Contain("listParam");
                actual["listParam"].Should().BeOfType<object[]>();
                ((object[]) actual["listParam"])[0].Should().Be("foo");
                ((object[]) actual["listParam"])[1].Should().Be("bar");
            }

            [Fact]
            public void SerializesBoolsProperly()
            {
                var mockGc = MockGc;
                var query = new CypherFluentQuery(mockGc.Object)
                    .WithParam("One", true);

                var actual = query.Query.ToNeo4jDriverParameters(mockGc.Object);
                actual.Keys.Should().Contain("One");
                actual["One"].Should().Be(true);
            }

            [Fact]
            public void SerializesDecimalsProperly()
            {
                var mockGc = MockGc;
                var query = new CypherFluentQuery(mockGc.Object)
                    .WithParam("One", 12.3m);

                var actual = query.Query.ToNeo4jDriverParameters(mockGc.Object);
                actual.Keys.Should().Contain("One");
                actual["One"].Should().Be(12.3m);
            }


            [Fact]
            public void SerializesGuidsCorrectly()
            {
                var cwg = new ClassWithGuid();

                var mockGc = new Mock<IRawGraphClient>();
                mockGc.Setup(x => x.JsonConverters).Returns(new List<JsonConverter>());

                var query = new CypherFluentQuery(mockGc.Object)
                    .Create("(t:Test {testParam})")
                    .WithParam("testParam", cwg);

                var parameters = query.Query.ToNeo4jDriverParameters(mockGc.Object);
                parameters.Should().NotBeNull();
                parameters.Count.Should().Be(1);
                var item = parameters.First();
                ((IDictionary<string, object>) item.Value)[nameof(ClassWithGuid.Id)].Should().BeOfType<string>();
                ((IDictionary<string, object>) item.Value)[nameof(ClassWithGuid.Id)].Should().Be(cwg.Id.ToString());
            }

            [Fact]
            public void SerializesIntsProperly()
            {
                var mockGc = MockGc;
                var query = new CypherFluentQuery(mockGc.Object)
                    .WithParam("One", 1);

                var actual = query.Query.ToNeo4jDriverParameters(mockGc.Object);
                actual.Keys.Should().Contain("One");
                actual["One"].Should().Be(1);
            }

            [Fact]
            public void SerializesListsCorrectly()
            {
                var list = new List<Foo> {new Foo {Bar = "test"}};

                var mockGc = MockGc;
                var query = new CypherFluentQuery(mockGc.Object)
                    .Unwind(list, "item");

                var actual = query.Query.ToNeo4jDriverParameters(mockGc.Object);
                actual.Keys.Should().Contain("p0");
                actual["p0"].Should().BeOfType<object[]>();
                var serializedFoo = ((object[]) actual["p0"])[0];
                serializedFoo.Should().BeOfType<Dictionary<string, object>>();
                var serializedFooDict = (Dictionary<string, object>) serializedFoo;
                serializedFooDict.Keys.Should().Contain("Bar");
                serializedFooDict["Bar"].Should().Be(list[0].Bar);
            }

            [Fact]
            public void SerializesListsOfSimpleTypesCorrectly()
            {
                var list = new List<string> {"foo", "bar"};

                var mockGc = MockGc;
                var query = new CypherFluentQuery(mockGc.Object)
                    .Match("(n)")
                    .Where("n.Id IN $listParam")
                    .WithParam("listParam", list);

                var actual = query.Query.ToNeo4jDriverParameters(mockGc.Object);
                actual.Keys.Should().Contain("listParam");
                actual["listParam"].Should().BeOfType<object[]>();
                var serialized = (object[]) actual["listParam"];
                serialized[0].Should().Be(list[0]);
                serialized[1].Should().Be(list[1]);
            }

            [Fact]
            public void SerializesStringsProperly()
            {
                var mockGc = MockGc;
                var query = new CypherFluentQuery(mockGc.Object)
                    .WithParam("One", "one");

                var actual = query.Query.ToNeo4jDriverParameters(mockGc.Object);
                actual.Keys.Should().Contain("One");
                actual["One"].Should().Be("one");
            }

            [Fact]
            public void SerializesTimeSpanCorrectly()
            {
                var ts = new TimeSpan(1, 2, 3, 4, 5);

                var query = new CypherFluentQuery(MockGc.Object)
                    .WithParam("tsParam", ts);

                var actual = query.Query.ToNeo4jDriverParameters(MockGc.Object);
                actual.Keys.Should().Contain("tsParam");
                var tsParam = actual["tsParam"].ToString();
                tsParam.Should().Be("1.02:03:04.0050000");
            }

            [Fact]
            //Issue 292 - https://github.com/Readify/Neo4jClient/issues/292
            public void SerializesWhenJsonConverterContainsNothingThatMatches()
            {
                var ts = new TimeSpan(1, 2, 3, 4, 5);
                var mockGc = MockGc;
                mockGc
                    .Setup(x => x.JsonConverters)
                    .Returns(new List<JsonConverter> {new NeoDateTimeSerializer()});

                var query = new CypherFluentQuery(mockGc.Object)
                    .WithParam("tsParam", ts);

                var actual = query.Query.ToNeo4jDriverParameters(MockGc.Object);
                actual.Keys.Should().Contain("tsParam");
                var tsParam = actual["tsParam"].ToString();
                tsParam.Should().Be("1.02:03:04.0050000");
            }

            [Fact]
            //Issue 292 - https://github.com/Readify/Neo4jClient/issues/292
            public void UsesCustomJsonSerializersWhereItCan()
            {
                var dateTime = new DateTime(2000, 1, 1);
                var mockGc = MockGc;
                mockGc
                    .Setup(x => x.JsonConverters)
                    .Returns(new List<JsonConverter> {new NeoDateTimeSerializer()});

                var query = new CypherFluentQuery(mockGc.Object)
                    .Create("(n:Node {p})")
                    .WithParam("p", new DateTimeContainer {Dt = dateTime});

                var actual = query.Query.ToNeo4jDriverParameters(mockGc.Object);
                actual.Keys.Should().Contain("p");
                actual["p"].Should().BeOfType<Dictionary<string, object>>();
                var serializedObj = (Dictionary<string, object>) actual["p"];

                serializedObj["Dt"].Should().Be(dateTime.ToUniversalTime().Ticks);
            }
        }
    }
}