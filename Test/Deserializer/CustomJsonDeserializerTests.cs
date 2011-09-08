using System;
using NUnit.Framework;
using Neo4jClient.Deserializer;
using RestSharp;

namespace Neo4jClient.Test.Deserializer
{
    [TestFixture]
    public class CustomJsonDeserializerTests
    {
        [Test]
        [TestCase("/Date(1315271562384+0000)/", Result = "2011-09-06 01:12:42 +00:00")]
        [TestCase("/Date(1315271562384+0200)/", Result = "2011-09-06 03:12:42 +02:00")]
        [TestCase("/Date(1315271562384+1000)/", Result = "2011-09-06 11:12:42 +10:00")]
        public string DeserializeShouldPreserveOffsetValues(string input)
        {
            // Arrange
            var deserializer = new CustomJsonDeserializer();
            var response = new RestResponse {Content = string.Format("{{'foo':'{0}'}}", input)};

            // Act
            var result = deserializer.Deserialize<DateTimeOffsetModel>(response);

            // Assert
            return result.Foo.ToString("yyyy-MM-dd HH:mm:ss zzz");
        }

        public class DateTimeOffsetModel
        {
            public DateTimeOffset Foo { get; set; }
        }
    }
}
