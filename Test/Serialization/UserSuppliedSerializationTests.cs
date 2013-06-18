using System;
using System.Globalization;
using NUnit.Framework;
using Neo4jClient.Deserializer;
using Neo4jClient.Serializer;
using Newtonsoft.Json;

namespace Neo4jClient.Test.Serialization
{
    [TestFixture]
    public class UserSuppliedSerializationTests
    {
        public class TestValue
        {
            public char A { get; set; }
            public char B { get; set; }
        }

        public class TestModel
        {
            public TestValue CustomValue { get; set; }
        }

        public class TestValueConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var typedValue = (TestValue)value;
                writer.WriteValue(typedValue.A + typedValue.B.ToString(CultureInfo.InvariantCulture));
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var rawValue = reader.Value.ToString();
                return new TestValue
                    {
                        A = rawValue[0],
                        B = rawValue[1]
                    };
            }

            public override bool CanConvert(Type objectType)
            {
                return typeof(TestValue) == objectType;
            }
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/89")]
        public void ShouldSerializeCustomValueWithCustomJsonConverter()
        {
            //Arrange
            var serializer = new CustomJsonSerializer
                {
                    JsonConverters = new []{new TestValueConverter()}
                };

            var model = new TestModel
                {
                    CustomValue = new TestValue
                        {
                            A = 'o',
                            B = 'p'
                        }
                };

            //Act
            var rawResult = serializer.Serialize(model);

            //Assert
            const string expectedRawOutput =
            "{\r\n  \"CustomValue\": \"op\"\r\n}";

            Assert.AreEqual(expectedRawOutput, rawResult);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/89")]
        public void ShouldDeserializeCustomValueWithCustomJsonConverter()
        {
            //Arrange
            const string rawInput =
            "{\r\n  \"CustomValue\": \"op\"\r\n}";

            var serializer = new CustomJsonDeserializer(new []{new TestValueConverter()});

            //Act
            var model = serializer.Deserialize<TestModel>(rawInput);

            //Assert
            Assert.NotNull(model, "Deserialization failed.");
            Assert.NotNull(model.CustomValue, "Model.CustomValue is unexpectedly null.");
            Assert.AreEqual('o', model.CustomValue.A);
            Assert.AreEqual('p', model.CustomValue.B);
        }
    }
}
