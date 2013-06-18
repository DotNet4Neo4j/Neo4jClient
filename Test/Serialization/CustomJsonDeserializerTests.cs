using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using NSubstitute;
using NUnit.Framework;
using Neo4jClient.ApiModels.Gremlin;
using Neo4jClient.Deserializer;
using Neo4jClient.Serializer;
using Newtonsoft.Json;

namespace Neo4jClient.Test.Serialization
{
    [TestFixture]
    public class CustomJsonDeserializerTests
    {
        [Test]
        [TestCase("", null)]
        [TestCase("rekjre", null)]
        [TestCase("/Date(abcs)/", null)]
        [TestCase("/Date(abcs+0000)/", null)]
        [TestCase("/Date(1315271562384)/", "2011-09-06T01:12:42.3840000+00:00")]
        [TestCase("/Date(1315271562384+0000)/", "2011-09-06T01:12:42.3840000+00:00")]
        [TestCase("/Date(1315271562384+0200)/", "2011-09-06T03:12:42.3840000+02:00")]
        [TestCase("/Date(1315271562384+1000)/", "2011-09-06T11:12:42.3840000+10:00")]
        [TestCase("/Date(-2187290565386+0000)/", "1900-09-09T03:17:14.6140000+00:00")]
        [TestCase("2011-09-06T01:12:42+10:00", "2011-09-06T01:12:42.0000000+10:00")]
        [TestCase("2011-09-06T01:12:42+00:00", "2011-09-06T01:12:42.0000000+00:00")]
        [TestCase("2012-08-31T10:11:00.3642578+10:00", "2012-08-31T10:11:00.3642578+10:00")]
        [TestCase("2012-08-31T00:11:00.3642578+00:00", "2012-08-31T00:11:00.3642578+00:00")]
        [TestCase("2011/09/06 10:11:00 +10:00", "2011-09-06T10:11:00.0000000+10:00")]
        [TestCase("2011/09/06 10:11:00 AM +10:00", "2011-09-06T10:11:00.0000000+10:00")]
        [TestCase("2011/09/06 12:11:00 PM +10:00", "2011-09-06T12:11:00.0000000+10:00")]
        public void DeserializeShouldPreserveOffsetValuesUsingIso8601Format(string input, string expectedResult)
        {
            var thread = Thread.CurrentThread;
            var previousCulture = thread.CurrentCulture;

            var culturesToTest = new[] {"en-AU", "en-US", previousCulture.Name};

            try
            {
                foreach (var cultureName in culturesToTest)
                {
                    thread.CurrentCulture = thread.CurrentUICulture = new CultureInfo(cultureName);

                    // Arrange
                    var deserializer = new CustomJsonDeserializer(null);
                    var content = string.Format("{{'Foo':'{0}'}}", input);

                    // Act
                    var result = deserializer.Deserialize<DateTimeOffsetModel>(content);

                    // Assert
                    if (expectedResult == null)
                        Assert.IsNull(result.Foo);
                    else
                    {
                        Assert.IsNotNull(result.Foo);
                        Assert.AreEqual(expectedResult, result.Foo.Value.ToString("o", CultureInfo.InvariantCulture));
                    }
                }
            }
            finally
            {
                thread.CurrentCulture = thread.CurrentUICulture = previousCulture;
            }
        }

        [Test]
        public void DeserializeTimeZoneInfo()
        {
            // Arrange
            var deserializer = new CustomJsonDeserializer(new []{new TimeZoneInfoConverter()});
            const string ausEasternStandardTime = "AUS Eastern Standard Time";
            var content = string.Format("{{'Foo':'{0}'}}", ausEasternStandardTime);

            // Act
            var result = deserializer.Deserialize<TimeZoneModel>(content);

            // Assert
            Assert.IsNotNull(result.Foo);
            Assert.AreEqual(TimeZoneInfo.FindSystemTimeZoneById(ausEasternStandardTime).DisplayName,
                            result.Foo.DisplayName);
        }

