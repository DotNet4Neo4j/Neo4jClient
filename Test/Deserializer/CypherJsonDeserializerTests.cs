using System;
using System.Collections.Generic;
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
            var result = deserializer.Deserialize(response).Single();

            // Assert
            if (expectedResult == null)
                Assert.IsNull(result.Foo);
            else
            {
                Assert.IsNotNull(result.Foo);
                Assert.AreEqual(expectedResult, result.Foo.Value.ToString("yyyy-MM-dd HH:mm:ss zzz"));
                Assert.AreEqual("Bar", result.Bar);
            }
        }

        public class DateTimeOffsetModel
        {
            public DateTimeOffset? Foo { get; set; }
            public string Bar { get; set; }
        }

        [Test]
        public void DeserializeShouldMapNodesInSetMode()
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

        [Test]
        public void DeserializeShouldMapRelationshipsInSetMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<RelationshipInstance<City>>(client, CypherResultMode.Set);
            var response = new RestResponse
            {
                Content = @"{
                  'data' : [ [ {
                    'start' : 'http://localhost:7474/db/data/node/55872',
                    'data' : {
                    'Name' : '東京'
                    },
                    'property' : 'http://localhost:7474/db/data/relationship/76931/properties/{key}',
                    'self' : 'http://localhost:7474/db/data/relationship/76931',
                    'properties' : 'http://localhost:7474/db/data/relationship/76931/properties',
                    'type' : 'REFERRAL_HAS_WHO_SECTION',
                    'extensions' : {
                    },
                    'end' : 'http://localhost:7474/db/data/node/55875'
                    } ], [ {
                    'start' : 'http://localhost:7474/db/data/node/55872',
                    'data' : {
                    'Name' : 'London'
                    },
                    'property' : 'http://localhost:7474/db/data/relationship/76931/properties/{key}',
                    'self' : 'http://localhost:7474/db/data/relationship/76931',
                    'properties' : 'http://localhost:7474/db/data/relationship/76931/properties',
                    'type' : 'REFERRAL_HAS_WHO_SECTION',
                    'extensions' : {
                    },
                    'end' : 'http://localhost:7474/db/data/node/55875'
                    } ], [ {
                    'start' : 'http://localhost:7474/db/data/node/55872',
                    'data' : {
                    'Name' : 'Sydney'
                    },
                    'property' : 'http://localhost:7474/db/data/relationship/76931/properties/{key}',
                    'self' : 'http://localhost:7474/db/data/relationship/76931',
                    'properties' : 'http://localhost:7474/db/data/relationship/76931/properties',
                    'type' : 'REFERRAL_HAS_WHO_SECTION',
                    'extensions' : {
                    },
                    'end' : 'http://localhost:7474/db/data/node/55875'
                    } ] ],
                  'columns' : [ 'c' ]
                }".Replace("'", "\"")
            };

            // Act
            var results = deserializer.Deserialize(response).ToArray();

            // Assert
            Assert.AreEqual(3, results.Count());

            var relationships = results.ElementAt(0);
            Assert.AreEqual("東京", relationships.Data.Name);

            relationships = results.ElementAt(1);
            Assert.AreEqual("London", relationships.Data.Name);

            relationships = results.ElementAt(2);
            Assert.AreEqual("Sydney", relationships.Data.Name);
        }

        [Test]
        public void DeserializeShouldMapIEnumerableOfRelationshipsInAProjectionMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<Projection>(client, CypherResultMode.Projection);
            var response = new RestResponse
            {
                Content = @"{
                  'data' : [ [ {
                    'outgoing_relationships' : 'http://localhost:7474/db/data/node/55745/relationships/out',
                    'data' : {
                      'Name' : '東京',
                      'Population' : 13000000
                    },
                    'all_typed_relationships' : 'http://localhost:7474/db/data/node/55745/relationships/all/{-list|&|types}',
                    'traverse' : 'http://localhost:7474/db/data/node/55745/traverse/{returnType}',
                    'self' : 'http://localhost:7474/db/data/node/55745',
                    'property' : 'http://localhost:7474/db/data/node/55745/properties/{key}',
                    'outgoing_typed_relationships' : 'http://localhost:7474/db/data/node/55745/relationships/out/{-list|&|types}',
                    'properties' : 'http://localhost:7474/db/data/node/55745/properties',
                    'incoming_relationships' : 'http://localhost:7474/db/data/node/55745/relationships/in',
                    'extensions' : {
                    },
                    'create_relationship' : 'http://localhost:7474/db/data/node/55745/relationships',
                    'paged_traverse' : 'http://localhost:7474/db/data/node/55745/paged/traverse/{returnType}{?pageSize,leaseTime}',
                    'all_relationships' : 'http://localhost:7474/db/data/node/55745/relationships/all',
                    'incoming_typed_relationships' : 'http://localhost:7474/db/data/node/55745/relationships/in/{-list|&|types}'
                  }, [ {
                    'start' : 'http://localhost:7474/db/data/node/55745',
                    'data' : {
                      'Number' : 66
                    },
                    'property' : 'http://localhost:7474/db/data/relationship/76743/properties/{key}',
                    'self' : 'http://localhost:7474/db/data/relationship/76743',
                    'properties' : 'http://localhost:7474/db/data/relationship/76743/properties',
                    'type' : 'REFERRAL_HAS_WHO_SECTION',
                    'extensions' : {
                    },
                    'end' : 'http://localhost:7474/db/data/node/55747'
                  } ] ], [ {
                    'outgoing_relationships' : 'http://localhost:7474/db/data/node/55745/relationships/out',
                    'data' : {
                      'Name' : '東京',
                      'Population' : 13000000
                    },
                    'all_typed_relationships' : 'http://localhost:7474/db/data/node/55745/relationships/all/{-list|&|types}',
                    'traverse' : 'http://localhost:7474/db/data/node/55745/traverse/{returnType}',
                    'self' : 'http://localhost:7474/db/data/node/55745',
                    'property' : 'http://localhost:7474/db/data/node/55745/properties/{key}',
                    'outgoing_typed_relationships' : 'http://localhost:7474/db/data/node/55745/relationships/out/{-list|&|types}',
                    'properties' : 'http://localhost:7474/db/data/node/55745/properties',
                    'incoming_relationships' : 'http://localhost:7474/db/data/node/55745/relationships/in',
                    'extensions' : {
                    },
                    'create_relationship' : 'http://localhost:7474/db/data/node/55745/relationships',
                    'paged_traverse' : 'http://localhost:7474/db/data/node/55745/paged/traverse/{returnType}{?pageSize,leaseTime}',
                    'all_relationships' : 'http://localhost:7474/db/data/node/55745/relationships/all',
                    'incoming_typed_relationships' : 'http://localhost:7474/db/data/node/55745/relationships/in/{-list|&|types}'
                  }, [ {
                    'start' : 'http://localhost:7474/db/data/node/55745',
                    'data' : {
                      'Number' : 66
                    },
                    'property' : 'http://localhost:7474/db/data/relationship/76743/properties/{key}',
                    'self' : 'http://localhost:7474/db/data/relationship/76743',
                    'properties' : 'http://localhost:7474/db/data/relationship/76743/properties',
                    'type' : 'REFERRAL_HAS_WHO_SECTION',
                    'extensions' : {
                    },
                    'end' : 'http://localhost:7474/db/data/node/55747'
                  }, {
                    'start' : 'http://localhost:7474/db/data/node/55747',
                    'data' : {
                      'Number' : 77
                    },
                    'property' : 'http://localhost:7474/db/data/relationship/76745/properties/{key}',
                    'self' : 'http://localhost:7474/db/data/relationship/76745',
                    'properties' : 'http://localhost:7474/db/data/relationship/76745/properties',
                    'type' : 'HAS_AUDIT',
                    'extensions' : {
                    },
                    'end' : 'http://localhost:7474/db/data/node/55748'
                  } ] ], [ {
                    'outgoing_relationships' : 'http://localhost:7474/db/data/node/55745/relationships/out',
                    'data' : {
                      'Name' : '東京',
                      'Population' : 13000000
                    },
                    'all_typed_relationships' : 'http://localhost:7474/db/data/node/55745/relationships/all/{-list|&|types}',
                    'traverse' : 'http://localhost:7474/db/data/node/55745/traverse/{returnType}',
                    'self' : 'http://localhost:7474/db/data/node/55745',
                    'property' : 'http://localhost:7474/db/data/node/55745/properties/{key}',
                    'outgoing_typed_relationships' : 'http://localhost:7474/db/data/node/55745/relationships/out/{-list|&|types}',
                    'properties' : 'http://localhost:7474/db/data/node/55745/properties',
                    'incoming_relationships' : 'http://localhost:7474/db/data/node/55745/relationships/in',
                    'extensions' : {
                    },
                    'create_relationship' : 'http://localhost:7474/db/data/node/55745/relationships',
                    'paged_traverse' : 'http://localhost:7474/db/data/node/55745/paged/traverse/{returnType}{?pageSize,leaseTime}',
                    'all_relationships' : 'http://localhost:7474/db/data/node/55745/relationships/all',
                    'incoming_typed_relationships' : 'http://localhost:7474/db/data/node/55745/relationships/in/{-list|&|types}'
                  }, [ {
                    'start' : 'http://localhost:7474/db/data/node/55745',
                    'data' : {
                      'Number' : 77
                    },
                    'property' : 'http://localhost:7474/db/data/relationship/76741/properties/{key}',
                    'self' : 'http://localhost:7474/db/data/relationship/76741',
                    'properties' : 'http://localhost:7474/db/data/relationship/76741/properties',
                    'type' : 'HAS_AUDIT',
                    'extensions' : {
                    },
                    'end' : 'http://localhost:7474/db/data/node/55746'
                  } ] ] ],
                  'columns' : [ 'Node', 'Relationships' ]
                }".Replace("'", "\"")
                            };

            // Act
            var results = deserializer.Deserialize(response).ToArray();

            // Assert
            var result = results[0];
            Assert.AreEqual("東京", result.Node.Data.Name);
            Assert.AreEqual(13000000, result.Node.Data.Population);
            Assert.AreEqual(66, result.Relationships.First().Data.Number);

            result = results[1];
            Assert.AreEqual("東京", result.Node.Data.Name);
            Assert.AreEqual(13000000, result.Node.Data.Population);
            Assert.AreEqual(66, result.Relationships.ToArray()[0].Data.Number);
            Assert.AreEqual(77, result.Relationships.ToArray()[1].Data.Number);

            result = results[2];
            Assert.AreEqual("東京", result.Node.Data.Name);
            Assert.AreEqual(13000000, result.Node.Data.Population);
            Assert.AreEqual(77, result.Relationships.First().Data.Number);
        }

        [Test]
        public void DeserializeShouldMapIEnumerableOfStringsInAProjectionMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<People>(client, CypherResultMode.Projection);
            var response = new RestResponse
            {
                Content = @"{
                  'data' : [ [ [ 'Ben Tu', 'Romiko Derbynew' ] ] ],
                  'columns' : [ 'Names' ]
                }".Replace("'", "\"")
            };

            // Act
            var results = deserializer.Deserialize(response).ToArray().First().Names.ToArray();

            // Assert
            Assert.AreEqual("Ben Tu", results[0]);
            Assert.AreEqual("Romiko Derbynew", results[1]);
        }

        [Test]
        public void DeserializeShouldMapIEnumerableOfStringsInSetMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<IEnumerable<string>>(client, CypherResultMode.Set);
            var response = new RestResponse
            {
                Content = @"{
                  'data' : [ [ [ 'Ben Tu', 'Romiko Derbynew' ] ] ],
                  'columns' : [ 'Names' ]
                }".Replace("'", "\"")
            };

            // Act
            var results = deserializer.Deserialize(response).ToArray().First().ToArray();

            // Assert
            Assert.AreEqual("Ben Tu", results[0]);
            Assert.AreEqual("Romiko Derbynew", results[1]);
        }

        public class City
        {
            public string Name { get; set; }
            public int Population { get; set; }
        }


        public class People
        {
            public IEnumerable<string> Names { get; set; }
        }

        public class Payload
        {
            public int Number { get; set; }
        }

        public class Projection
        {
            public IEnumerable<RelationshipInstance<Payload>> Relationships { get; set; }
            public Node<City> Node { get; set; }
        }

        [Test]
        public void DeserializeShouldMapProjectionIntoAnonymousType()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<City>(client, CypherResultMode.Set);
            var response = new RestResponse
            {
                Content = @"{
                  'data' : [
                    [ { 'Name': '東京', 'Population': 13000000 } ],
                    [ { 'Name': 'London', 'Population': 8000000 } ],
                    [ { 'Name': 'Sydney', 'Population': 4000000 } ]
                  ],
                  'columns' : [ 'Cities' ]
                }".Replace("'", "\"")
            };

            // Act
            var results = deserializer.Deserialize(response).ToArray();

            // Assert
            Assert.AreEqual(3, results.Count());

            var city = results.ElementAt(0);
            Assert.AreEqual("東京", city.Name);
            Assert.AreEqual(13000000, city.Population);

            city = results.ElementAt(1);
            Assert.AreEqual("London", city.Name);
            Assert.AreEqual(8000000, city.Population);

            city = results.ElementAt(2);
            Assert.AreEqual("Sydney", city.Name);
            Assert.AreEqual(4000000, city.Population);
        }
    }
}
