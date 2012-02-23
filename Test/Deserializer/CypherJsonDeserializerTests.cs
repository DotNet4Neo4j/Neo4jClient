using System;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Neo4jClient.Cypher;
using Neo4jClient.Deserializer;
using RestSharp;

namespace Neo4jClient.Test.Deserializer
{
    [TestFixture]
    public class CypherJsonDeserializerTests
    {
        const string SetModeContentFormat =
            @"{{
                'columns' : [ 'a' ],
                'data' : [ [ {{ 'Foo': '{0}', 'Bar': 'Bar' }} ] ]
            }}";

        const string ProjectionModeContentFormat =
            @"{{
                'columns' : [ 'Foo', 'Bar' ],
                'data' : [ [ '{0}', 'Bar' ] ]
            }}";

        [Test]
        [TestCase(CypherResultMode.Set, SetModeContentFormat, "", null)]
        [TestCase(CypherResultMode.Set, SetModeContentFormat, "rekjre", null)]
        [TestCase(CypherResultMode.Set, SetModeContentFormat, "/Date(abcs)/", null)]
        [TestCase(CypherResultMode.Set, SetModeContentFormat, "/Date(abcs+0000)/", null)]
        [TestCase(CypherResultMode.Set, SetModeContentFormat, "/Date(1315271562384)/", "2011-09-06 01:12:42 +00:00")]
        [TestCase(CypherResultMode.Set, SetModeContentFormat, "/Date(1315271562384+0000)/", "2011-09-06 01:12:42 +00:00")]
        [TestCase(CypherResultMode.Set, SetModeContentFormat, "/Date(1315271562384+0200)/", "2011-09-06 03:12:42 +02:00")]
        [TestCase(CypherResultMode.Set, SetModeContentFormat, "/Date(1315271562384+1000)/", "2011-09-06 11:12:42 +10:00")]
        [TestCase(CypherResultMode.Set, SetModeContentFormat, "/Date(-2187290565386+0000)/", "1900-09-09 03:17:14 +00:00")]
        [TestCase(CypherResultMode.Projection, ProjectionModeContentFormat, "", null)]
        [TestCase(CypherResultMode.Projection, ProjectionModeContentFormat, "rekjre", null)]
        [TestCase(CypherResultMode.Projection, ProjectionModeContentFormat, "/Date(abcs)/", null)]
        [TestCase(CypherResultMode.Projection, ProjectionModeContentFormat, "/Date(abcs+0000)/", null)]
        [TestCase(CypherResultMode.Projection, ProjectionModeContentFormat, "/Date(1315271562384)/", "2011-09-06 01:12:42 +00:00")]
        [TestCase(CypherResultMode.Projection, ProjectionModeContentFormat, "/Date(1315271562384+0000)/", "2011-09-06 01:12:42 +00:00")]
        [TestCase(CypherResultMode.Projection, ProjectionModeContentFormat, "/Date(1315271562384+0200)/", "2011-09-06 03:12:42 +02:00")]
        [TestCase(CypherResultMode.Projection, ProjectionModeContentFormat, "/Date(1315271562384+1000)/", "2011-09-06 11:12:42 +10:00")]
        [TestCase(CypherResultMode.Projection, ProjectionModeContentFormat, "/Date(-2187290565386+0000)/", "1900-09-09 03:17:14 +00:00")]
        public void DeserializeShouldPreserveOffsetValues(CypherResultMode resultMode, string contentFormat, string input, string expectedResult)
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<DateTimeOffsetModel>(client, resultMode);
            var response = new RestResponse {Content = string.Format(contentFormat, input)};

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

        [Test]
        public void DeserializeShouldMapNodesInSingleColumnMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<Node<City>>(client, CypherResultMode.Set);
            var response = new RestResponse
            {
                Content = @"{
                  'data' : [ [ {
                    'outgoing_relationships' : 'http://localhost:7474/db/data/node/5/relationships/out',
                    'data' : {
                      'Name' : '東京'
                    },
                    'all_typed_relationships' : 'http://localhost:7474/db/data/node/5/relationships/all/{-list|&|types}',
                    'traverse' : 'http://localhost:7474/db/data/node/5/traverse/{returnType}',
                    'self' : 'http://localhost:7474/db/data/node/5',
                    'property' : 'http://localhost:7474/db/data/node/5/properties/{key}',
                    'outgoing_typed_relationships' : 'http://localhost:7474/db/data/node/5/relationships/out/{-list|&|types}',
                    'properties' : 'http://localhost:7474/db/data/node/5/properties',
                    'incoming_relationships' : 'http://localhost:7474/db/data/node/5/relationships/in',
                    'extensions' : {
                    },
                    'create_relationship' : 'http://localhost:7474/db/data/node/5/relationships',
                    'paged_traverse' : 'http://localhost:7474/db/data/node/5/paged/traverse/{returnType}{?pageSize,leaseTime}',
                    'all_relationships' : 'http://localhost:7474/db/data/node/5/relationships/all',
                    'incoming_typed_relationships' : 'http://localhost:7474/db/data/node/5/relationships/in/{-list|&|types}'
                  } ], [ {
                    'outgoing_relationships' : 'http://localhost:7474/db/data/node/4/relationships/out',
                    'data' : {
                      'Name' : 'London'
                    },
                    'all_typed_relationships' : 'http://localhost:7474/db/data/node/4/relationships/all/{-list|&|types}',
                    'traverse' : 'http://localhost:7474/db/data/node/4/traverse/{returnType}',
                    'self' : 'http://localhost:7474/db/data/node/4',
                    'property' : 'http://localhost:7474/db/data/node/4/properties/{key}',
                    'outgoing_typed_relationships' : 'http://localhost:7474/db/data/node/4/relationships/out/{-list|&|types}',
                    'properties' : 'http://localhost:7474/db/data/node/4/properties',
                    'incoming_relationships' : 'http://localhost:7474/db/data/node/4/relationships/in',
                    'extensions' : {
                    },
                    'create_relationship' : 'http://localhost:7474/db/data/node/4/relationships',
                    'paged_traverse' : 'http://localhost:7474/db/data/node/4/paged/traverse/{returnType}{?pageSize,leaseTime}',
                    'all_relationships' : 'http://localhost:7474/db/data/node/4/relationships/all',
                    'incoming_typed_relationships' : 'http://localhost:7474/db/data/node/4/relationships/in/{-list|&|types}'
                  } ], [ {
                    'outgoing_relationships' : 'http://localhost:7474/db/data/node/3/relationships/out',
                    'data' : {
                      'Name' : 'Sydney'
                    },
                    'all_typed_relationships' : 'http://localhost:7474/db/data/node/3/relationships/all/{-list|&|types}',
                    'traverse' : 'http://localhost:7474/db/data/node/3/traverse/{returnType}',
                    'self' : 'http://localhost:7474/db/data/node/3',
                    'property' : 'http://localhost:7474/db/data/node/3/properties/{key}',
                    'outgoing_typed_relationships' : 'http://localhost:7474/db/data/node/3/relationships/out/{-list|&|types}',
                    'properties' : 'http://localhost:7474/db/data/node/3/properties',
                    'incoming_relationships' : 'http://localhost:7474/db/data/node/3/relationships/in',
                    'extensions' : {
                    },
                    'create_relationship' : 'http://localhost:7474/db/data/node/3/relationships',
                    'paged_traverse' : 'http://localhost:7474/db/data/node/3/paged/traverse/{returnType}{?pageSize,leaseTime}',
                    'all_relationships' : 'http://localhost:7474/db/data/node/3/relationships/all',
                    'incoming_typed_relationships' : 'http://localhost:7474/db/data/node/3/relationships/in/{-list|&|types}'
                  } ] ],
                  'columns' : [ 'c' ]
                }".Replace("'", "\"")
            };

            // Act
            var results = deserializer.Deserialize(response).ToArray();

            // Assert
            Assert.AreEqual(3, results.Count());

            var node = results.ElementAt(0);
            Assert.AreEqual(5, node.Reference.Id);
            Assert.AreEqual("東京", node.Data.Name);

            node = results.ElementAt(1);
            Assert.AreEqual(4, node.Reference.Id);
            Assert.AreEqual("London", node.Data.Name);

            node = results.ElementAt(2);
            Assert.AreEqual(3, node.Reference.Id);
            Assert.AreEqual("Sydney", node.Data.Name);
        }

        public class City
        {
            public string Name { get; set; }
        }
    }
}
