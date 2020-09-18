using System;
using System.Collections.Generic;
using System.Linq;
using Neo4jClient.Cypher;
using Neo4jClient.Serialization;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.Serialization
{
    
    public class CypherJsonDeserializerTests : IClassFixture<CultureInfoSetupFixture>
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

        private class DateTimeTestCasesFactory
        {
            public static IEnumerable<object[]> TestCases
            {
                get
                {
                    yield return new object[]{"2015-06-01T15:03:39.1462808", DateTimeKind.Unspecified};
                    yield return new object[] { "2015-06-01T15:03:39.1462808Z", DateTimeKind.Utc};
                    yield return new object[] { "2015-06-01T15:03:39.1462808+00:00", DateTimeKind.Local};
                }
            }
        }


        private class DateTimeOffsetCasesFactory
        {
            public static IEnumerable<object[]> TestCases
            {
                get
                {
                    yield return new object[]{CypherResultMode.Set, CypherResultFormat.Rest, SetModeContentFormat, "", null};
                    yield return new object[]{CypherResultMode.Set, CypherResultFormat.Rest, SetModeContentFormat, "rekjre", null};
                    yield return new object[]{CypherResultMode.Set, CypherResultFormat.Rest, SetModeContentFormat, "/Date(abcs)/", null};
                    yield return new object[]{CypherResultMode.Set, CypherResultFormat.Rest, SetModeContentFormat, "/Date(abcs+0000)/", null};
                    yield return new object[]{CypherResultMode.Set, CypherResultFormat.Rest, SetModeContentFormat, "/Date(1315271562384)/", "2011-09-06 01:12:42 +00:00"};
                    yield return new object[]{CypherResultMode.Set, CypherResultFormat.Rest, SetModeContentFormat, "/Date(1315271562384+0000)/", "2011-09-06 01:12:42 +00:00"};
                    yield return new object[]{CypherResultMode.Set, CypherResultFormat.Rest, SetModeContentFormat, "/Date(1315271562384+0200)/", "2011-09-06 03:12:42 +02:00"};
                    yield return new object[]{CypherResultMode.Set, CypherResultFormat.Rest, SetModeContentFormat, "/Date(1315271562384+1000)/", "2011-09-06 11:12:42 +10:00"};
                    yield return new object[]{CypherResultMode.Set, CypherResultFormat.Rest, SetModeContentFormat, "/Date(-2187290565386+0000)/", "1900-09-09 03:17:14 +00:00"};
                    yield return new object[]{CypherResultMode.Set, CypherResultFormat.Rest, SetModeContentFormat, "2011-09-06T01:12:42+10:00", "2011-09-06 01:12:42 +10:00"};
                    yield return new object[]{CypherResultMode.Set, CypherResultFormat.Rest, SetModeContentFormat, "2011-09-06T01:12:42+00:00", "2011-09-06 01:12:42 +00:00"};
                    yield return new object[]{CypherResultMode.Set, CypherResultFormat.Rest, SetModeContentFormat, "2012-08-31T10:11:00.3642578+10:00", "2012-08-31 10:11:00 +10:00"};
                    yield return new object[]{CypherResultMode.Projection, CypherResultFormat.Rest, ProjectionModeContentFormat, "", null};
                    yield return new object[]{CypherResultMode.Projection, CypherResultFormat.Rest, ProjectionModeContentFormat, "rekjre", null};
                    yield return new object[]{CypherResultMode.Projection, CypherResultFormat.Rest, ProjectionModeContentFormat, "/Date(abcs)/", null};
                    yield return new object[]{CypherResultMode.Projection, CypherResultFormat.Rest, ProjectionModeContentFormat, "/Date(abcs+0000)/", null};
                    yield return new object[]{CypherResultMode.Projection, CypherResultFormat.Rest, ProjectionModeContentFormat, "/Date(1315271562384)/", "2011-09-06 01:12:42 +00:00"};
                    yield return new object[]{CypherResultMode.Projection, CypherResultFormat.Rest, ProjectionModeContentFormat, "/Date(1315271562384+0000)/", "2011-09-06 01:12:42 +00:00"};
                    yield return new object[]{CypherResultMode.Projection, CypherResultFormat.Rest, ProjectionModeContentFormat, "/Date(1315271562384+0200)/", "2011-09-06 03:12:42 +02:00"};
                    yield return new object[]{CypherResultMode.Projection, CypherResultFormat.Rest, ProjectionModeContentFormat, "/Date(1315271562384+1000)/", "2011-09-06 11:12:42 +10:00"};
                    yield return new object[]{CypherResultMode.Projection, CypherResultFormat.Rest, ProjectionModeContentFormat, "/Date(-2187290565386+0000)/", "1900-09-09 03:17:14 +00:00"};
                    yield return new object[]{CypherResultMode.Projection, CypherResultFormat.Rest, ProjectionModeContentFormat, "2011-09-06T01:12:42+10:00", "2011-09-06 01:12:42 +10:00"};
                    yield return new object[]{CypherResultMode.Projection, CypherResultFormat.Rest, ProjectionModeContentFormat, "2011-09-06T01:12:42+00:00", "2011-09-06 01:12:42 +00:00"};
                    yield return new object[]{CypherResultMode.Projection, CypherResultFormat.Rest, ProjectionModeContentFormat, "2012-08-31T10:11:00.3642578+10:00", "2012-08-31 10:11:00 +10:00"};
                }
            }
        }

        public CypherJsonDeserializerTests()
        {
            CultureInfoSetupFixture.SetDeterministicCulture();
        }
                    
        [Theory]
        [MemberData(nameof(DateTimeOffsetCasesFactory.TestCases), MemberType = typeof(DateTimeOffsetCasesFactory))]
        public void DeserializeShouldPreserveOffsetValues(CypherResultMode resultMode, CypherResultFormat format, string contentFormat, string input, string expectedResult)
        {  
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<DateTimeOffsetModel>(client, resultMode, format);
            var content = string.Format(contentFormat, input);

            // Act
            var result = deserializer.Deserialize(content, false).Single();

            // Assert
            if (expectedResult == null)
                Assert.Null(result.Foo);
            else
            {
                Assert.NotNull(result.Foo);
                Assert.Equal(expectedResult, result.Foo.Value.ToString("yyyy-MM-dd HH:mm:ss zzz"));
                Assert.Equal("Bar", result.Bar);
            }
        }

        [Theory]
        [MemberData(nameof(DateTimeTestCasesFactory.TestCases), MemberType = typeof(DateTimeTestCasesFactory))]
        public void DeserializeDateShouldPreserveKind(string dateTime, DateTimeKind kind)
        { 
            //Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<DateTimeModel>(client, CypherResultMode.Projection, CypherResultFormat.Rest);
            var content = string.Format(ProjectionModeContentFormat, dateTime);

            //Act
            var result = deserializer.Deserialize(content, false).Single();

            //Assert
            Assert.Equal(result.Foo.Kind, kind);
        }

        [Theory]
        [MemberData(nameof(DateTimeTestCasesFactory.TestCases), MemberType = typeof(DateTimeTestCasesFactory))]
        public void DeserializeDateShouldPreservePointInTime(string dateTime, DateTimeKind kind)
        { 
            //Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<DateTimeModel>(client, CypherResultMode.Projection, CypherResultFormat.Rest);
            var content = string.Format(ProjectionModeContentFormat, dateTime);

            //Act
            var result = deserializer.Deserialize(content, false).Single();

            //Assert
            Assert.Equal(result.Foo.ToUniversalTime(), DateTime.Parse(dateTime).ToUniversalTime());
        }

        public class DateTimeOffsetModel
        {
            public DateTimeOffset? Foo { get; set; }
            public string Bar { get; set; }
        }

        private class DateTimeModel
        {
            public DateTime Foo { get; set; }
            public string Bar { get; set; }
        }

        [Fact]
        public void DeserializeShouldMapNodesInSetMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<Node<City>>(client, CypherResultMode.Set,
                CypherResultFormat.Rest);
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
            var results = deserializer.Deserialize(content, false).ToArray();

            // Assert
            Assert.Equal(3, results.Count());

            var node = results.ElementAt(0);
            Assert.Equal(5, node.Reference.Id);
            Assert.Equal("東京", node.Data.Name);

            node = results.ElementAt(1);
            Assert.Equal(4, node.Reference.Id);
            Assert.Equal("London", node.Data.Name);

            node = results.ElementAt(2);
            Assert.Equal(3, node.Reference.Id);
            Assert.Equal("Sydney", node.Data.Name);
        }

        [Fact]
        public void DeserializeShouldMapRelationshipsInSetMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<RelationshipInstance<City>>(client, CypherResultMode.Set, CypherResultFormat.Rest);
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
            var results = deserializer.Deserialize(content, false).ToArray();

            // Assert
            Assert.Equal(3, results.Count());

            var relationships = results.ElementAt(0);
            Assert.Equal("東京", relationships.Data.Name);

            relationships = results.ElementAt(1);
            Assert.Equal("London", relationships.Data.Name);

            relationships = results.ElementAt(2);
            Assert.Equal("Sydney", relationships.Data.Name);
        }

        [Fact]
        public void DeserializeShouldMapIEnumerableOfRelationshipsInAProjectionMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<Projection>(client, CypherResultMode.Projection, CypherResultFormat.Rest);
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
            var results = deserializer.Deserialize(content, false).ToArray();

            // Assert
            var result = results[0];
            Assert.Equal("東京", result.Node.Data.Name);
            Assert.Equal(13000000, result.Node.Data.Population);
            Assert.Equal(66, result.Relationships.First().Data.Number);

            result = results[1];
            Assert.Equal("東京", result.Node.Data.Name);
            Assert.Equal(13000000, result.Node.Data.Population);
            Assert.Equal(66, result.Relationships.ToArray()[0].Data.Number);
            Assert.Equal(77, result.Relationships.ToArray()[1].Data.Number);

            result = results[2];
            Assert.Equal("東京", result.Node.Data.Name);
            Assert.Equal(13000000, result.Node.Data.Population);
            Assert.Equal(77, result.Relationships.First().Data.Number);
        }

        [Fact]
        public void DeserializeShouldMapIEnumerableOfNodesReturnedByCollectInAProjectionMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<ResultWithNestedNodeDto>(client, CypherResultMode.Projection, CypherResultFormat.Rest);
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
            var results = deserializer.Deserialize(content, false).ToArray();

            Assert.IsAssignableFrom<IEnumerable<ResultWithNestedNodeDto>>(results);
            Assert.Equal(1, results.Count());
            Assert.Equal(2, results[0].Fooness.Count());

            Assert.Equal("bar", results[0].Fooness.ToArray()[0].Data.Bar);
            Assert.Equal("baz", results[0].Fooness.ToArray()[0].Data.Baz);

            Assert.Equal("bar", results[0].Fooness.ToArray()[1].Data.Bar);
            Assert.Equal("baz", results[0].Fooness.ToArray()[1].Data.Baz);

        }

        [Fact]
        public void DeserializeShouldMapIEnumerableOfNodesReturnedByCollectInColumnInProjectionMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<ProjectionFeedItem>(client, CypherResultMode.Projection, CypherResultFormat.DependsOnEnvironment);
            
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
            var results = deserializer.Deserialize(content, false).ToArray();
            
            Assert.Equal(1, results.Count());
            Assert.NotNull(results[0].Post);
            Assert.NotNull(results[0].User);
            Assert.NotNull(results[0].Likers);
            
            Assert.IsAssignableFrom<IEnumerable<User>>(results[0].Likers);
            
            Assert.Equal(2, results[0].Likers.Count());
            Assert.Equal("USER1", results[0].User.UserIdentifier);
            Assert.Equal("USER1POST0", results[0].Post.PostIdentifier);
            //and make sure the likers properties have been set
            Assert.Equal("USER1", results[0].Likers.ToArray()[0].UserIdentifier);
            Assert.Equal("USER2", results[0].Likers.ToArray()[1].UserIdentifier);
        }

        [Fact]
        public void DeserializeShouldMapNullIEnumerableOfNodesReturnedByCollectInInAProjectionMode()
        {

            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<ResultWithNestedNodeDto>(client, CypherResultMode.Projection, CypherResultFormat.DependsOnEnvironment);
            var content =
                        @"{
                      'data' : [ [ [ null ] ] ],
                      'columns' : ['Fooness']
                            }"
                            .Replace('\'', '"');

            var results = deserializer.Deserialize(content, false).ToArray();

            Assert.IsAssignableFrom<IEnumerable<ResultWithNestedNodeDto>>(results);
            Assert.Equal(1, results.Count());

            Assert.Null(results[0].Fooness);
        }

        [Fact]
        public void DeserializeShouldMapIEnumerableOfStringsInAProjectionMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<People>(client, CypherResultMode.Projection, CypherResultFormat.DependsOnEnvironment);
            var content = @"{
                  'data' : [ [ [ 'Ben Tu', 'Romiko Derbynew' ] ] ],
                  'columns' : [ 'Names' ]
                }".Replace("'", "\"");

            // Act
            var results = deserializer.Deserialize(content, false).ToArray().First().Names.ToArray();

            // Assert
            Assert.Equal("Ben Tu", results[0]);
            Assert.Equal("Romiko Derbynew", results[1]);
        }

        [Fact]
        public void DeserializeShouldMapIEnumerableOfStringsThatAreEmptyInAProjectionMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<People>(client, CypherResultMode.Projection, CypherResultFormat.DependsOnEnvironment);
            var content = @"{
                  'data' : [ [ [ ] ] ],
                  'columns' : [ 'Names' ]
                }".Replace("'", "\"");

            // Act
            var results = deserializer.Deserialize(content, false).ToArray();

            // Assert
            Assert.Equal(0, results.First().Names.Count());
        }

        [Fact]
        public void DeserializeShouldMapIEnumerableOfStringsInSetMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<IEnumerable<string>>(client, CypherResultMode.Set, CypherResultFormat.DependsOnEnvironment);
            var content = @"{
                  'data' : [ [ [ 'Ben Tu', 'Romiko Derbynew' ] ] ],
                  'columns' : [ 'Names' ]
                }".Replace("'", "\"");

            // Act
            var results = deserializer.Deserialize(content, false).ToArray().First().ToArray();

            // Assert
            Assert.Equal("Ben Tu", results[0]);
            Assert.Equal("Romiko Derbynew", results[1]);
        }

        [Fact]
        public void DeserializeShouldMapArrayOfStringsInSetMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<string[]>(client, CypherResultMode.Set, CypherResultFormat.DependsOnEnvironment);
            var content = @"{
                  'data' : [ [ [ 'Ben Tu', 'Romiko Derbynew' ] ] ],
                  'columns' : [ 'Names' ]
                }".Replace("'", "\"");

            // Act
            var results = deserializer.Deserialize(content, false).ToArray().First().ToArray();

            // Assert
            Assert.Equal("Ben Tu", results[0]);
            Assert.Equal("Romiko Derbynew", results[1]);
        }

        [Fact]
        public void DeserializeShouldMapArrayOfIntegersInSetMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<int[]>(client, CypherResultMode.Set, CypherResultFormat.DependsOnEnvironment);
            var content = @"{
                  'data' : [ [ [ '666', '777' ] ] ],
                  'columns' : [ 'Names' ]
                }".Replace("'", "\"");

            // Act
            var results = deserializer.Deserialize(content, false).ToArray().First().ToArray();

            // Assert
            Assert.Equal(666, results[0]);
            Assert.Equal(777, results[1]);
        }

        [Fact]
        public void DeserializeShouldMapIntegerInSetMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<int>(client, CypherResultMode.Set, CypherResultFormat.DependsOnEnvironment);
            var content = @"{
                          'data' : [ [ 666 ] ],
                          'columns' : [ 'count(distinct registration)' ]
                        }".Replace("'", "\"");

            // Act
            var results = deserializer.Deserialize(content, false).ToArray();

            // Assert
            Assert.Equal(666, results.First());

        }

        [Fact]
        public void DeserializeShouldRespectJsonPropertyAttribute()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<UserWithJsonPropertyAttribute>(client, CypherResultMode.Set, CypherResultFormat.DependsOnEnvironment);
            var content = @"{
  'columns' : [ 'Foo' ],
  'data' : [ [ {
    'paged_traverse' : 'http://localhost:8000/db/data/node/740/paged/traverse/{returnType}{?pageSize,leaseTime}',
    'outgoing_relationships' : 'http://localhost:8000/db/data/node/740/relationships/out',
    'data' : {
      'givenName' : 'Bob'
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
            var results = deserializer.Deserialize(content, false).ToArray();

            // Assert
            Assert.Equal("Bob", results.Single().Name);
        }

        [Fact]
        //[Description("https://bitbucket.org/Readify/neo4jclient/issue/63")]
        public void DeserializeShouldMapNullCollectResultsWithOtherProperties()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<ModelWithCollect>(client, CypherResultMode.Projection, CypherResultFormat.DependsOnEnvironment);
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
            var results = deserializer.Deserialize(content, false).ToArray();

            // Assert
            Assert.Equal(1, results.Count());
            Assert.Equal(null, results[0].Fans);
            Assert.IsAssignableFrom<Node<User>>(results[0].Poster);
            Assert.Equal(740, results[0].Poster.Reference.Id);
            Assert.Equal("Bob", results[0].Poster.Data.GivenName);
        }

        [Fact]
        //[Description("https://bitbucket.org/Readify/neo4jclient/issue/67")]
        public void DeserializeShouldMapNullCollectResultsWithOtherProperties_Test2()
        {
            // START app=node(0) MATCH app<-[?:Alternative]-alternatives RETURN app AS App, collect(alternatives) AS Alternatives;

            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<AppAlternatives>(client, CypherResultMode.Projection, CypherResultFormat.DependsOnEnvironment);
            var content = @"{
  'columns' : [ 'Application', 'Alternatives' ],
  'data' : [ [ {
    'paged_traverse' : 'http://localhost:8000/db/data/node/123/paged/traverse/{returnType}{?pageSize,leaseTime}',
    'outgoing_relationships' : 'http://localhost:8000/db/data/node/123/relationships/out',
    'data' : {
    },
    'all_typed_relationships' : 'http://localhost:8000/db/data/node/123/relationships/all/{-list|&|types}',
    'traverse' : 'http://localhost:8000/db/data/node/123/traverse/{returnType}',
    'all_relationships' : 'http://localhost:8000/db/data/node/123/relationships/all',
    'property' : 'http://localhost:8000/db/data/node/123/properties/{key}',
    'self' : 'http://localhost:8000/db/data/node/123',
    'outgoing_typed_relationships' : 'http://localhost:8000/db/data/node/123/relationships/out/{-list|&|types}',
    'properties' : 'http://localhost:8000/db/data/node/123/properties',
    'incoming_relationships' : 'http://localhost:8000/db/data/node/123/relationships/in',
    'incoming_typed_relationships' : 'http://localhost:8000/db/data/node/123/relationships/in/{-list|&|types}',
    'extensions' : {
    },
    'create_relationship' : 'http://localhost:8000/db/data/node/123/relationships'
  }, [ null ] ] ]
}".Replace("'", "\"");

            // Act
            var results = deserializer.Deserialize(content, false).ToArray();

            // Assert
            Assert.Equal(1, results.Count());
            Assert.Null(results[0].Alternatives);
            Assert.IsAssignableFrom<Node<App>>(results[0].Application);
            Assert.Equal(123, results[0].Application.Reference.Id);
        }

        [Fact]
        //[Description("https://bitbucket.org/Readify/neo4jclient/issue/67")]
        public void DeserializeShouldMapNullCollectResultsWithOtherProperties_Test3()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<ModelWithCollect>(client, CypherResultMode.Projection, CypherResultFormat.DependsOnEnvironment);
            var content = @"{'columns':['Fans'],'data':[[[null,null]]]}".Replace("'", "\"");

            // Act
            var results = deserializer.Deserialize(content, false).ToArray();

            // Assert
            Assert.Equal(1, results.Count());
            Assert.Null(results[0].Fans);
        }

        [Fact]
        //[Description("http://stackoverflow.com/questions/23764217/argumentexception-when-result-is-empty")]
        public void DeserializeShouldMapNullResult()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<object>(client, CypherResultMode.Set, CypherResultFormat.DependsOnEnvironment);
            var content = @"{ 'columns' : [ 'db' ], 'data' : [ [ null ] ] }".Replace("'", "\"");

            // Act
            var results = deserializer.Deserialize(content, false).ToArray();

            // Assert
            Assert.Equal(1, results.Count());
            Assert.Null(results[0]);
        }

        [Fact]
        public void DeserializeShouldMapProjectionIntoAnonymousTypeTest()
        {
            DeserializeShouldMapProjectionIntoAnonymousType(new { Name = "", Population = 0 });
        }

