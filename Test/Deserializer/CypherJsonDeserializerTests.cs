using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Neo4jClient.Deserializer;
using RestSharp;

namespace Neo4jClient.Test.Deserializer
{
    [TestFixture]
    public class CypherJsonDeserializerTests
    {
        [Test]
        [TestCase("", null)]
        [TestCase("rekjre", null)]
        [TestCase("/Date(abcs)/", null)]
        [TestCase("/Date(abcs+0000)/", null)]
        [TestCase("/Date(1315271562384)/", "2011-09-06 01:12:42 +00:00")]
        [TestCase("/Date(1315271562384+0000)/", "2011-09-06 01:12:42 +00:00")]
        [TestCase("/Date(1315271562384+0200)/", "2011-09-06 03:12:42 +02:00")]
        [TestCase("/Date(1315271562384+1000)/", "2011-09-06 11:12:42 +10:00")]
        [TestCase("/Date(-2187290565386+0000)/", "1900-09-09 03:17:14 +00:00")]
        public void DeserializeShouldPreserveOffsetValues(string input, string expectedResult)
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<DateTimeOffsetModel>(client);
            var response = new RestResponse {Content = @"{
                                                          'columns' : [ 'Foo', 'Bar' ],
                                                          'data' : [ [ '" + input + @"', 'Bar' ] ]
                                                        }"};

            // Act
            var result = deserializer.Deserialize(response).ToArray();

            // Assert
            if (expectedResult == null)
                Assert.IsNull(result.First().Foo);
            else
            {
                Assert.IsNotNull(result.First().Foo);
                Assert.AreEqual(expectedResult, result.First().Foo.Value.ToString("yyyy-MM-dd HH:mm:ss zzz"));
                Assert.AreEqual("Bar", result.First().Bar);
            }
        }

        public class DateTimeOffsetModel
        {
            public DateTimeOffset? Foo { get; set; }
            public string Bar { get; set; }
        }
    }
}
