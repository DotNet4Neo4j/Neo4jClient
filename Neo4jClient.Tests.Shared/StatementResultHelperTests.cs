using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FluentAssertions;
using Moq;
using Neo4j.Driver.V1;
using Neo4jClient.Cypher;
using Neo4jClient.Serialization;
using Neo4jClient.Test.Fixtures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Test
{
    public class StatementResultHelperTests
    {
        private class ComplexClass
        {
            public SimpleIntClass SimpleIntClass { get; set; }
            public IEnumerable<SimpleIntClass> Ints { get; set; }
            public IEnumerable<SimpleIntClass> Ints2 { get; set; }
        }

        private class SimpleIntClass
        {
            public int IntValue { get; set; }
        }

        private class TestRelationship : IRelationship {
            public TestRelationship() { }

            public TestRelationship(IDictionary<string, object> properties, int id = 99, int startId = 100, int endId = 200)
            {
                StartNodeId = startId;
                EndNodeId = endId;
                Properties = new ReadOnlyDictionary<string, object>(properties);
                Id = id;
            }

            public object this[string key] => Properties[key];

            public IReadOnlyDictionary<string, object> Properties { get; }
            public long Id { get; }
            public bool Equals(IRelationship other)
            {
                return true;
            }

            public string Type { get; }
            public long StartNodeId { get; }
            public long EndNodeId { get; }
        }

        private class TestNode : INode
        {
            public TestNode() { }
            public TestNode(Dictionary<string, object> properties, int id = 100)
            {
                Properties = properties;
                Id = id;
                Labels = new List<string> { "Foo" };
            }

            public object this[string key] => Properties[key];

            public IReadOnlyDictionary<string, object> Properties { get; }
            public long Id { get; }

            public bool Equals(INode other)
            {
                return false;
            }

            public IReadOnlyList<string> Labels { get; }
        }

        public abstract class BaseClass
        {
            public string Base { get; set; }
        }

        public class DerivedClass : BaseClass
        {
            public string Derived { get; set; }
        }

        private class ClassWithDateTime
        {
            public DateTimeOffset Offset { get; set; }
        }

        private class ClassWithDateTimeAndAttribute
        {
            [Neo4jDateTime]
            public DateTimeOffset Offset { get; set; }
        }

        private class ClassWithArray
        {
            public string[] StrArr { get; set; }
        }

        private static IGraphClient GraphClient
        {
            get
            {
                var gc = Substitute.For<IGraphClient>();
                gc.JsonConverters.Returns(BoltGraphClient.DefaultJsonConverters.ToList());
                gc.JsonContractResolver.Returns(new DefaultContractResolver());

                return gc;
            }
        }

        private class ConvertibleClass
        {
            public string Value { get; set; }
        }

        public class ConvertibleJsonConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var jObject = JObject.Load(reader);
                var t = JsonConvert.DeserializeObject<ConvertibleClass>(jObject.ToString());
                t.Value = "FooBar";
                return t;
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(ConvertibleClass);
            }
        }

        public class ToJsonMethod : IClassFixture<CultureInfoSetupFixture>
        {
            [Fact]
            public void ConvertsCorrectly()
            {
                var node = Substitute.For<INode>();
                node.Properties.Returns(
                    new Dictionary<string, object>
                    {
                        {"Value", "Value"},
                        {"Complex", "{\"SimpleIntClass\":{\"IntValue\":23}}" }
                    });

                var jsonString = node.ToJsonString();
                jsonString = jsonString.Replace("\r\n", "").Replace(" ", "");
                jsonString.Should().Be("{\"Value\":\"Value\",\"Complex\":\"{\\\"SimpleIntClass\\\":{\\\"IntValue\\\":23}}\"}");

            }
        }

        public class Deserialize_TMethod : IClassFixture<CultureInfoSetupFixture>
        {
            public class RecordWithList
            {
                public string A { get; set; }
                public IEnumerable<object> B { get; set; }
                public RecordWithList()
                {
                    B = new List<object>();
                }
            }

            [Fact]
            public void DeserializeRecordWithListCorrectly()
            {
                var record = Substitute.For<IRecord>();
                record.Keys.Returns(new List<string> { "x" });
                record["x"].Returns(new List<object>
                {
                    new KeyValuePair<string, object>("A", null),
                    new KeyValuePair<string, object>("B", new List<object>())
                });

                var expectedContent = "{ \"columns\":[\"x\"], \"data\":[[ \"A\":null,\"B\":[] ]] }";

                var mockDeserializer = new Mock<ICypherJsonDeserializer<RecordWithList>>();
                mockDeserializer
                    .Setup(d => d.Deserialize(It.IsAny<string>()))
                    .Returns(new List<RecordWithList>());

                record.Deserialize(mockDeserializer.Object, CypherResultMode.Projection);
                mockDeserializer.Verify(d => d.Deserialize(expectedContent), Times.Once);
            }

            [Fact]
            public void DeserializeSetCorrectly()
            {
                var record = Substitute.For<IRecord>();
                record.Keys.Returns(new List<string> { "x" });
                record["x"].Returns(new List<int>{1,2});

                var expectedContent = "{ \"columns\":[\"x\"], \"data\":[[ [1,2] ]] }";

                var mockDeserializer = new Mock<ICypherJsonDeserializer<string>>();
                mockDeserializer
                    .Setup(d => d.Deserialize(It.IsAny<string>()))
                    .Returns(new List<string>());

                record.Deserialize(mockDeserializer.Object, CypherResultMode.Set);
                mockDeserializer.Verify(d => d.Deserialize(expectedContent), Times.Once);
            }

            [Fact]
            public void DeserializesAnonymousNestedObjectCorrectly()
            {
                var record = Substitute.For<IRecord>();
                record.Keys.Returns(new List<string> { "x" });
                record["x"].Returns(new List<object>
                    {
                        new Dictionary<string, object>
                        {
                            {
                                "Info", new Dictionary<string, object>
                                {
                                    {"A", "a"},
                                    {"B", "b"}
                                }
                            }
                        }
                    }
                );

                var expectedContent = "{ \"columns\":[\"x\"], \"data\":[[ [{{ \"Info\":{\"A\":\"a\",\"B\":\"b\"} }}] ]] }";
                var mockDeserializer = new Mock<ICypherJsonDeserializer<string>>();
                mockDeserializer
                    .Setup(d => d.Deserialize(It.IsAny<string>()))
                    .Returns(new List<string>());

                record.Deserialize(mockDeserializer.Object, CypherResultMode.Set);
                mockDeserializer.Verify(d => d.Deserialize(expectedContent), Times.Once);
            }

//            [Fact]
//            public void DeserializesAnonymousObjectCorrectly()
//            {
//                throw new NotImplementedException();
//            }

            [Fact]
            public void DeserializesPlainObjectCorrectly()
            {
                var node = new TestNode(new Dictionary<string, object> { { "Value", "foo" } });
                var record = Substitute.For<IRecord>();
                record.Keys.Returns(new List<string> { "x" });
                record["x"].Returns(node);

                var expectedContent = "{ \"columns\":[\"x\"], \"data\":[[ {\"data\":{ \"Value\":\"foo\" }} ]] }";

                var mockDeserializer = new Mock<ICypherJsonDeserializer<DerivedClass>>();
                mockDeserializer
                    .Setup(d => d.Deserialize(It.IsAny<string>()))
                    .Returns(new List<DerivedClass>());

                record.Deserialize(mockDeserializer.Object, CypherResultMode.Projection);
                mockDeserializer.Verify(d => d.Deserialize(expectedContent), Times.Once);
            }

            [Fact]
            public void DeserializesListOfObjectsCorrectly()
            {
                var node = new TestNode(new Dictionary<string, object> { { "Value", "foo" } });
                var record = Substitute.For<IRecord>();
                record.Keys.Returns(new List<string> { "x" });
                record["x"].Returns(node);

                var expectedContent = "{ \"columns\":[\"x\"], \"data\":[[ {\"data\":{ \"Value\":\"foo\" }} ]] }";

                var mockDeserializer = new Mock<ICypherJsonDeserializer<DerivedClass>>();
                mockDeserializer
                    .Setup(d => d.Deserialize(It.IsAny<string>()))
                    .Returns(new List<DerivedClass>());

                record.Deserialize(mockDeserializer.Object, CypherResultMode.Projection);
                mockDeserializer.Verify(d => d.Deserialize(expectedContent), Times.Once);
            }

            [Fact]
            public void DeserilizesProjectedListCorrectly()
            {
                var expectedContent = "{ \"columns\":[\"a\",\"b\"], \"data\":[[ {\"data\":{ \"Value\":\"foo\" }},[{\"nested_b1\":{\"data\":{ \"Value\":\"bar\" }}},{\"nested_b2\":{\"data\":{ \"Value\":\"foobar\" }}}] ]] }";

                var nodeA = new TestNode(new Dictionary<string, object> { { "Value", "foo" } });
                var nodeB1 = new TestRelationship(new Dictionary<string, object> {{"Value","bar"}});
                var nodeB2 = new TestNode(new Dictionary<string, object> { { "Value", "foobar" } });

                var listB = new List<object>
                {
                    new Dictionary<string, object> {{"nested_b1", nodeB1}},
                    new Dictionary<string, object> {{"nested_b2", nodeB2}}
                };
                
                var record = Substitute.For<IRecord>();
                record.Keys.Returns(new List<string> { "a", "b" });
                record["a"].Returns(nodeA);
                record["b"].Returns(listB);

                var mockDeserializer = new Mock<ICypherJsonDeserializer<DerivedClass>>();
                mockDeserializer
                    .Setup(d => d.Deserialize(It.IsAny<string>()))
                    .Returns(new List<DerivedClass>());

                record.Deserialize(mockDeserializer.Object, CypherResultMode.Projection);
                mockDeserializer.Verify(d => d.Deserialize(expectedContent), Times.Once);
            }

            [Fact]
            public void DeserializesStringContentCorrectly()
            {
                var expectedContent = "{ \"columns\":[\"n.Prop\"], \"data\":[[ \"Foo\" ]] }";
                var record = Substitute.For<IRecord>();
                record.Keys.Returns(new List<string> { "n.Prop" });
                record["n.Prop"].Returns("Foo");

                var mockDeserializer = new Mock<ICypherJsonDeserializer<string>>();
                mockDeserializer
                    .Setup(d => d.Deserialize(It.IsAny<string>()))
                    .Returns(new List<string>());

                record.Deserialize(mockDeserializer.Object, CypherResultMode.Set);
                mockDeserializer.Verify(d => d.Deserialize(expectedContent), Times.Once);
            }

            [Fact]
            public void DeserializesIntContentCorrectly()
            {
                var expectedContent = "{ \"columns\":[\"n.Prop\"], \"data\":[[ 1 ]] }";
                var record = Substitute.For<IRecord>();
                record.Keys.Returns(new List<string> { "n.Prop" });
                record["n.Prop"].Returns(1);

                var mockDeserializer = new Mock<ICypherJsonDeserializer<int>>();
                mockDeserializer
                    .Setup(d => d.Deserialize(It.IsAny<string>()))
                    .Returns(new List<int>());

                record.Deserialize(mockDeserializer.Object, CypherResultMode.Set);
                mockDeserializer.Verify(d => d.Deserialize(expectedContent), Times.Once);
            }

            [Fact]
            public void HandlesNullValuesCorrectly()
            {
                var expectedContent = "{ \"columns\":[\"a\"], \"data\":[[ null ]] }";

                var record = Substitute.For<IRecord>();
                record.Keys.Returns(new List<string> { "a" });
                record.Values["a"].Returns(null);

                var mockDeserializer = new Mock<ICypherJsonDeserializer<DerivedClass>>();
                mockDeserializer
                    .Setup(d => d.Deserialize(It.IsAny<string>()))
                    .Returns(new List<DerivedClass>());

                record.Deserialize(mockDeserializer.Object, CypherResultMode.Projection);
                mockDeserializer.Verify(d => d.Deserialize(expectedContent), Times.Once);
            }
        }

        public class Parse_TMethod : IClassFixture<CultureInfoSetupFixture>
        {

            [Fact]
            public void UserCustomJsonConvertersProperly()
            {
                const string value = "FooBar";

                var localGc = GraphClient;
                localGc.JsonConverters.Add(new ConvertibleJsonConverter());

                var node = new TestNode(new Dictionary<string, object> { { "Value", "anotherValue" } });
                var record = Substitute.For<IRecord>();
                record.Keys.Returns(new List<string> { "X" });
                record["X"].Returns(node);

                var result = record.Parse<ConvertibleClass>(localGc);
                result.Should().NotBeNull();
                result.Value.Should().Be(value);
            }

            [Fact]
            public void ParsesEmptyArrayCorrectly()
            {
                var node = new TestNode(new Dictionary<string, object> { { "StrArr", new List<object>() } });
                var record = Substitute.For<IRecord>();
                record.Keys.Returns(new List<string> { "X" });
                record["X"].Returns(node);

                var result = record.Parse<ClassWithArray>(GraphClient);
                result.Should().NotBeNull();
                result.StrArr.Should().HaveCount(0);
            }

            [Fact]
            public void ParsesArrayWithContentCorrectly()
            {
                var node = new TestNode(new Dictionary<string, object> { { "StrArr", new List<object>{"Foo", "Bar"} } });
                var record = Substitute.For<IRecord>();
                record.Keys.Returns(new List<string> { "X" });
                record["X"].Returns(node);

                var result = record.Parse<ClassWithArray>(GraphClient);
                result.Should().NotBeNull();
                result.StrArr.Should().HaveCount(2);
            }

            [Fact]
            public void ParsesDateTimesFromNeo4jCorrectly()
            {
                LocalDateTime ldt = new LocalDateTime(2000,3,1,1,1,1);
                var node = new TestNode(new Dictionary<string, object> { { "Offset", ldt } });
                var record = Substitute.For<IRecord>();
                record.Keys.Returns(new List<string> { "X" });
                record["X"].Returns(node);

                var result = record.Parse<ClassWithDateTimeAndAttribute>(GraphClient);
                result.Should().NotBeNull();
                result.Offset.Year.Should().Be(2000);
                result.Offset.Month.Should().Be(3);
                result.Offset.Day.Should().Be(1);
            }

            [Fact]
            public void ParsesDateTimeOffsetCorrectly()
            {
                const string dtoffsetString = "2000-03-01T00:00:00+00:00";
                var node = new TestNode(new Dictionary<string, object> { { "Offset", dtoffsetString } });
                var record = Substitute.For<IRecord>();
                record.Keys.Returns(new List<string> { "X" });
                record["X"].Returns(node);

                var result = record.Parse<ClassWithDateTime>(GraphClient);
                result.Should().NotBeNull();
                result.Offset.Year.Should().Be(2000);
                result.Offset.Month.Should().Be(3);
                result.Offset.Day.Should().Be(1);
            }

            [Fact]
            public void ParsesClassWithBaseClassProperly()
            {
                var node = new TestNode(new Dictionary<string, object> { { "Base", "Base" }, {"Derived", "Derived"} });
                var record = Substitute.For<IRecord>();
                record.Keys.Returns(new List<string> { "X" });
                record["X"].Returns(node);

                var result = record.Parse<DerivedClass>(GraphClient);
                result.Should().NotBeNull();
                result.Derived.Should().Be("Derived");
                result.Base.Should().Be("Base");
            }

            [Fact]
            public void ParsesSimpleRecordCorrectly()
            {
                var record = Substitute.For<IRecord>();
                record.Keys.Returns(new List<string> {"a"});
                var node = new TestNode(new Dictionary<string, object>{{"IntValue", 1}});
                record["a"].Returns(node);

                var result = record.Parse<SimpleIntClass>(GraphClient);
                result.IntValue.Should().Be(1);
            }


            [Fact]
            public void ParsesComplicatedReturnCorrectly()
            {
                var node = new TestNode(new Dictionary<string, object> { { "IntValue", 1 } });
                var node2 = new TestNode(new Dictionary<string, object> { { "IntValue", 2 } });
                var record = Substitute.For<IRecord>();
                record.Keys.Returns(new List<string> { "SimpleIntClass", "Ints" });
                record["SimpleIntClass"].Returns(node);
                record["Ints"].Returns(new List<object> {node2});

                var result = record.Parse<ComplexClass>(GraphClient);
                result.SimpleIntClass.IntValue.Should().Be(1);
                result.Ints.Single().IntValue.Should().Be(2);
                result.Ints2.Should().BeNull();
            }
        }

        public class ParseAnonymousMethod : IClassFixture<CultureInfoSetupFixture>
        {
            [Fact]
            public void ParsesDoubleCorrectly()
            {
                var record = Substitute.For<IRecord>();
                record.Keys.Returns(new List<string> { "a" });

                record["a"].Returns(1.00);

                var result = record.ParseAnonymous(GraphClient);
                result.Should().BeOfType<string>();
                result.Should().NotBeNullOrWhiteSpace();
            }

            [Fact]
            public void ParsesIntCorrectly()
            {
                var record = Substitute.For<IRecord>();
                record.Keys.Returns(new List<string> { "a" });

                record["a"].Returns(1);

                var result = record.ParseAnonymous(GraphClient);
                result.Should().BeOfType<string>();
                result.Should().NotBeNullOrWhiteSpace();
            }

            [Fact]
            public void ParsesNullCorrectly()
            {
                var record = Substitute.For<IRecord>();
                record.Keys.Returns(new List<string> { "a" });

                record["a"].Returns(null);

                var result = record.ParseAnonymous(GraphClient); 
                result.Should().BeOfType<string>();
                result.Should().NotBeNullOrWhiteSpace();
                result.Should().Be("{\"columns\":[\"a\"],\"data\":[[null]]}");
            }

            [Fact]
            public void ParsesBoolCorrectly()
            {
                var record = Substitute.For<IRecord>();
                record.Keys.Returns(new List<string> { "a" });
                
                record["a"].Returns(false);

                var result = record.ParseAnonymous(GraphClient);
                result.Should().BeOfType<string>();
                result.Should().NotBeNullOrWhiteSpace();
                result.Should().Be("{\"columns\":[\"a\"],\"data\":[[false]]}");
            }

            [Fact]
            public void ParsesSimpleRecordCorrectly()
            {
                var record = Substitute.For<IRecord>();
                record.Keys.Returns(new List<string> { "a" });
                var node = new TestNode(new Dictionary<string, object> { { "IntValue", 1 } });
                record["a"].Returns(node);

                var result = record.ParseAnonymous(GraphClient);
                result.Should().BeOfType<string>();
                result.Should().NotBeNullOrWhiteSpace();
            }


            [Fact]
            public void ParsesComplicatedReturnCorrectly()
            {
                var node = new TestNode(new Dictionary<string, object> { { "IntValue", 1 } });
                var node2 = new TestNode(new Dictionary<string, object> { { "IntValue", 2 } });
                var record = Substitute.For<IRecord>();
                record.Keys.Returns(new List<string> { "SimpleIntClass", "Ints" });
                record["SimpleIntClass"].Returns(node);
                record["Ints"].Returns(new List<object> { node2 });

                var result = record.ParseAnonymous(GraphClient);
                result.Should().NotBeNullOrWhiteSpace();
                result.Should().Contain("\"IntValue\":1");
                result.Should().Contain("\"IntValue\":2");
            }

            [Fact]
            public void ParsesPathsCorrectly()
            {
                var node1 = new TestNode(new Dictionary<string, object> { { "IntValue", 1 } }, 1);
                var node2 = new TestNode(new Dictionary<string, object> { { "IntValue", 2 } }, 2);
                var node3 = new TestNode(new Dictionary<string, object> { { "IntValue", 3 } }, 3);

                var path = Substitute.For<IPath>();
                path.Start.Returns(node1);
                path.End.Returns(node3);
                path.Nodes.Returns(new[] {node1, node2, node3});
                path.Relationships.Returns(new[]
                {
                    new TestRelationship(new Dictionary<string, object> { { "IntValue", 1 } }, startId: 1, endId: 2),
                    new TestRelationship(new Dictionary<string, object> { { "IntValue", 2 } }, startId: 2, endId: 3)
                });

                var record = Substitute.For<IRecord>();
                record.Keys.Returns(new List<string> { "path" });
                record["path"].Returns(path);

                var result = record.ParseAnonymous(GraphClient);
                result.Should().NotBeNullOrWhiteSpace();
            }

            [Fact]
            public void ParsesNeo4jAnonymousTypesCorrectly()
            {
                var expectedContent = "{\"columns\":[\"a\"],\"data\":[[{\"IntValue\":3}]]}";
                var list = new List<KeyValuePair<string, object>>();
                list.Add(new KeyValuePair<string, object>("IntValue", 3));

                var record = Substitute.For<IRecord>();
                record.Keys.Returns(new List<string> { "a" });
                record["a"].Returns(list);

                var result = record.ParseAnonymous(GraphClient);
                result.Should().NotBeNull();
                result.Should().Be(expectedContent);
            }

            [Fact]
            public void ParsesComplicatedReturnCorrectly2()
            {
                var node = new TestNode(new Dictionary<string, object> { { "IntValue", 1 } });
                var node2 = new TestNode(new Dictionary<string, object> { { "IntValue", 2 } });
                var record = Substitute.For<IRecord>();
                record.Keys.Returns(new List<string> { "SimpleIntClass", "Ints" });
                record["SimpleIntClass"].Returns(node);
                record["Ints"].Returns(new List<object>());

                var result = record.ParseAnonymous(GraphClient);
                result.Should().NotBeNullOrWhiteSpace();
                result.Should().Contain("\"IntValue\":1");
                result.Should().Contain("[]");
            }
        }

        public class ToJsonStringMethod : IClassFixture<CultureInfoSetupFixture>
        {
            private class Foo
            {
                public string BarStr { get; set; }
                public int BarInt { get; set; }
            }

            private class FooWithList : Foo
            {
                public IEnumerable<string> BarList { get; set; }
            }

            private class FooWithListOfComplexType : Foo
            {
                public IEnumerable<ComplexType> BarList { get; set; }
            }

            private class ComplexType
            {
                public string Value { get; set; }
            }

            [Fact]
            public void ReturnsCorrectJsonForDictionary()
            {
                IDictionary<string, object> o = new Dictionary<string, object>();
                o.Add("a", new Foo {BarInt = 1, BarStr = "bar"});

                var result = o.ToJsonString(false, false, false);
                result.Should().NotBeNullOrWhiteSpace();
                result.Should().Be("{ \"a\":{\"BarStr\":\"bar\",\"BarInt\":1} }");
            }

            [Fact]
            public void ReturnsCorrectJsonForDictionaryWithListInternal()
            {
                IDictionary<string, object> o = new Dictionary<string, object>();
                o.Add("a", new FooWithList { BarInt = 1, BarStr = "bar", BarList = new System.Collections.Generic.List<string>{"x", "y", "z"}});

                var result = o.ToJsonString(false, false, false);
                result.Should().NotBeNullOrWhiteSpace();
                result.Should().Be("{ \"a\":{\"BarList\":[\"x\",\"y\",\"z\"],\"BarStr\":\"bar\",\"BarInt\":1} }");
            }

            [Fact]
            public void ReturnsCorrectJsonForDictionaryWithComplexListInternal()
            {
                IDictionary<string, object> o = new Dictionary<string, object>();
                o.Add("a", new FooWithListOfComplexType { BarInt = 1, BarStr = "bar", BarList = new System.Collections.Generic.List<ComplexType> { new ComplexType{Value = "y"}, new ComplexType{Value = "z"} } });

                var result = o.ToJsonString(false, false, false);
                result.Should().NotBeNullOrWhiteSpace();
                result.Should().Be("{ \"a\":{\"BarList\":[{\"Value\":\"y\"},{\"Value\":\"z\"}],\"BarStr\":\"bar\",\"BarInt\":1} }");
            }

            [Theory]
            [InlineData(false, false, false)]
            [InlineData(true, false, false)]
            [InlineData(false, true, false)]
            [InlineData(false, false, true)]
            [InlineData(true, true, false)]
            [InlineData(false, true, true)]
            [InlineData(true, true, true)]
            public void ReturnsNull_WhenInputIsNull(bool inSet, bool isNested, bool isNestedInList)
            {
                object o = null;
                var result = o.ToJsonString(inSet, isNested, isNestedInList);
                result.Should().BeNull();
            }

            [Fact]
            public void Poco_DeserializedCorrectly()
            {
                var poco = new Poco {Foo = "foo", Bar = 1};
                var result = poco.ToJsonString(false, false, false);

                result.Replace(" ", "").Should().Be("{\"Foo\":\"foo\",\"Bar\":1}");
            }

            [Fact]
            public void ListPocoNotNested()
            {
                var listOfPoco = new List<Poco>
                {
                    new Poco {Foo = "foo1", Bar = 1},
                    new Poco {Foo = "foo2", Bar = 2}
                };

                var result = listOfPoco.ToJsonString(false, false, false);
                result.Replace(" ", "").Should().Be("[{\"data\":{\"Foo\":\"foo1\",\"Bar\":1}},{\"data\":{\"Foo\":\"foo2\",\"Bar\":2}}]");
            }

            private class Poco
            {
                public string Foo { get; set; }
                public int Bar { get; set; }
            }
        }
    }
}