// ReSharper disable UnusedParameter.Local
        static void DeserializeShouldMapProjectionIntoAnonymousType<TAnon>(TAnon dummy)
// ReSharper restore UnusedParameter.Local
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<TAnon>(client, CypherResultMode.Projection, CypherResultFormat.DependsOnEnvironment);
            var content = @"{
                'data' : [
                    [ 'Tokyo', 13000000 ],
                    [ 'London', 8000000 ],
                    [ 'Sydney', 4000000 ]
                ],
                'columns' : [ 'Name', 'Population' ]
            }".Replace("'", "\"");

            // Act
            var results = deserializer.Deserialize(content, false).ToArray();

            // Assert
            Assert.Equal(3, results.Count());

            dynamic city = results.ElementAt(0);
            Assert.Equal("Tokyo", city.Name);
            Assert.Equal(13000000, city.Population);

            city = results.ElementAt(1);
            Assert.Equal("London", city.Name);
            Assert.Equal(8000000, city.Population);

            city = results.ElementAt(2);
            Assert.Equal("Sydney", city.Name);
            Assert.Equal(4000000, city.Population);
        }

        [Fact]
        //[Description("https://bitbucket.org/Readify/neo4jclient/issue/67/cypher-deserialization-error-when-using")]
        public void DeserializeShouldMapProjectionIntoAnonymousTypeWithNullCollectResultTest()
        {
            DeserializeShouldMapProjectionIntoAnonymousTypeWithNullCollectResult(new { Name = "", Friends = new object[0] });
        }

        // ReSharper disable UnusedParameter.Local
        static void DeserializeShouldMapProjectionIntoAnonymousTypeWithNullCollectResult<TAnon>(TAnon dummy)
        // ReSharper restore UnusedParameter.Local
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<TAnon>(client, CypherResultMode.Projection, CypherResultFormat.DependsOnEnvironment);
            var content = @"{
                'data' : [
                    [ 'Jim', [null] ]
                ],
                'columns' : [ 'Name', 'Friends' ]
            }".Replace("'", "\"");

            // Act
            var results = deserializer.Deserialize(content, false).ToArray();

            // Assert
            Assert.Equal(1, results.Count());

            dynamic person = results.ElementAt(0);
            Assert.Equal("Jim", person.Name);
            Assert.Equal(null, person.Friends);
        }

        public class App
        {
        }

        public class AppAlternatives
        {
            public Node<App> Application { get; set; }
            public IEnumerable<Node<App>> Alternatives { get; set; }
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

        public class UserWithJsonPropertyAttribute
        {
            [JsonProperty("givenName")]
            public string Name { get; set; }
        }

        private class CityAndLabel
        {
            public City City { get; set; }
            public IEnumerable<string> Labels { get; set; }
        }

        private class State
        {
            public string Name { get; set; }
        }

        private class StateCityAndLabel
        {
            public State State { get; set; }
            public IEnumerable<string> Labels { get; set; }
            public IEnumerable<City> Cities { get; set; }
        }

        private class StateCityAndLabelWithNode
        {
            public State State { get; set; }
            public IEnumerable<string> Labels { get; set; }
            public IEnumerable<Node<City>> Cities { get; set; }
        }

        [Fact (Skip = "Not valid anymore?")]
        public void DeserializeNestedObjectsInTransactionReturningNode()
        {
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<StateCityAndLabelWithNode>(client, CypherResultMode.Projection, CypherResultFormat.Rest, true);
            var content = @"{'results':[
                {
                    'columns':[
                        'State','Labels','Cities'
                    ],
                    'data':[
                        {
                            'rest':[
                                {'Name':'Baja California'},
                                ['State'],
                                [
                                    {
                    'outgoing_relationships' : 'http://localhost:7474/db/data/node/5/relationships/out',
                    'data' : {
                      'Name' : 'Tijuana',
                      'Population': 1300000
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
                  } , {
                    'outgoing_relationships' : 'http://localhost:7474/db/data/node/4/relationships/out',
                    'data' : {
                      'Name' : 'Mexicali',
                      'Population': 500000
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
                  }
                                ]
                            ]
                        }
                    ]
                }
            ]}";
            var results = deserializer.Deserialize(content, false).ToArray();
            Assert.Equal(1, results.Length);
            var result = results[0];
            Assert.Equal("Baja California", result.State.Name);
            Assert.Equal("State", result.Labels.First());

            var cities = result.Cities.ToArray();
            Assert.Equal(2, cities.Length);

            var city = cities[0];
            Assert.Equal("Tijuana", city.Data.Name);
            Assert.Equal(1300000, city.Data.Population);

            city = cities[1];
            Assert.Equal("Mexicali", city.Data.Name);
            Assert.Equal(500000, city.Data.Population);
        }

        [Fact]
        public void DeserializeNestedObjectsInTransaction()
        {
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<StateCityAndLabel>(client, CypherResultMode.Projection, CypherResultFormat.DependsOnEnvironment, true);
            var content = @"{'results':[
                {
                    'columns':[
                        'State','Labels','Cities'
                    ],
                    'data':[
                        {
                            'row':[
                                {'Name':'Baja California'},
                                ['State'],
                                [
                                    {'Name':'Tijuana', 'Population': 1300000},
                                    {'Name':'Mexicali', 'Population': 500000}
                                ]
                            ]
                        }
                    ]
                }
            ]}";
            var results = deserializer.Deserialize(content, false).ToArray();
            Assert.Equal(1, results.Length);
            var result = results[0];
            Assert.Equal("Baja California", result.State.Name);
            Assert.Equal("State", result.Labels.First());

            var cities = result.Cities.ToArray();
            Assert.Equal(2, cities.Length);

            var city = cities[0];
            Assert.Equal("Tijuana", city.Name);
            Assert.Equal(1300000, city.Population);

            city = cities[1];
            Assert.Equal("Mexicali", city.Name);
            Assert.Equal(500000, city.Population);
        }


        [Fact]
        public void DeserializerTwoLevelProjectionInTransaction()
        {
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<CityAndLabel>(client, CypherResultMode.Projection, CypherResultFormat.DependsOnEnvironment, true);
            var content = @"{'results':[
                {
                    'columns':[
                        'City', 'Labels'
                    ],
                    'data':[
                        {
                            'row': [{ 'Name': 'Sydney', 'Population': 4000000}, ['City1']]
                        },
                        {
                            'row': [{ 'Name': 'Tijuana', 'Population': 1300000}, ['City2']]
                        }
                    ]
                }
            ]}";
            var results = deserializer.Deserialize(content, false).ToArray();
            Assert.Equal(2, results.Length);
            var city = results[0];
            Assert.Equal("Sydney", city.City.Name);
            Assert.Equal(4000000, city.City.Population);
            Assert.Equal("City1", city.Labels.First());
            city = results[1];
            Assert.Equal("Tijuana", city.City.Name);
            Assert.Equal(1300000, city.City.Population);
            Assert.Equal("City2", city.Labels.First());
        }

        [Fact]
        public void DeserializerProjectionInTransaction()
        {
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<City>(client, CypherResultMode.Projection, CypherResultFormat.DependsOnEnvironment, true);
            var content = @"{'results':[
                {
                    'columns':[
                        'Name', 'Population'
                    ],
                    'data':[
                        {
                            'row': ['Sydney', 4000000]
                        },
                        {
                            'row': ['Tijuana', 1300000]
                        }
                    ]
                }
            ]}";
            var results = deserializer.Deserialize(content, false).ToArray();
            Assert.Equal(2, results.Length);
            var city = results[0];
            Assert.Equal("Sydney", city.Name);
            Assert.Equal(4000000, city.Population);
            city = results[1];
            Assert.Equal("Tijuana", city.Name);
            Assert.Equal(1300000, city.Population);
        }

        [Fact]
        public void DeserializeSimpleSetInTransaction()
        {
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<int>(client, CypherResultMode.Set, CypherResultFormat.DependsOnEnvironment, true);
            var content = @"{'results':[
                {
                    'columns':[
                        'count(n)'
                    ],
                    'data':[
                        {
                            'row': [3]
                        }
                    ]
                }
            ]}";
            var results = deserializer.Deserialize(content, false).ToArray();
            Assert.Equal(1, results.Length);
            Assert.Equal(3, results[0]);
        }

        [Fact]
        public void DeserializeResultsSetInTransaction()
        {
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<City>(client, CypherResultMode.Set, CypherResultFormat.DependsOnEnvironment, true);
            var content = @"{'results':[
                {
                    'columns':[
                        'c'
                    ],
                    'data':[
                        {
                            'row': [{
                                'Name': 'Sydney', 'Population': 4000000
                            }]
                        }
                    ]
                }
            ]}";
            var results = deserializer.Deserialize(content, false).ToArray();
            Assert.Equal(1, results.Length);
            var city = results[0];
            Assert.Equal("Sydney", city.Name);
            Assert.Equal(4000000, city.Population);
        }

        [Fact]
        public void DeserializeShouldPreserveUtf8Characters()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<City>(client, CypherResultMode.Set, CypherResultFormat.DependsOnEnvironment);
            var content = @"{
                  'data' : [
                    [ { 'Name': '東京', 'Population': 13000000 } ],
                    [ { 'Name': 'London', 'Population': 8000000 } ],
                    [ { 'Name': 'Sydney', 'Population': 4000000 } ]
                  ],
                  'columns' : [ 'Cities' ]
                }".Replace("'", "\"");

            // Act
            var results = deserializer.Deserialize(content, false).ToArray();

            // Assert
            Assert.Equal(3, results.Count());

            var city = results.ElementAt(0);
            Assert.Equal("東京", city.Name);
            Assert.Equal(13000000, city.Population);

            city = results.ElementAt(1);
            Assert.Equal("London", city.Name);
            Assert.Equal(8000000, city.Population);

            city = results.ElementAt(2);
            Assert.Equal("Sydney", city.Name);
            Assert.Equal(4000000, city.Population);
        }

        [Fact]
        public void DeserializeShouldMapNodesToObjectsInSetModeWhenTheSourceLooksLikeANodeButTheDestinationDoesnt()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<Asset>(client, CypherResultMode.Set, CypherResultFormat.DependsOnEnvironment);
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
            var results = deserializer.Deserialize(content, false).ToArray();

            // Assert
            var resultsArray = results.ToArray();
            Assert.Equal(2, resultsArray.Count());

            var firstResult = resultsArray[0];
            Assert.Equal("67", firstResult.Name);
            Assert.Equal(2, firstResult.UniqueId);

            var secondResult = resultsArray[1];
            Assert.Equal("15 Mile", secondResult.Name);
            Assert.Equal(1, secondResult.UniqueId);
        }

        [Fact]
        public void BadJsonShouldThrowExceptionThatIncludesJson()
        {
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<Asset>(client, CypherResultMode.Set, CypherResultFormat.DependsOnEnvironment);
            const string content = @"xyz-json-zyx";

            var ex = Assert.Throws<ArgumentException>(() =>
                deserializer.Deserialize(content, false)
            );
            Assert.Contains(content, ex.Message);
        }

        [Fact]
        public void BadJsonShouldThrowExceptionThatIncludesFullNameOfTargetType()
        {
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<Asset>(client, CypherResultMode.Set, CypherResultFormat.DependsOnEnvironment);

            var ex = Assert.Throws<ArgumentException>(() =>
                deserializer.Deserialize("xyz-json-zyx", true)
            );
            Assert.Contains(typeof(Asset).FullName, ex.Message);
        }

        [Fact]
        public void ClassWithoutDefaultPublicConstructorShouldThrowExceptionThatExplainsThis()
        {
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<ClassWithoutDefaultPublicConstructor>(client, CypherResultMode.Set, CypherResultFormat.DependsOnEnvironment);
            var content = @"{
                  'data' : [
                    [ { 'Name': 'Tokyo', 'Population': 13000000 } ],
                    [ { 'Name': 'London', 'Population': 8000000 } ],
                    [ { 'Name': 'Sydney', 'Population': 4000000 } ]
                  ],
                  'columns' : [ 'Cities' ]
                }".Replace("'", "\"");

            var ex = Assert.Throws<ArgumentException>(() => deserializer.Deserialize(content, false));
            Assert.StartsWith("We expected a default public constructor on ClassWithoutDefaultPublicConstructor so that we could create instances of it to deserialize data into, however this constructor does not exist or is inaccessible.", ex.Message);
        }

        public class ClassWithoutDefaultPublicConstructor
        {
            public int A { get; set; }

            public ClassWithoutDefaultPublicConstructor(int a)
            {
                A = a;
            }
        }

        public class Asset
        {
            public long UniqueId { get; set; }
            public string Name { get; set; }
        }

        [Fact]
        //[Description("https://bitbucket.org/Readify/neo4jclient/issue/162/deserialization-of-int-long-into-nullable")]
        public void DeserializeInt64IntoNullableInt64()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<long?>(client, CypherResultMode.Set, CypherResultFormat.DependsOnEnvironment);
            var content = @"{
                          'data' : [ [ 123 ] ],
                          'columns' : [ 'count(distinct registration)' ]
                        }".Replace("'", "\"");

            // Act
            var result = deserializer.Deserialize(content, false).First();

            // Assert
            Assert.Equal(123, result);
        }

        public class ModelWithByteArray
        {
            public byte[] Array { get; set; }
        }

        [Fact]
        //[Description("https://bitbucket.org/Readify/neo4jclient/issue/114/byte-support")]
        public void DeserializeBase64StringIntoByteArrayInProjectionResultMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<ModelWithByteArray>(client, CypherResultMode.Projection, CypherResultFormat.DependsOnEnvironment);
            var content = @"{
                          'data' : [ [ 'AQIDBA==' ] ],
                          'columns' : [ 'Array' ]
                        }".Replace("'", "\"");

            // Act
            var result = deserializer.Deserialize(content, false).First();

            // Assert
            Assert.Equal(new byte[] {1, 2, 3, 4}, result.Array);
        }

        [Fact]
        //[Description("https://bitbucket.org/Readify/neo4jclient/issue/114/byte-support")]
        public void DeserializeBase64StringIntoByteArrayInSetResultMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new CypherJsonDeserializer<byte[]>(client, CypherResultMode.Set, CypherResultFormat.DependsOnEnvironment);
            var content = @"{
                          'data' : [ [ 'AQIDBA==' ] ],
                          'columns' : [ 'column1' ]
                        }".Replace("'", "\"");

            // Act
            var result = deserializer.Deserialize(content, false).First();

            // Assert
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, result);
        }
    }
}
