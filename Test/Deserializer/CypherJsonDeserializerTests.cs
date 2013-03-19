using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Neo4jClient.Cypher;
using Neo4jClient.Deserializer;

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
        [TestCase(CypherResultMode.Set, SetModeContentFormat, "2011-09-06T01:12:42+10:00", "2011-09-06 01:12:42 +10:00")]
        [TestCase(CypherResultMode.Set, SetModeContentFormat, "2011-09-06T01:12:42+00:00", "2011-09-06 01:12:42 +00:00")]
        [TestCase(CypherResultMode.Set, SetModeContentFormat, "2012-08-31T10:11:00.3642578+10:00", "2012-08-31 10:11:00 +10:00")]
        [TestCase(CypherResultMode.Projection, ProjectionModeContentFormat, "", null)]
        [TestCase(CypherResultMode.Projection, ProjectionModeContentFormat, "rekjre", null)]
        [TestCase(CypherResultMode.Projection, ProjectionModeContentFormat, "/Date(abcs)/", null)]
        [TestCase(CypherResultMode.Projection, ProjectionModeContentFormat, "/Date(abcs+0000)/", null)]
        [TestCase(CypherResultMode.Projection, ProjectionModeContentFormat, "/Date(1315271562384)/", "2011-09-06 01:12:42 +00:00")]
        [TestCase(CypherResultMode.Projection, ProjectionModeContentFormat, "/Date(1315271562384+0000)/", "2011-09-06 01:12:42 +00:00")]
        [TestCase(CypherResultMode.Projection, ProjectionModeContentFormat, "/Date(1315271562384+0200)/", "2011-09-06 03:12:42 +02:00")]
        [TestCase(CypherResultMode.Projection, ProjectionModeContentFormat, "/Date(1315271562384+1000)/", "2011-09-06 11:12:42 +10:00")]
        [TestCase(CypherResultMode.Projection, ProjectionModeContentFormat, "/Date(-2187290565386+0000)/", "1900-09-09 03:17:14 +00:00")]
        [TestCase(CypherResultMode.Projection, ProjectionModeContentFormat, "2011-09-06T01:12:42+10:00", "2011-09-06 01:12:42 +10:00")]
        [TestCase(CypherResultMode.Projection, ProjectionModeContentFormat, "2011-09-06T01:12:42+00:00", "2011-09-06 01:12:42 +00:00")]
        [TestCase(CypherResultMode.Projection, ProjectionModeContentFormat, "2012-08-31T10:11:00.3642578+10:00", "2012-08-31 10:11:00 +10:00")]
        public void DeserializeShouldPreserveOffsetValues(CypherResultMode resultMode, string contentFormat, string input, string expectedResult)
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<DateTimeOffsetModel>(client, resultMode);
            var content = string.Format(contentFormat, input);

            // Act
            var result = deserializer.Deserialize(content).Single();

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
            var content = @"{
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
                }".Replace("'", "\"");

            // Act
            var results = deserializer.Deserialize(content).ToArray();

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
            var content = @"{
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
                }".Replace("'", "\"");

            // Act
            var results = deserializer.Deserialize(content).ToArray();

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
            var content = @"{
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
                }".Replace("'", "\"");

            // Act
            var results = deserializer.Deserialize(content).ToArray();

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
        public void DeserializeShouldMapIEnumerableOfNodesReturnedByCollectInAProjectionMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<ResultWithNestedNodeDto>(client, CypherResultMode.Projection);
            var content = @"{
                      'data' : [ [ [ {
                        'outgoing_relationships' : 'http://foo/db/data/node/920/relationships/out',
                        'data' : {
                                    'Bar' : 'bar',
                                    'Baz' : 'baz'
                        },
                        'all_typed_relationships' : 'http://foo/db/data/node/920/relationships/all/{-list|&|types}',
                        'traverse' : 'http://foo/db/data/node/920/traverse/{returnType}',
                        'property' : 'http://foo/db/data/node/920/properties/{key}',
                        'self' : 'http://foo/db/data/node/920',
                        'outgoing_typed_relationships' : 'http://foo/db/data/node/920/relationships/out/{-list|&|types}',
                        'properties' : 'http://foo/db/data/node/920/properties',
                        'incoming_relationships' : 'http://foo/db/data/node/920/relationships/in',
                        'extensions' : {
                        },
                        'create_relationship' : 'http://foo/db/data/node/920/relationships',
                        'paged_traverse' : 'http://foo/db/data/node/920/paged/traverse/{returnType}{?pageSize,leaseTime}',
                        'all_relationships' : 'http://foo/db/data/node/920/relationships/all',
                        'incoming_typed_relationships' : 'http://foo/db/data/node/920/relationships/in/{-list|&|types}'
                      }, {
                        'outgoing_relationships' : 'http://foo/db/data/node/5482/relationships/out',
                        'data' : {
                                    'Bar' : 'bar',
                                    'Baz' : 'baz'
                        },
                        'all_typed_relationships' : 'http://foo/db/data/node/5482/relationships/all/{-list|&|types}',
                        'traverse' : 'http://foo/db/data/node/5482/traverse/{returnType}',
                        'property' : 'http://foo/db/data/node/5482/properties/{key}',
                        'self' : 'http://foo/db/data/node/5482',
                        'outgoing_typed_relationships' : 'http://foo/db/data/node/5482/relationships/out/{-list|&|types}',
                        'properties' : 'http://foo/db/data/node/5482/properties',
                        'incoming_relationships' : 'http://foo/db/data/node/5482/relationships/in',
                        'extensions' : {
                        },
                        'create_relationship' : 'http://foo/db/data/node/5482/relationships',
                        'paged_traverse' : 'http://foo/db/data/node/5482/paged/traverse/{returnType}{?pageSize,leaseTime}',
                        'all_relationships' : 'http://foo/db/data/node/5482/relationships/all',
                        'incoming_typed_relationships' : 'http://foo/db/data/node/5482/relationships/in/{-list|&|types}'
                      } ] ] ],
                      'columns' : ['Fooness']
                            }".Replace('\'', '"');

            // Act
            var results = deserializer.Deserialize(content).ToArray();

            Assert.IsInstanceOf<IEnumerable<ResultWithNestedNodeDto>>(results);
            Assert.AreEqual(1, results.Count());
            Assert.AreEqual(2, results[0].Fooness.Count());

            Assert.AreEqual("bar", results[0].Fooness.ToArray()[0].Data.Bar);
            Assert.AreEqual("baz", results[0].Fooness.ToArray()[0].Data.Baz);

            Assert.AreEqual("bar", results[0].Fooness.ToArray()[1].Data.Bar);
            Assert.AreEqual("baz", results[0].Fooness.ToArray()[1].Data.Baz);

        }

        [Test]
        public void DeserializeShouldMapIEnumerableOfNodesReturnedByCollectInColumnInProjectionMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<ProjectionFeedItem>(client, CypherResultMode.Projection);
            
            //The sample query that generated this data:
            /*
             START me=node:node_auto_index(UserIdentifier='USER0')
             MATCH me-[rels:FOLLOWS*0..1]-myfriend
             WITH myfriend
             MATCH myfriend-[:POSTED*]-statusupdates<-[r?:LIKE]-likers
             WHERE myfriend <> statusupdates
             RETURN distinct statusupdates, FILTER (x in collect(distinct likers) : x <> null), myfriend
             ORDER BY statusupdates.PostTime DESC
             LIMIT 25; 
             */
            //The data below is copied from the return from the above query. You will notice that the second column of data (Likers) is actually an array
            //of items. This means the deserializer has to be able to deal with columns that are returning a list of items.
            var content = @"{
             'columns':['Post','Likers','User'],
             'data':[[
             {
                 'extensions':{},
                 'paged_traverse':'http://localhost:7474/db/data/node/189/paged/traverse/{returnType}{?pageSize,leaseTime}',
                 'outgoing_relationships':'http://localhost:7474/db/data/node/189/relationships/out',
                 'traverse':'http://localhost:7474/db/data/node/189/traverse/{returnType}',
                 'all_typed_relationships':'http://localhost:7474/db/data/node/189/relationships/all/{-list|&|types}',
                 'property':'http://localhost:7474/db/data/node/189/properties/{key}',
                 'all_relationships':'http://localhost:7474/db/data/node/189/relationships/all',
                 'self':'http://localhost:7474/db/data/node/189',
                 'properties':'http://localhost:7474/db/data/node/189/properties',
                 'outgoing_typed_relationships':'http://localhost:7474/db/data/node/189/relationships/out/{-list|&|types}',
                 'incoming_relationships':'http://localhost:7474/db/data/node/189/relationships/in',
                 'incoming_typed_relationships':'http://localhost:7474/db/data/node/189/relationships/in/{-list|&|types}',
                 'create_relationship':'http://localhost:7474/db/data/node/189/relationships',
                 'data':{
                      'PostTime':634881866575852740,
                      'PostIdentifier':'USER1POST0',
                      'Comment':'Here is a post statement 0 posted by user 1',
                      'Content':'Blah blah blah'
                 }
             },
             [{
                  'extensions':{},
                  'paged_traverse':'http://localhost:7474/db/data/node/188/paged/traverse/{returnType}{?pageSize,leaseTime}',
                  'outgoing_relationships':'http://localhost:7474/db/data/node/188/relationships/out',
                  'traverse':'http://localhost:7474/db/data/node/188/traverse/{returnType}',
                  'all_typed_relationships':'http://localhost:7474/db/data/node/188/relationships/all/{-list|&|types}',
                  'property':'http://localhost:7474/db/data/node/188/properties/{key}',
                  'all_relationships':'http://localhost:7474/db/data/node/188/relationships/all',
                  'self':'http://localhost:7474/db/data/node/188',
                  'properties':'http://localhost:7474/db/data/node/188/properties',
                  'outgoing_typed_relationships':'http://localhost:7474/db/data/node/188/relationships/out/{-list|&|types}',
                  'incoming_relationships':'http://localhost:7474/db/data/node/188/relationships/in',
                  'incoming_typed_relationships':'http://localhost:7474/db/data/node/188/relationships/in/{-list|&|types}',
                  'create_relationship':'http://localhost:7474/db/data/node/188/relationships',
                  'data':{
                       'UserIdentifier':'USER1',
                       'Email':'someoneelse@something.net',
                       'FamilyName':'Jones',
                       'GivenName':'bob'
                  }
             },
             {
                  'extensions':{},
                  'paged_traverse':'http://localhost:7474/db/data/node/197/paged/traverse/{returnType}{?pageSize,leaseTime}',
                  'outgoing_relationships':'http://localhost:7474/db/data/node/197/relationships/out',
                  'traverse':'http://localhost:7474/db/data/node/197/traverse/{returnType}',
                  'all_typed_relationships':'http://localhost:7474/db/data/node/197/relationships/all/{-list|&|types}',
                  'property':'http://localhost:7474/db/data/node/197/properties/{key}',
                  'all_relationships':'http://localhost:7474/db/data/node/197/relationships/all',
                  'self':'http://localhost:7474/db/data/node/197',
                  'properties':'http://localhost:7474/db/data/node/197/properties',
                  'outgoing_typed_relationships':'http://localhost:7474/db/data/node/197/relationships/out/{-list|&|types}',
                  'incoming_relationships':'http://localhost:7474/db/data/node/197/relationships/in',
                  'incoming_typed_relationships':'http://localhost:7474/db/data/node/197/relationships/in/{-list|&|types}',
                  'create_relationship':'http://localhost:7474/db/data/node/197/relationships',
                  'data':{
                       'UserIdentifier':'USER2',
                       'Email':'someone@someotheraddress.net',
                       'FamilyName':'Bob',
                       'GivenName':'Jimmy'
                  }
             }],
             {
                  'extensions':{},
                  'paged_traverse':'http://localhost:7474/db/data/node/188/paged/traverse/{returnType}{?pageSize,leaseTime}',
                  'outgoing_relationships':'http://localhost:7474/db/data/node/188/relationships/out',
                  'traverse':'http://localhost:7474/db/data/node/188/traverse/{returnType}',
                  'all_typed_relationships':'http://localhost:7474/db/data/node/188/relationships/all/{-list|&|types}',
                  'property':'http://localhost:7474/db/data/node/188/properties/{key}',
                  'all_relationships':'http://localhost:7474/db/data/node/188/relationships/all',
                  'self':'http://localhost:7474/db/data/node/188',
                  'properties':'http://localhost:7474/db/data/node/188/properties',
                  'outgoing_typed_relationships':'http://localhost:7474/db/data/node/188/relationships/out/{-list|&|types}',
                  'incoming_relationships':'http://localhost:7474/db/data/node/188/relationships/in',
                  'incoming_typed_relationships':'http://localhost:7474/db/data/node/188/relationships/in/{-list|&|types}',
                  'create_relationship':'http://localhost:7474/db/data/node/188/relationships',
                  'data':{
                       'UserIdentifier':'USER1',
                       'Email':'someoneelse@something.net',
                       'FamilyName':'Jones',
                       'GivenName':'bob'}
                   }
            ]]}".Replace('\'', '"');
            
            // Act
            var results = deserializer.Deserialize(content).ToArray();
            
            Assert.AreEqual(1, results.Count());
            Assert.IsNotNull(results[0].Post);
            Assert.IsNotNull(results[0].User);
            Assert.IsNotNull(results[0].Likers);
            
            Assert.IsInstanceOf<IEnumerable<User>>(results[0].Likers);
            
            Assert.AreEqual(2, results[0].Likers.Count());
            Assert.AreEqual("USER1", results[0].User.UserIdentifier);
            Assert.AreEqual("USER1POST0", results[0].Post.PostIdentifier);
            //and make sure the likers properties have been set
            Assert.AreEqual("USER1", results[0].Likers.ToArray()[0].UserIdentifier);
            Assert.AreEqual("USER2", results[0].Likers.ToArray()[1].UserIdentifier);
        }

        [Test]
        public void DeserializeShouldMapNullIEnumerableOfNodesReturnedByCollectInInAProjectionMode()
        {

            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<ResultWithNestedNodeDto>(client, CypherResultMode.Projection);
            var content =
                        @"{
                      'data' : [ [ [ null ] ] ],
                      'columns' : ['Fooness']
                            }"
                            .Replace('\'', '"');

            var results = deserializer.Deserialize(content).ToArray();

            Assert.IsInstanceOf<IEnumerable<ResultWithNestedNodeDto>>(results);
            Assert.AreEqual(1, results.Count());

            Assert.IsNull(results[0].Fooness);
        }

        [Test]
        public void DeserializeShouldMapIEnumerableOfStringsInAProjectionMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<People>(client, CypherResultMode.Projection);
            var content = @"{
                  'data' : [ [ [ 'Ben Tu', 'Romiko Derbynew' ] ] ],
                  'columns' : [ 'Names' ]
                }".Replace("'", "\"");

            // Act
            var results = deserializer.Deserialize(content).ToArray().First().Names.ToArray();

            // Assert
            Assert.AreEqual("Ben Tu", results[0]);
            Assert.AreEqual("Romiko Derbynew", results[1]);
        }

        [Test]
        public void DeserializeShouldMapIEnumerableOfStringsThatAreEmptyInAProjectionMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<People>(client, CypherResultMode.Projection);
            var content = @"{
                  'data' : [ [ [ ] ] ],
                  'columns' : [ 'Names' ]
                }".Replace("'", "\"");

            // Act
            var results = deserializer.Deserialize(content).ToArray();

            // Assert
            Assert.AreEqual(0, results.First().Names.Count());
        }

        [Test]
        public void DeserializeShouldMapIEnumerableOfStringsInSetMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<IEnumerable<string>>(client, CypherResultMode.Set);
            var content = @"{
                  'data' : [ [ [ 'Ben Tu', 'Romiko Derbynew' ] ] ],
                  'columns' : [ 'Names' ]
                }".Replace("'", "\"");

            // Act
            var results = deserializer.Deserialize(content).ToArray().First().ToArray();

            // Assert
            Assert.AreEqual("Ben Tu", results[0]);
            Assert.AreEqual("Romiko Derbynew", results[1]);
        }

        [Test]
        public void DeserializeShouldMapArrayOfStringsInSetMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<string[]>(client, CypherResultMode.Set);
            var content = @"{
                  'data' : [ [ [ 'Ben Tu', 'Romiko Derbynew' ] ] ],
                  'columns' : [ 'Names' ]
                }".Replace("'", "\"");

            // Act
            var results = deserializer.Deserialize(content).ToArray().First().ToArray();

            // Assert
            Assert.AreEqual("Ben Tu", results[0]);
            Assert.AreEqual("Romiko Derbynew", results[1]);
        }

        [Test]
        public void DeserializeShouldMapArrayOfIntegersInSetMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<int[]>(client, CypherResultMode.Set);
            var content = @"{
                  'data' : [ [ [ '666', '777' ] ] ],
                  'columns' : [ 'Names' ]
                }".Replace("'", "\"");

            // Act
            var results = deserializer.Deserialize(content).ToArray().First().ToArray();

            // Assert
            Assert.AreEqual(666, results[0]);
            Assert.AreEqual(777, results[1]);
        }

        [Test]
        public void DeserializeShouldMapIntegerInSetMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<int>(client, CypherResultMode.Set);
            var content = @"{
                          'data' : [ [ 666 ] ],
                          'columns' : [ 'count(distinct registration)' ]
                        }".Replace("'", "\"");

            // Act
            var results = deserializer.Deserialize(content).ToArray();

            // Assert
            Assert.AreEqual(666, results.First());

        }

        [Test]
        public void Issue63_DeserializeShouldMapEmptyCollectResultsWithOtherProperties()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<ModelWithCollect>(client, CypherResultMode.Projection);
            var content = @"{
  'columns' : [ 'Fans', 'Poster' ],
  'data' : [ [ [ null ], {
    'paged_traverse' : 'http://localhost:8000/db/data/node/740/paged/traverse/{returnType}{?pageSize,leaseTime}',
    'outgoing_relationships' : 'http://localhost:8000/db/data/node/740/relationships/out',
    'data' : {
      'GivenName' : 'Bob'
    },
    'all_typed_relationships' : 'http://localhost:8000/db/data/node/740/relationships/all/{-list|&|types}',
    'traverse' : 'http://localhost:8000/db/data/node/740/traverse/{returnType}',
    'all_relationships' : 'http://localhost:8000/db/data/node/740/relationships/all',
    'self' : 'http://localhost:8000/db/data/node/740',
    'property' : 'http://localhost:8000/db/data/node/740/properties/{key}',
    'properties' : 'http://localhost:8000/db/data/node/740/properties',
    'outgoing_typed_relationships' : 'http://localhost:8000/db/data/node/740/relationships/out/{-list|&|types}',
    'incoming_relationships' : 'http://localhost:8000/db/data/node/740/relationships/in',
    'incoming_typed_relationships' : 'http://localhost:8000/db/data/node/740/relationships/in/{-list|&|types}',
    'extensions' : {
    },
    'create_relationship' : 'http://localhost:8000/db/data/node/740/relationships'
  } ] ]
}".Replace("'", "\"");

            // Act
            var results = deserializer.Deserialize(content).ToArray();

            // Assert
            Assert.AreEqual(1, results.Count());
            Assert.AreEqual(null, results[0].Fans);
            Assert.IsInstanceOf<Node<User>>(results[0].Poster);
            Assert.AreEqual(740, results[0].Poster.Reference.Id);
            Assert.AreEqual("Bob", results[0].Poster.Data.GivenName);
        }

        public class Post 
        {
            public String Content { get; set; }
            public String Comment { get; set; }
            public String PostIdentifier { get; set; }
            public long PostTime { get; set; }
        }

        public class User 
        {
            public string GivenName { get; set; }
            public string FamilyName { get; set; }
            public string Email { get; set; }
            public string UserIdentifier { get; set; }
        }

        public class ProjectionFeedItem 
        {
            public Post Post { get; set; }
            public User User { get; set; }
            public IEnumerable<User> Likers { get; set; }
        }

        public class FooData
        {
            public string Bar { get; set; }
            public string Baz { get; set; }
            public DateTimeOffset? Date { get; set; }
        }

        public class ResultWithNestedNodeDto
        {
            public IEnumerable<Node<FooData>> Fooness { get; set; }
            public string Name { get; set; }
            public long? UniqueId { get; set; }
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

        public class ModelWithCollect
        {
            public Node<User> Poster { get; set; }
            public IEnumerable<Node<object>> Fans { get; set; }
        }

        [Test]
        public void DeserializeShouldPreserveUtf8Characters()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<City>(client, CypherResultMode.Set);
            var content = @"{
                  'data' : [
                    [ { 'Name': '東京', 'Population': 13000000 } ],
                    [ { 'Name': 'London', 'Population': 8000000 } ],
                    [ { 'Name': 'Sydney', 'Population': 4000000 } ]
                  ],
                  'columns' : [ 'Cities' ]
                }".Replace("'", "\"");

            // Act
            var results = deserializer.Deserialize(content).ToArray();

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

        [Test]
        public void DeserializeShouldMapNodesToObjectsInSetModeWhenTheSourceLooksLikeANodeButTheDestinationDoesnt()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<Asset>(client, CypherResultMode.Set);
            var content = @"
                    {
                        'data' : [ [ {
                        'outgoing_relationships' : 'http://foo/db/data/node/879/relationships/out',
                        'data' : {
                            'Name' : '67',
                            'UniqueId' : 2
                        },
                        'traverse' : 'http://foo/db/data/node/879/traverse/{returnType}',
                        'all_typed_relationships' : 'http://foo/db/data/node/879/relationships/all/{-list|&|types}',
                        'property' : 'http://foo/db/data/node/879/properties/{key}',
                        'self' : 'http://foo/db/data/node/879',
                        'properties' : 'http://foo/db/data/node/879/properties',
                        'outgoing_typed_relationships' : 'http://foo/db/data/node/879/relationships/out/{-list|&|types}',
                        'incoming_relationships' : 'http://foo/db/data/node/879/relationships/in',
                        'extensions' : {
                        },
                        'create_relationship' : 'http://foo/db/data/node/879/relationships',
                        'paged_traverse' : 'http://foo/db/data/node/879/paged/traverse/{returnType}{?pageSize,leaseTime}',
                        'all_relationships' : 'http://foo/db/data/node/879/relationships/all',
                        'incoming_typed_relationships' : 'http://foo/db/data/node/879/relationships/in/{-list|&|types}'
                        } ], [ {
                        'outgoing_relationships' : 'http://foo/db/data/node/878/relationships/out',
                        'data' : {
                            'Name' : '15 Mile',
                            'UniqueId' : 1
                        },
                        'traverse' : 'http://foo/db/data/node/878/traverse/{returnType}',
                        'all_typed_relationships' : 'http://foo/db/data/node/878/relationships/all/{-list|&|types}',
                        'property' : 'http://foo/db/data/node/878/properties/{key}',
                        'self' : 'http://foo/db/data/node/878',
                        'properties' : 'http://foo/db/data/node/878/properties',
                        'outgoing_typed_relationships' : 'http://foo/db/data/node/878/relationships/out/{-list|&|types}',
                        'incoming_relationships' : 'http://foo/db/data/node/878/relationships/in',
                        'extensions' : {
                        },
                        'create_relationship' : 'http://foo/db/data/node/878/relationships',
                        'paged_traverse' : 'http://foo/db/data/node/878/paged/traverse/{returnType}{?pageSize,leaseTime}',
                        'all_relationships' : 'http://foo/db/data/node/878/relationships/all',
                        'incoming_typed_relationships' : 'http://foo/db/data/node/878/relationships/in/{-list|&|types}'
                        } ] ],
                        'columns' : [ 'asset' ]
                    }".Replace("'", "\"");

            // Act
            var results = deserializer.Deserialize(content).ToArray();

            // Assert
            var resultsArray = results.ToArray();
            Assert.AreEqual(2, resultsArray.Count());

            var firstResult = resultsArray[0];
            Assert.AreEqual("67", firstResult.Name);
            Assert.AreEqual(2, firstResult.UniqueId);

            var secondResult = resultsArray[1];
            Assert.AreEqual("15 Mile", secondResult.Name);
            Assert.AreEqual(1, secondResult.UniqueId);
        }

        public class Asset
        {
            public long UniqueId { get; set; }
            public string Name { get; set; }
        }
    }
}
