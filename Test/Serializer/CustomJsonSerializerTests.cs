using System;
using NUnit.Framework;
using Neo4jClient.Serializer;
using RestSharp;

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
    }
}
