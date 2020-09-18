using System;
using System.Globalization;
using Neo4jClient.Serialization;
using Newtonsoft.Json;
using Xunit;

namespace Neo4jClient.Tests.Serialization
{
    
    public class UserSuppliedSerializationTests : IClassFixture<CultureInfoSetupFixture>
    {
        public UserSuppliedSerializationTests()
        {
            CultureInfoSetupFixture.SetDeterministicCulture();
        }

        public class TestValueA
        {
            public char A { get; set; }
            public char B { get; set; }
        }

        public class TestModelA
        {
            public TestValueA CustomValue { get; set; }
        }

        public class TestValueAConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var typedValue = (TestValueA)value;
                writer.WriteValue(typedValue.A + typedValue.B.ToString(CultureInfo.InvariantCulture));
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var rawValue = reader.Value.ToString();
                return new TestValueA
                {
                    A = rawValue[0],
                    B = rawValue[1]
                };
            }

            public override bool CanConvert(Type objectType)
            {
                return typeof(TestValueA) == objectType;
            }
        }

        [System.ComponentModel.TypeConverter(typeof(TestValueBTypeConverter))]
        public class TestValueB
        {
            public char A { get; set; }
            public char B { get; set; }
        }

        public class TestModelB
        {
            public TestValueB CustomValue { get; set; }
        }

        public class TestModelC
        {
            public System.Drawing.Point MyPoint { get; set; }
        }

        public class TestValueBTypeConverter : System.ComponentModel.TypeConverter
        {
            public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof (string);
            }

            public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext context, Type destinationType)
            {
                return destinationType == typeof (string);
            }

            public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value == null) return null;
                var valueAsString = value.ToString();
                return new TestValueB {A = valueAsString[0], B = valueAsString[1]};
            }

            public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                var typedValue = (TestValueB)value;
                return typedValue.A + typedValue.B.ToString(CultureInfo.InvariantCulture);
            }
        }

        [Fact]
        //[Description("https://bitbucket.org/Readify/neo4jclient/issue/89")]
        public void ShouldSerializeCustomValueWithCustomJsonConverter()
        {
            //Arrange
            var serializer = new CustomJsonSerializer
                {
                    JsonConverters = new []{new TestValueAConverter()}
                };

            var model = new TestModelA
                {
                    CustomValue = new TestValueA
                        {
                            A = 'o',
                            B = 'p'
                        }
                };

            //Act
            var rawResult = serializer.Serialize(model);

            //Assert
            string expectedRawOutput =
            $"{{{Environment.NewLine}  \"CustomValue\": \"op\"{Environment.NewLine}}}";

            Assert.Equal(expectedRawOutput, rawResult);
        }

        [Fact]
        //[Description("https://bitbucket.org/Readify/neo4jclient/issue/89")]
        public void ShouldDeserializeCustomValueWithCustomJsonConverter()
        {
            //Arrange
            string rawInput =
            $"{{{Environment.NewLine}  \"CustomValue\": \"op\"{Environment.NewLine}}}";

            var serializer = new CustomJsonDeserializer(new []{new TestValueAConverter()});

            //Act
            var model = serializer.Deserialize<TestModelA>(rawInput);

            //Assert
            Assert.NotNull(model);
            Assert.NotNull(model.CustomValue);
            Assert.Equal('o', model.CustomValue.A);
            Assert.Equal('p', model.CustomValue.B);
        }

        [Fact]
        //[Description("https://bitbucket.org/Readify/neo4jclient/issue/89")]
        public void ShouldSerializeCustomTypeThatHasTypeConverterUsingTypeConverterBasedJsonConverter()
        {
            //Arrange
            var serializer = new CustomJsonSerializer
            {
                JsonConverters = new[] { new TypeConverterBasedJsonConverter() }
            };

            var model = new TestModelB
            {
                CustomValue = new TestValueB
                {
                    A = 'o',
                    B = 'p'
                }
            };

            //Act
            var rawResult = serializer.Serialize(model);

            //Assert
            string expectedRawOutput =
            $"{{{Environment.NewLine}  \"CustomValue\": \"op\"{Environment.NewLine}}}";

            Assert.Equal(expectedRawOutput, rawResult);
        }

        [Fact]
        //[Description("https://bitbucket.org/Readify/neo4jclient/issue/89")]
        public void ShouldDeserializeCustomTypeThatHasTypeConverterUsingTypeConverterBasedJsonConverter()
        {
            //Arrange
            string rawInput =
            $"{{{Environment.NewLine}  \"CustomValue\": \"op\"{Environment.NewLine}}}";

            var serializer = new CustomJsonDeserializer(new JsonConverter[]{new TypeConverterBasedJsonConverter()});

            //Act
            var model = serializer.Deserialize<TestModelB>(rawInput);

            //Assert
            Assert.NotNull(model);
            Assert.NotNull(model.CustomValue);
            Assert.Equal('o', model.CustomValue.A);
            Assert.Equal('p', model.CustomValue.B);
        }

        [Fact]
        //[Description("https://bitbucket.org/Readify/neo4jclient/issue/89")]
        public void ShouldSerializeBuiltInTypeThatHasTypeConverterUsingTypeConverterBasedJsonConverter()
        {
            //Arrange
            var serializer = new CustomJsonSerializer
            {
                JsonConverters = new[] { new TypeConverterBasedJsonConverter() }
            };

            var model = new TestModelC
            {
                MyPoint = new System.Drawing.Point(100, 200)
            };

            //Act
            var rawResult = serializer.Serialize(model);

            //Assert
            string expectedRawOutput = $"{{{Environment.NewLine}  \"MyPoint\": \"100, 200\"{Environment.NewLine}}}";

            Assert.Equal(expectedRawOutput, rawResult);
        }

        [Fact]
        //[Description("https://bitbucket.org/Readify/neo4jclient/issue/89")]
        public void ShouldDeserializeBuiltInTypeThatHasTypeConverterUsingTypeConverterBasedJsonConverter()
        {
            //Arrange
            string rawInput = $"{{{Environment.NewLine}  \"MyPoint\": \"100, 200\"{Environment.NewLine}}}";

            var serializer = new CustomJsonDeserializer(new JsonConverter[] { new TypeConverterBasedJsonConverter() });

            //Act
            var model = serializer.Deserialize<TestModelC>(rawInput);

            //Assert
            Assert.NotNull(model);
            Assert.NotNull(model.MyPoint);
            Assert.Equal(new System.Drawing.Point(100, 200), model.MyPoint);
        }


        public class TestNodeWithSomeNeo4jIgnoreAttributes
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
        //[Description("test https part of https://github.com/Readify/Neo4jClient/issues/336  https://github.com/Readify/Neo4jClient/pull/337  - see BoltGraphClientTests for bolt part")]
        public void JsonSerializerShouldNotSerializeNeo4JIgnoreAttribute()
        {
            // Arrange
            var testNode = new TestNodeWithSomeNeo4jIgnoreAttributes { Text = "foo", TextIgnore = "fooignore", TestInt = 42, TestNeo4jIntIgnore = 42, TestJsonIntIgnore = 42 };
            var serializer = new CustomJsonSerializer
            {
                NullHandling = NullValueHandling.Ignore,
                JsonConverters = GraphClient.DefaultJsonConverters
            };

            // Act
            var result = serializer.Serialize(testNode);

            const string expectedValue = "{\r\n  \"Text\": \"foo\",\r\n  \"TestInt\": 42\r\n}";

            // Assert
            Assert.Equal(expectedValue, result);
        }
    }
}