        [TestCase("400.09:03:02.0100000", 400, 9,3,2,10)]
        [TestCase("09:03:02.0100000", 0, 9, 3, 2, 10)]
        [TestCase("09:03:02.0010000", 0, 9, 3, 2, 1)]
        [TestCase("09:03:11.9990000", 0, 9, 3, 2, 9999)]
        [TestCase("400.09:03:02", 400, 9, 3, 2, 0)]
        [TestCase("09:03:02", 0, 9, 3, 2, 0)]
        public void DeserializeTimeSpan(string value, int days, int hours, int minutes, int seconds, int milliseconds)
        {
            // Arrange
            var deserializer = new CustomJsonDeserializer(new JsonConverter[0]);
            var content = string.Format("{{'Foo':'{0}'}}", value);

            // Act
            var result = deserializer.Deserialize<TimeSpanModel>(content);

            // Assert
            Assert.IsNotNull(result.Foo);
            Assert.AreEqual(new TimeSpan(days, hours, minutes, seconds, milliseconds), result.Foo);
        }

        public class DateTimeOffsetModel
        {
            public DateTimeOffset? Foo { get; set; }
        }

        public class TimeZoneModel
        {
            public TimeZoneInfo Foo { get; set; }
        }

        public class TimeSpanModel
        {
            public TimeSpan Foo { get; set; }
        }

        [Test]
        public void DeserializeShouldConvertTableCapResponseToGremlinTableCapResponse()
        {
            // Arrange
            var deserializer = new CustomJsonDeserializer(new JsonConverter[0]);
            const string content = @"{
                              ""columns"" : [ ""ColumnA"" ],
                              ""data"" : [ [ ""DataA"" ], [ ""DataB"" ] ]
                            }";

            // Act
            var result = deserializer.Deserialize<GremlinTableCapResponse>(content);
            var data = result.Data.SelectMany(d => d).ToArray();

            // Assert
            Assert.IsTrue(result.Columns.Any(c => c == "ColumnA"));
            Assert.IsTrue(data.Any(d => d == "DataA"));
            Assert.IsTrue(data.Any(d => d == "DataB"));
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

        [Test]
        public void ReadJsonCanMapNullableEnumsToEnum()
        {
            // Arrange
            var conv = new NullableEnumValueConverter();
            var jsonReader = Substitute.For<JsonReader>();
            jsonReader.Value.ReturnsForAnyArgs("Female");

            // Act
            var result = conv.ReadJson(jsonReader, typeof (Gender?), null, null);
            var expected = (Gender?) Gender.Female;

            // Assert
            Assert.AreEqual(expected, result);
        }

        [Test]
        [TestCase("{\"Gender\": \"Female\"}", Gender.Female)]
        [TestCase("{\"Gender\": \"1\"}", Gender.Female)]
        public void DeserializeEnumFromString(string content, Gender expectedGender)
        {
            // Arrange
            var deserializer = new CustomJsonDeserializer(new JsonConverter[0]);

            // Act
            var deserialziedGender = deserializer.Deserialize<EnumModel>(content);

            // Assert
            Assert.IsNotNull(deserialziedGender);
            Assert.AreEqual(deserialziedGender.Gender, expectedGender);
        }

        [Test]
        [TestCase("{\"GenderNullable\": \"Female\"}", Gender.Female)]
        [TestCase("{\"GenderNullable\": \"1\"}", Gender.Female)]
        public void DeserializeNullableEnumFromString(string content, Gender? expectedGender)
        {
            // Arrange
            var deserializer = new CustomJsonDeserializer(new JsonConverter[0]);

            // Act
            var result = deserializer.Deserialize<EnumModel>(content);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedGender, result.GenderNullable);
        }

        [Test]
        public void DeserializeGuid()
        {
            //Arrage
            var myGuid = Guid.NewGuid();
            var foo = new EnumerableModel { Guids = new List<Guid> { myGuid } };

            // Act
            var customSerializer = new CustomJsonSerializer();
            var testStr = customSerializer.Serialize(foo);

            var customDeserializer = new CustomJsonDeserializer(new JsonConverter[0]);
            var result = customDeserializer.Deserialize<EnumerableModel>(testStr);

            // Assert
            Assert.AreEqual(myGuid, result.Guids.First());
        }

        [Test]
        [TestCase("[ \"Male\", \"Female\", \"Unknown\" ]", new [] { Gender.Male, Gender.Female, Gender.Unknown })]
        public void DeserializeIEnumerableOfEnum(string content, Gender[] genders)
        {
            // Act
            var deserializer = new CustomJsonDeserializer(new JsonConverter[0]);

            // Assert
            var result = deserializer.Deserialize<List<Gender>>(content);
            CollectionAssert.AreEquivalent(result, genders);
        }
    }
}
