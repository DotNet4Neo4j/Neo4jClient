using System;
using System.Linq;
using NUnit.Framework;
using Neo4jClient.ApiModels;
using Neo4jClient.Deserializer;
using RestSharp;

namespace Neo4jClient.Test.Deserializer
{
    [TestFixture]
    public class CustomJsonDeserializerTests
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
            var deserializer = new CustomJsonDeserializer();
            var response = new RestResponse {Content = string.Format("{{'Foo':'{0}'}}", input)};

            // Act
            var result = deserializer.Deserialize<DateTimeOffsetModel>(response);

            // Assert
            if (expectedResult == null)
                Assert.IsNull(result.Foo);
            else
            {
                Assert.IsNotNull(result.Foo);
                Assert.AreEqual(expectedResult, result.Foo.Value.ToString("yyyy-MM-dd HH:mm:ss zzz"));
            }
        }

        public class DateTimeOffsetModel
        {
            public DateTimeOffset? Foo { get; set; }
        }

        [Test]
        public void DeserializeShouldConvertTableCapResponseToGremlinTableCapResponse()
        {
            // Arrange
            var deserializer = new CustomJsonDeserializer();
            var response = new RestResponse
            {
                Content = @"{
                              ""columns"" : [ ""ColumnA"" ],
                              ""data"" : [ [ ""DataA"" ], [ ""DataB"" ] ]
                            }"
            };

            // Act
            var result = deserializer.Deserialize<GremlinTableCapResponse>(response);
            var data = result.Data.SelectMany(d => d).ToArray();

            // Assert
            Assert.IsTrue(result.Columns.Any(c => c == "ColumnA"));
            Assert.IsTrue(data.Any(d => d == "DataA"));
            Assert.IsTrue(data.Any(d => d == "DataB"));
        }
    }
}
