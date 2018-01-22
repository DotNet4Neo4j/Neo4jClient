using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Transactions;
using FluentAssertions;
using Newtonsoft.Json.Serialization;
using NSubstitute;
using Xunit;
using Neo4jClient.ApiModels.Gremlin;
using Neo4jClient.Serialization;
using Neo4jClient.Test.Fixtures;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Neo4jClient.Test.Serialization
{

    public class CustomJsonDeserializerTests 
    {
        public class BoltGraphClientVersion : IClassFixture<CultureInfoSetupFixture>
        {
            private class NestedClass
            {
                public int IntValue { get; set; }
            }

            private class ClassWithClassProperty
            {
                public string SimpleString { get; set; }
                public NestedClass NestedClass { get; set; }
            }

            private class ClassWithClassPropertyJsonSerializer : JsonConverter
            {
                public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
                {
                    throw new NotImplementedException();
                }

                public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
                {
                    if (objectType != typeof(ClassWithClassProperty))
                        return null;

                    //Load our object
                    var jObject = JObject.Load(reader);

                    var data = jObject.Property("data");
                    if (data != null)
                        jObject = data.Value as JObject;

                    //Get the InsString token into a temp var
                    var token = jObject.Property(nameof(NestedClass)).Value;
                    //Remove it so it's not deserialized by Json.NET
                    jObject.Remove(nameof(NestedClass));

                    //Get the dictionary ourselves and deserialize
                    var nestedClass = JsonConvert.DeserializeObject<NestedClass>(token.ToString());

                    //The output
                    var output = JsonConvert.DeserializeObject<ClassWithClassProperty>(jObject.ToString());

                    //Add our dictionary
                    output.NestedClass = nestedClass;

                    //return
                    return output;
                }

                public override bool CanConvert(Type objectType)
                {
                    return objectType == typeof(ClassWithClassProperty);
                }
            }


        }

        public class GraphClientVersion : IClassFixture<CultureInfoSetupFixture>
        {
            [Theory]
            [InlineData("", null)]
            [InlineData("rekjre", null)]
            [InlineData("/Date(abcs)/", null)]
            [InlineData("/Date(abcs+0000)/", null)]
            [InlineData("/Date(1315271562384)/", "2011-09-06T01:12:42.3840000+00:00")]
            [InlineData("/Date(1315271562384+0000)/", "2011-09-06T01:12:42.3840000+00:00")]
            [InlineData("/Date(1315271562384+0200)/", "2011-09-06T03:12:42.3840000+02:00")]
            [InlineData("/Date(1315271562384+1000)/", "2011-09-06T11:12:42.3840000+10:00")]
            [InlineData("/Date(-2187290565386+0000)/", "1900-09-09T03:17:14.6140000+00:00")]
            [InlineData("2011-09-06T01:12:42+10:00", "2011-09-06T01:12:42.0000000+10:00")]
            [InlineData("2011-09-06T01:12:42+09:00", "2011-09-06T01:12:42.0000000+09:00")]
            [InlineData("2011-09-06T01:12:42-07:00", "2011-09-06T01:12:42.0000000-07:00")]
            [InlineData("2011-09-06T01:12:42+00:00", "2011-09-06T01:12:42.0000000+00:00")]
            [InlineData("2012-08-31T10:11:00.3642578+10:00", "2012-08-31T10:11:00.3642578+10:00")]
            [InlineData("2012-08-31T00:11:00.3642578+00:00", "2012-08-31T00:11:00.3642578+00:00")]
            [InlineData("2011/09/06 10:11:00 +10:00", "2011-09-06T10:11:00.0000000+10:00")]
            [InlineData("2011/09/06 10:11:00 AM +10:00", "2011-09-06T10:11:00.0000000+10:00")]
            [InlineData("2011/09/06 12:11:00 PM +10:00", "2011-09-06T12:11:00.0000000+10:00")]
            public void DeserializeShouldPreserveOffsetValuesUsingIso8601Format(string input, string expectedResult)
            {
                var culturesToTest = new[] {"en-AU", "en-US"};

                foreach (var cultureName in culturesToTest)
                {
                    // Arrange
                    var deserializer = new CustomJsonDeserializer(GraphClient.DefaultJsonConverters, new CultureInfo(cultureName));
                    var content = string.Format("{{'Foo':'{0}'}}", input);

                    // Act
                    var result = deserializer.Deserialize<DateTimeOffsetModel>(content);

                    // Assert
                    if (expectedResult == null)
                        Assert.Null(result.Foo);
                    else
                    {
                        Assert.NotNull(result.Foo);
                        Assert.Equal(expectedResult, result.Foo.Value.ToString("o", CultureInfo.InvariantCulture));
                    }
                }
            }

            [Theory]
            [InlineData("", null, DateTimeKind.Utc)]
            [InlineData("rekjre", null, DateTimeKind.Utc)]
            [InlineData("/Date(abcs)/", null, DateTimeKind.Utc)]
            [InlineData("/Date(1315271562384)/", "2011-09-06T01:12:42.3840000Z", DateTimeKind.Utc)]
            [InlineData("/Date(-2187290565386)/", "1900-09-09T03:17:14.6140000Z", DateTimeKind.Utc)]
            [InlineData("2015-07-27T22:30:35Z", "2015-07-27T22:30:35.0000000Z", DateTimeKind.Utc)]
            [InlineData("2011-09-06T01:12:42", "2011-09-06T01:12:42.0000000", DateTimeKind.Unspecified)]
            [InlineData("2012-08-31T10:11:00.3642578", "2012-08-31T10:11:00.3642578", DateTimeKind.Unspecified)]
            [InlineData("2011/09/06 10:11:00", "2011-09-06T10:11:00.0000000", DateTimeKind.Unspecified)]
            [InlineData("2011/09/06 10:11:00 AM", "2011-09-06T10:11:00.0000000", DateTimeKind.Unspecified)]
            [InlineData("2011/09/06 12:11:00 PM", "2011-09-06T12:11:00.0000000", DateTimeKind.Unspecified)]
            public void DeserializeShouldPreserveDateValuesUsingIso8601Format(string input, string expectedResult, DateTimeKind expectedKind)
            {
                var culturesToTest = new[] {"en-AU", "en-US", "nb-NO"};

                foreach (var cultureName in culturesToTest)
                {
                    // Arrange
                    var deserializer = new CustomJsonDeserializer(GraphClient.DefaultJsonConverters, new CultureInfo(cultureName));
                    var content = string.Format("{{'Foo':'{0}'}}", input);

                    // Act
                    var result = deserializer.Deserialize<DateTimeModel>(content);

                    // Assert
                    if (expectedResult == null)
                        Assert.Null(result.Foo);
                    else
                    {
                        Assert.NotNull(result.Foo);
                        Assert.Equal(expectedKind, result.Foo.Value.Kind);
                        Assert.Equal(expectedResult, result.Foo.Value.ToString("o", CultureInfo.InvariantCulture));
                    }
                }
            }

            [Fact]
            public void DeserializeTimeZoneInfoWithDefaultJsonConverters()
            {
                // Arrange
                var deserializer = new CustomJsonDeserializer(GraphClient.DefaultJsonConverters);
                const string ausEasternStandardTime = "AUS Eastern Standard Time";
                var content = string.Format("{{'Foo':'{0}'}}", ausEasternStandardTime);

                // Act
                var result = deserializer.Deserialize<TimeZoneModel>(content);

                // Assert
                Assert.NotNull(result.Foo);
                Assert.Equal(TimeZoneInfo.FindSystemTimeZoneById(ausEasternStandardTime).DisplayName,
                    result.Foo.DisplayName);
            }

            [Theory]
            [InlineData("400.09:03:02.0100000", 400, 9, 3, 2, 10)]
            [InlineData("09:03:02.0100000", 0, 9, 3, 2, 10)]
            [InlineData("09:03:02.0010000", 0, 9, 3, 2, 1)]
            [InlineData("09:03:11.9990000", 0, 9, 3, 2, 9999)]
            [InlineData("400.09:03:02", 400, 9, 3, 2, 0)]
            [InlineData("09:03:02", 0, 9, 3, 2, 0)]
            public void DeserializeTimeSpanWithDefaultJsonConverters(string value, int days, int hours, int minutes, int seconds, int milliseconds)
            {
                // Arrange
                var deserializer = new CustomJsonDeserializer(GraphClient.DefaultJsonConverters);
                var content = string.Format("{{'Foo':'{0}'}}", value);

                // Act
                var result = deserializer.Deserialize<TimeSpanModel>(content);

                // Assert
                Assert.NotNull(result.Foo);
                Assert.Equal(new TimeSpan(days, hours, minutes, seconds, milliseconds), result.Foo);
            }

            public class DateTimeOffsetModel
            {
                public DateTimeOffset? Foo { get; set; }
            }

            public class DateTimeModel
            {
                public DateTime? Foo { get; set; }
            }

            public class TimeZoneModel
            {
                public TimeZoneInfo Foo { get; set; }
            }

            public class TimeSpanModel
            {
                public TimeSpan Foo { get; set; }
            }

            [Fact]
            public void DeserializeShouldConvertTableCapResponseToGremlinTableCapResponse()
            {
                // Arrange
                var deserializer = new CustomJsonDeserializer(GraphClient.DefaultJsonConverters);
                const string content = @"{
                              ""columns"" : [ ""ColumnA"" ],
                              ""data"" : [ [ ""DataA"" ], [ ""DataB"" ] ]
                            }";

                // Act
                var result = deserializer.Deserialize<GremlinTableCapResponse>(content);
                var data = result.Data.SelectMany(d => d).ToArray();

                // Assert
                Assert.True(result.Columns.Any(c => c == "ColumnA"));
                Assert.True(data.Any(d => d == "DataA"));
                Assert.True(data.Any(d => d == "DataB"));
            }

            public class EnumModel
            {
                [JsonProperty]
                public Gender Gender { get; set; }

                [JsonProperty]
                public Gender? GenderNullable { get; set; }
            }

            public enum Gender
            {
                Male,
                Female,
                Unknown
            }

            public class EnumerableModel
            {
                [JsonProperty]
                public IEnumerable<Guid> Guids { get; set; }
            }

            [Fact]
            public void ReadJsonCanMapNullableEnumsToEnum()
            {
                // Arrange
                var conv = new NullableEnumValueConverter();
                var jsonReader = Substitute.For<JsonReader>();
                jsonReader.Value.ReturnsForAnyArgs("Female");

                // Act
                var result = conv.ReadJson(jsonReader, typeof(Gender?), null, null);
                var expected = (Gender?) Gender.Female;

                // Assert
                Assert.Equal(expected, result);
            }

            private class DateTimeDeserializer : DateTimeConverterBase
            {
                public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
                {
                    if (value.GetType().IsAssignableFrom(typeof(DateTime)))
                        writer.WriteValue(((DateTime) value).Ticks);
                }

                public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
                {
                    if (objectType.IsAssignableFrom(typeof(DateTime)))
                        return new DateTime(long.Parse(reader.Value.ToString()));

                    return DateTime.MinValue;
                }
            }

            [Fact]
            public void DeserializeShouldUseCustomSerializerBeforeDefault()
            {
                var deserializer = new CustomJsonDeserializer(new List<JsonConverter>(GraphClient.DefaultJsonConverters) {new DateTimeDeserializer()});
                var expected = new DateTime(2000, 1, 1).Date;

                var deserializeDateTime = deserializer.Deserialize<DateTimeModel>("{\"Foo\":\"630822816000000000\"}");

                Assert.NotNull(deserializeDateTime.Foo);
                Assert.Equal(expected, deserializeDateTime.Foo.Value);
            }


            [Theory]
            [InlineData("{\"Gender\": \"Female\"}", Gender.Female)]
            [InlineData("{\"Gender\": \"1\"}", Gender.Female)]
            public void DeserializeEnumFromStringWithDefaultJsonConverters(string content, Gender expectedGender)
            {
                // Arrange
                var deserializer = new CustomJsonDeserializer(GraphClient.DefaultJsonConverters);

                // Act
                var deserialziedGender = deserializer.Deserialize<EnumModel>(content);

                // Assert
                Assert.NotNull(deserialziedGender);
                Assert.Equal(deserialziedGender.Gender, expectedGender);
            }

            [Theory]
            [InlineData("{\"GenderNullable\": \"Female\"}", Gender.Female)]
            [InlineData("{\"GenderNullable\": \"1\"}", Gender.Female)]
            public void DeserializeNullableEnumFromStringWithDefaultJsonConverters(string content, Gender? expectedGender)
            {
                // Arrange
                var deserializer = new CustomJsonDeserializer(GraphClient.DefaultJsonConverters);

                // Act
                var result = deserializer.Deserialize<EnumModel>(content);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(expectedGender, result.GenderNullable);
            }

            [Fact]
            public void DeserializeGuidWithDefaultJsonConverters()
            {
                //Arrage
                var myGuid = Guid.NewGuid();
                var foo = new EnumerableModel {Guids = new List<Guid> {myGuid}};

                // Act
                var customSerializer = new CustomJsonSerializer {JsonConverters = GraphClient.DefaultJsonConverters};
                var testStr = customSerializer.Serialize(foo);

                var customDeserializer = new CustomJsonDeserializer(GraphClient.DefaultJsonConverters);
                var result = customDeserializer.Deserialize<EnumerableModel>(testStr);

                // Assert
                Assert.Equal(myGuid, result.Guids.First());
            }

            [Theory]
            [InlineData("[ \"Male\", \"Female\", \"Unknown\" ]", new[] {Gender.Male, Gender.Female, Gender.Unknown})]
            public void DeserializeIEnumerableOfEnumWithDefaultJsonConverters(string content, Gender[] genders)
            {
                // Act
                var deserializer = new CustomJsonDeserializer(GraphClient.DefaultJsonConverters);

                // Assert
                var result = deserializer.Deserialize<List<Gender>>(content);
                result.Should().BeEquivalentTo(genders);
            }


            public class ModelWithDecimal
            {
                public decimal MyDecimalValue { get; set; }
            }

            [Fact]
            //[Description("https://bitbucket.org/Readify/neo4jclient/issue/149/deserialization-of-type-decimal-fails-when")]
            public void DecimalDeserializationIsCultureIndependent()
            {
                //SetupFixture defaults culture info so culture-dependent tests should preserve culture state
                var currentNumberDecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

                try
                {
                    //Arange
                    var newCulture = new CultureInfo(CultureInfo.CurrentCulture.Name) {NumberFormat = {NumberDecimalSeparator = ","}};
                    Thread.CurrentThread.CurrentCulture = newCulture;

                    const string serializedModelWithDecimal = "{'data':{'MyDecimalValue':0.5}}";

                    //Act
                    var customDeserializer = new CustomJsonDeserializer(GraphClient.DefaultJsonConverters);
                    var result = customDeserializer.Deserialize<ModelWithDecimal>(serializedModelWithDecimal);

                    //Assert
                    Assert.Equal(0.5m, result.MyDecimalValue);
                }
                finally
                {
                    CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator = currentNumberDecimalSeparator;
                }
            }

            public class CamelModel
            {
                public string FirstName { get; set; }
                public Gender Gender { get; set; }
                public DateTimeOffset DateOfBirth { get; set; }
                public string S { get; set; }
            }

            [Fact]
            public void CamelCaseTest()
            {
                //setup
                var model = new CamelModel
                {
                    FirstName = "first",
                    DateOfBirth = new DateTime(1980, 4, 1),
                    Gender = Gender.Male,
                    S = "short property"
                };
                var serializer = new CustomJsonSerializer();
                serializer.JsonContractResolver = new CamelCasePropertyNamesContractResolver();
                var st = serializer.Serialize(model);

                //act
                var deserializer = new CustomJsonDeserializer(GraphClient.DefaultJsonConverters, resolver: (DefaultContractResolver) serializer.JsonContractResolver);
                var output = deserializer.Deserialize<CamelModel>(st);

                //assert
                AssertCamelModel(model, output);
            }

            private void AssertCamelModel(CamelModel expected, CamelModel actual)
            {
                Assert.Equal(expected.FirstName, actual.FirstName);
                Assert.Equal(expected.DateOfBirth, actual.DateOfBirth);
                Assert.Equal(expected.Gender, actual.Gender);
            }


            [Fact]
            public void CamelCaseListTest()
            {
                //setup
                var model = new List<CamelModel>
                {
                    new CamelModel
                    {
                        FirstName = "first",
                        DateOfBirth = new DateTime(1980, 4, 1),
                        Gender = Gender.Male
                    },
                    new CamelModel
                    {
                        FirstName = "second",
                        DateOfBirth = new DateTime(1981, 4, 1),
                        Gender = Gender.Female
                    }
                };

                var serializer = new CustomJsonSerializer();
                serializer.JsonContractResolver = new CamelCasePropertyNamesContractResolver();
                var st = serializer.Serialize(model);

                //act
                var deserializer = new CustomJsonDeserializer(GraphClient.DefaultJsonConverters, resolver: (DefaultContractResolver) serializer.JsonContractResolver);
                var output = deserializer.Deserialize<List<CamelModel>>(st);

                //assert
                AssertCamelModel(model[0], output[0]);
                AssertCamelModel(model[1], output[1]);
            }
        }
    }
}
