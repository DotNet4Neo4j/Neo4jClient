using System;
using NUnit.Framework;
using Neo4jClient.Serializer;

namespace Neo4jClient.Test.Serializer
{
    [TestFixture]
    public class CustomJsonSerializerTests
    {
        [Test]
        public void SerializeTimeZoneInfo()
        {
            // Arrange
            var serializer = new CustomJsonSerializer();
            const string ausEasternStandardTime = "AUS Eastern Standard Time";
            var timeZoneData = TimeZoneInfo.FindSystemTimeZoneById(ausEasternStandardTime);

            // Act
            var result = serializer.Serialize(timeZoneData);

            // Assert
            Assert.AreEqual(ausEasternStandardTime, result.Replace("\"",""));
        }

        public class TimeZoneModel
        {
            public TimeZoneInfo Foo { get; set; }
        }

        public enum Gender{Male, Female, Unknown}

        public class TestFoo
        {
            public Gender Gender { get; set; }
            public Gender? GenderNullable { get; set; }
        }

        [Test]
        public void SerializeEnumToStringValues()
        {
            // Arrange
            var testClass = new TestFoo
                {
                    Gender = Gender.Female,
                    GenderNullable = Gender.Male
                };

            var serializer = new CustomJsonSerializer();
            const string expected = "{\r\n  \"Gender\": \"Female\",\r\n  \"GenderNullable\": \"Male\"\r\n}";

            // Act
            var result = serializer.Serialize(testClass);

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}
