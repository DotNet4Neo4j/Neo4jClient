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

        [Test]
        public void SerializeTimeSpan()
        {
            // Arrange
            var serializer = new CustomJsonSerializer();
            var value = new TimeSpan(400, 13, 3, 2,10);
            var model = new TimeSpanModel
                {
                    Foo = value
                };

            // Act
            var result = serializer.Serialize(model.Foo);

            // Assert
            Assert.AreEqual("400.13:03:02.0100000", result.Replace("\"", ""));
        }

        public class TimeZoneModel
        {
            public TimeZoneInfo Foo { get; set; }
        }

        public class TimeSpanModel
        {
            public TimeSpan Foo { get; set; }
        }
    }
}
