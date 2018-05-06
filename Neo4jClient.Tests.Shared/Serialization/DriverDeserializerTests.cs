using System;
using System.Linq;
using NSubstitute;
using Xunit;
using System.Collections.Generic;
using Moq;
using Neo4j.Driver.V1;
using Neo4jClient.Cypher;
using Neo4jClient.Serialization;
using Neo4jClient.Serialization.BoltDriver;
using Neo4jClient.Serialization.Json;
using Neo4jClient.Test.BoltGraphClientTests;
using Neo4jClient.Test.Fixtures;
using Newtonsoft.Json;

namespace Neo4jClient.Tests.Shared.Serialization
{
    public class DriverDeserializerTests : IClassFixture<CultureInfoSetupFixture>
    {
        private static IRelationship GenerateMockRelationship(Dictionary<string, object> data, long relId,
            string type, long start, long end)
        {
            var relMock = new Mock<IRelationship>();
            relMock
                .Setup(r => r.StartNodeId)
                .Returns(start);
            relMock
                .Setup(r => r.EndNodeId)
                .Returns(end);
            relMock
                .Setup(r => r.Properties)
                .Returns(data);
            relMock
                .Setup(r => r.Type)
                .Returns(type);
            relMock
                .Setup(r => r.Id)
                .Returns(relId);

            return relMock.Object;
        }

        private static INode GenerateMockNode(Dictionary<string, object> data, long? nodeId = null, List<string> labels = null)
        {
            var nodeMock = new Mock<INode>();
            nodeMock
                .Setup(n => n.Properties)
                .Returns(data);

            nodeMock
                .Setup(n => n.Labels)
                .Returns(labels ?? new List<string>() {"Node"});

            nodeMock
                .Setup(n => n.Id)
                .Returns(nodeId ?? 1);

            return nodeMock.Object;
        }

        private static IRecord GenerateMockRecord(Dictionary<string, object> data)
        {
            var recordMock = Substitute.For<IRecord>();
            recordMock.Values.Returns(data);

            recordMock.Keys.Returns(data.Keys.ToList());

            foreach (var key in data.Keys)
            {
                recordMock[key].Returns(r => data[key]);
            }
            

            return recordMock;
        }

        private static IRecord GenerateMockRelationshipRecord(string columnName, Dictionary<string, object> data, long relId,
            string type, long start, long end)
        {
            var recordData = new Dictionary<string, object>()
            {
                {columnName, GenerateMockRelationship(data, relId, type, start, end)}
            };

            return GenerateMockRecord(recordData);
        }

        private static IRecord GenerateMockNodeRecord(string columnName, Dictionary<string, object> data,
            long? nodeId = null, List<string> labels = null)
        {
            var recordData = new Dictionary<string, object>()
            {
                {columnName, GenerateMockNode(data, nodeId, labels)}
            };

            return GenerateMockRecord(recordData);
        }



        private static readonly Func<string, IStatementResult> GenerateResultsInSetMode = dateTime =>
        {
            var recordMock = GenerateMockRecord(new Dictionary<string, object>()
            {
                {
                    "a", new Dictionary<string, object>()
                    {
                        {"Foo", dateTime},
                        {"Bar", "Bar"}
                    }
                }
            });

            var testStatementResult = new TestStatementResult(new[] {"a"}, recordMock);
            return testStatementResult;
        };

        private static readonly Func<string, IStatementResult> GenerateResultsInProjectionMode = dateTime =>
        {
            var recordMock = GenerateMockRecord(new Dictionary<string, object>()
            {
                {"Foo", dateTime},
                {"Bar", "Bar"}
            });

            var testStatementResult = new TestStatementResult(new[] { "Foo", "Bar" }, recordMock);
            return testStatementResult;
        };

        private class DateTimeTestCasesFactory
        {
            public static IEnumerable<object[]> TestCases
            {
                get
                {
                    yield return new object[] { "2015-06-01T15:03:39.1462808", DateTimeKind.Unspecified };
                    yield return new object[] { "2015-06-01T15:03:39.1462808Z", DateTimeKind.Utc };
                    yield return new object[] { "2015-06-01T15:03:39.1462808+00:00", DateTimeKind.Local };
                }
            }
        }


        private class DateTimeOffsetCasesFactory
        {
            public static IEnumerable<object[]> TestCases
            {
                get
                {
                    yield return new object[] { CypherResultMode.Set, GenerateResultsInSetMode, "", null };
                    yield return new object[] { CypherResultMode.Set, GenerateResultsInSetMode, "rekjre", null };
                    yield return new object[] { CypherResultMode.Set, GenerateResultsInSetMode, "/Date(abcs)/", null };
                    yield return new object[] { CypherResultMode.Set, GenerateResultsInSetMode, "/Date(abcs+0000)/", null };
                    yield return new object[] { CypherResultMode.Set, GenerateResultsInSetMode, "/Date(1315271562384)/", "2011-09-06 01:12:42 +00:00" };
                    yield return new object[] { CypherResultMode.Set, GenerateResultsInSetMode, "/Date(1315271562384+0000)/", "2011-09-06 01:12:42 +00:00" };
                    yield return new object[] { CypherResultMode.Set, GenerateResultsInSetMode, "/Date(1315271562384+0200)/", "2011-09-06 03:12:42 +02:00" };
                    yield return new object[] { CypherResultMode.Set, GenerateResultsInSetMode, "/Date(1315271562384+1000)/", "2011-09-06 11:12:42 +10:00" };
                    yield return new object[] { CypherResultMode.Set, GenerateResultsInSetMode, "/Date(-2187290565386+0000)/", "1900-09-09 03:17:14 +00:00" };
                    yield return new object[] { CypherResultMode.Set, GenerateResultsInSetMode, "2011-09-06T01:12:42+10:00", "2011-09-06 01:12:42 +10:00" };
                    yield return new object[] { CypherResultMode.Set, GenerateResultsInSetMode, "2011-09-06T01:12:42+00:00", "2011-09-06 01:12:42 +00:00" };
                    yield return new object[] { CypherResultMode.Set, GenerateResultsInSetMode, "2012-08-31T10:11:00.3642578+10:00", "2012-08-31 10:11:00 +10:00" };
                    yield return new object[] { CypherResultMode.Projection, GenerateResultsInProjectionMode, "", null };
                    yield return new object[] { CypherResultMode.Projection, GenerateResultsInProjectionMode, "rekjre", null };
                    yield return new object[] { CypherResultMode.Projection, GenerateResultsInProjectionMode, "/Date(abcs)/", null };
                    yield return new object[] { CypherResultMode.Projection, GenerateResultsInProjectionMode, "/Date(abcs+0000)/", null };
                    yield return new object[] { CypherResultMode.Projection, GenerateResultsInProjectionMode, "/Date(1315271562384)/", "2011-09-06 01:12:42 +00:00" };
                    yield return new object[] { CypherResultMode.Projection, GenerateResultsInProjectionMode, "/Date(1315271562384+0000)/", "2011-09-06 01:12:42 +00:00" };
                    yield return new object[] { CypherResultMode.Projection, GenerateResultsInProjectionMode, "/Date(1315271562384+0200)/", "2011-09-06 03:12:42 +02:00" };
                    yield return new object[] { CypherResultMode.Projection, GenerateResultsInProjectionMode, "/Date(1315271562384+1000)/", "2011-09-06 11:12:42 +10:00" };
                    yield return new object[] { CypherResultMode.Projection, GenerateResultsInProjectionMode, "/Date(-2187290565386+0000)/", "1900-09-09 03:17:14 +00:00" };
                    yield return new object[] { CypherResultMode.Projection, GenerateResultsInProjectionMode, "2011-09-06T01:12:42+10:00", "2011-09-06 01:12:42 +10:00" };
                    yield return new object[] { CypherResultMode.Projection, GenerateResultsInProjectionMode, "2011-09-06T01:12:42+00:00", "2011-09-06 01:12:42 +00:00" };
                    yield return new object[] { CypherResultMode.Projection, GenerateResultsInProjectionMode, "2012-08-31T10:11:00.3642578+10:00", "2012-08-31 10:11:00 +10:00" };
                }
            }
        }

        [Theory]
        [MemberData(nameof(DateTimeOffsetCasesFactory.TestCases), MemberType = typeof(DateTimeOffsetCasesFactory))]
        public void DeserializeShouldPreserveOffsetValues(CypherResultMode resultMode, Func<string, IStatementResult> generateSerializedContent, 
            string input, string expectedResult)
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new DriverDeserializer<DateTimeOffsetModel>(client, resultMode);
            var content = generateSerializedContent(input);

            // Act
            var result = deserializer.Deserialize(content).Single();

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
            var deserializer = new DriverDeserializer<DateTimeModel>(client, CypherResultMode.Projection);
            var content = GenerateResultsInProjectionMode(dateTime);

            //Act
            var result = deserializer.Deserialize(content).Single();

            //Assert
            Assert.Equal(result.Foo.Kind, kind);
        }

        [Theory]
        [MemberData(nameof(DateTimeTestCasesFactory.TestCases), MemberType = typeof(DateTimeTestCasesFactory))]
        public void DeserializeDateShouldPreservePointInTime(string dateTime, DateTimeKind kind)
        {
            //Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new DriverDeserializer<DateTimeModel>(client, CypherResultMode.Projection);
            var content = GenerateResultsInProjectionMode(dateTime);

            //Act
            var result = deserializer.Deserialize(content).Single();

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
            var deserializer = new DriverDeserializer<Node<City>>(client, CypherResultMode.Set);

            var content = new TestStatementResult(
                new []{"c"},
                GenerateMockNodeRecord("c", new Dictionary<string, object>(){{"Name", "東京" } }, 5),
                GenerateMockNodeRecord("c", new Dictionary<string, object>() { { "Name", "London" } }, 4),
                GenerateMockNodeRecord("c", new Dictionary<string, object>() { { "Name", "Sydney" } }, 3));

            // Act
            var results = deserializer.Deserialize(content).ToArray();

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

            var content = new TestStatementResult(
                new[] { "c" },
                GenerateMockRelationshipRecord("c", new Dictionary<string, object>() { { "Name", "東京" } }, 76931,
                    "REFERRAL_HAS_WHO_SECTION", 55872, 55875),
                GenerateMockRelationshipRecord("c", new Dictionary<string, object>() { { "Name", "London" } }, 76931,
                    "REFERRAL_HAS_WHO_SECTION", 55872, 55875),
                GenerateMockRelationshipRecord("c", new Dictionary<string, object>() { { "Name", "Sydney" } }, 76931,
                    "REFERRAL_HAS_WHO_SECTION", 55872, 55875));

            var deserializer = new DriverDeserializer<RelationshipInstance<City>>(client, CypherResultMode.Set);
            // Act
            var results = deserializer.Deserialize(content).ToArray();

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
            var deserializer = new DriverDeserializer<Projection>(client, CypherResultMode.Projection);

            var content = new TestStatementResult(
                new[] {"Node", "Relationships"},
                GenerateMockRecord(
                    new Dictionary<string, object>()
                    {
                        {
                            "Node", GenerateMockNode(new Dictionary<string, object>
                            {
                                {"Name", "東京"},
                                {"Population", 13000000}

                            }, 55745)
                        },
                        {
                            "Relationships", new List<IRelationship>()
                            {
                                GenerateMockRelationship(new Dictionary<string, object>()
                                {
                                    {"Number", 66}
                                }, 76743, "REFERRAL_HAS_WHO_SECTION", 55745, 55747)
                            }
                        },
                    }),
                GenerateMockRecord(
                    new Dictionary<string, object>()
                    {
                        {
                            "Node", GenerateMockNode(new Dictionary<string, object>
                            {
                                {"Name", "東京"},
                                {"Population", 13000000}

                            }, 55745)
                        },
                        {
                            "Relationships", new List<IRelationship>()
                            {
                                GenerateMockRelationship(new Dictionary<string, object>()
                                {
                                    {"Number", 66}
                                }, 76743, "REFERRAL_HAS_WHO_SECTION", 55745, 55747),
                                GenerateMockRelationship(new Dictionary<string, object>()
                                {
                                    {"Number", 77}
                                }, 76745, "HAS_AUDIT", 55747, 55748),
                            }
                        },
                    }),
                GenerateMockRecord(
                    new Dictionary<string, object>()
                    {
                        {
                            "Node", GenerateMockNode(new Dictionary<string, object>
                            {
                                {"Name", "東京"},
                                {"Population", 13000000}

                            }, 55745)
                        },
                        {
                            "Relationships", new List<IRelationship>()
                            {
                                GenerateMockRelationship(new Dictionary<string, object>()
                                    {
                                        {"Number", 77}
                                    }, 76741,
                                    "HAS_AUDIT",
                                    55745,
                                    55746)
                            }
                        },
                    }));

            // Act
            var results = deserializer.Deserialize(content).ToArray();

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
            var deserializer = new DriverDeserializer<ResultWithNestedNodeDto>(client, CypherResultMode.Projection);

            var content = new TestStatementResult(
                new[] {"Fooness"},
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {
                        "Fooness", new List<INode>()
                        {
                            GenerateMockNode(new Dictionary<string, object>()
                            {
                                {"Bar", "bar"},
                                {"Baz", "baz"}
                            }, 920),
                            GenerateMockNode(new Dictionary<string, object>()
                            {
                                {"Bar", "bar"},
                                {"Baz", "baz"}
                            }, 5482)
                        }
                    }
                })
            );

            // Act
            var results = deserializer.Deserialize(content).ToArray();

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
            var deserializer = new DriverDeserializer<ProjectionFeedItem>(client, CypherResultMode.Projection);

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

            var content = new TestStatementResult(
                new[] {"Post", "Likers", "User"},
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {
                        "Post", GenerateMockNode(new Dictionary<string, object>()
                        {
                            {"PostTime", 634881866575852740},
                            {"PostIdentifier", "USER1POST0"},
                            {"Comment", "Here is a post statement 0 posted by user 1"},
                            {"Content", "Blah blah blah"}
                        }, 189)
                    },
                    {
                        "Likers", new List<INode>()
                        {
                            GenerateMockNode(new Dictionary<string, object>()
                            {
                                {"UserIdentifier", "USER1"},
                                {"Email", "someoneelse@something.net"},
                                {"FamilyName", "Jones"},
                                {"GivenName", "bob"}
                            }, 188),
                            GenerateMockNode(new Dictionary<string, object>()
                            {
                                {"UserIdentifier", "USER2"},
                                {"Email", "someone@someotheraddress.net"},
                                {"FamilyName", "Bob"},
                                {"GivenName", "Jimmy"}
                            }, 197)
                        }
                    },
                    {
                        "User", GenerateMockNode(new Dictionary<string, object>()
                        {
                            {"UserIdentifier", "USER1"},
                            {"Email", "someoneelse@something.net"},
                            {"FamilyName", "Jones"},
                            {"GivenName", "bob"}
                        }, 188)
                    }
                })
            );

            // Act
            var results = deserializer.Deserialize(content).ToArray();

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
        public void DeserializeShouldMapIEnumerableOfStringsInAProjectionMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new DriverDeserializer<People>(client, CypherResultMode.Projection);

            var content = new TestStatementResult(new []{"Names"}, GenerateMockRecord(new Dictionary<string, object>()
            {
                {"Names", new List<string>(){"Ben Tu", "Romiko Derbynew"} }
            }));

            // Act
            var results = deserializer.Deserialize(content).ToArray().First().Names.ToArray();

            // Assert
            Assert.Equal("Ben Tu", results[0]);
            Assert.Equal("Romiko Derbynew", results[1]);
        }

        [Fact]
        public void DeserializeShouldMapIEnumerableOfStringsThatAreEmptyInAProjectionMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new DriverDeserializer<People>(client, CypherResultMode.Projection);
            var content = new TestStatementResult(new[] {"Names"}, GenerateMockRecord(new Dictionary<string, object>()
            {
                {"Names", new List<string>()}
            }));

            // Act
            var results = deserializer.Deserialize(content).ToArray();

            // Assert
            Assert.Empty(results.First().Names);
        }

        [Fact]
        public void DeserializeShouldMapIEnumerableOfStringsInSetMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new DriverDeserializer<IEnumerable<string>>(client, CypherResultMode.Set);
            var content = new TestStatementResult(new[] { "Names" }, GenerateMockRecord(new Dictionary<string, object>()
            {
                {"Names", new List<string>(){"Ben Tu", "Romiko Derbynew"} }
            }));

            // Act
            var results = deserializer.Deserialize(content).ToArray().First().ToArray();

            // Assert
            Assert.Equal("Ben Tu", results[0]);
            Assert.Equal("Romiko Derbynew", results[1]);
        }

        [Fact]
        public void DeserializeShouldMapArrayOfIntegersInSetMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new DriverDeserializer<int[]>(client, CypherResultMode.Set);

            var content = new TestStatementResult(new[] { "Names" }, GenerateMockRecord(new Dictionary<string, object>()
            {
                {"Names", new List<int>(){666, 777} }
            }));

            // Act
            var results = deserializer.Deserialize(content).ToArray().First().ToArray();

            // Assert
            Assert.Equal(666, results[0]);
            Assert.Equal(777, results[1]);
        }

        [Fact]
        public void DeserializeShouldMapIntegerInSetMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new DriverDeserializer<int>(client, CypherResultMode.Set);

            var content = new TestStatementResult(new[] { "count(distinct registration)" }, GenerateMockRecord(new Dictionary<string, object>()
            {
                {"count(distinct registration)", 666}
            }));

            // Act
            var results = deserializer.Deserialize(content).ToArray();

            // Assert
            Assert.Equal(666, results.First());

        }

        [Fact]
        public void DeserializeShouldRespectPropertyRenamingAttribute()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new DriverDeserializer<UserWithJsonPropertyAttribute>(client, CypherResultMode.Set);
            var content = new TestStatementResult(
                new[] {"Foo"},
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {
                        "Foo", GenerateMockNode(new Dictionary<string, object>()
                        {
                            {"givenName", "Bob"}
                        }, 740)
                    }
                }));

            // Act
            var results = deserializer.Deserialize(content).ToArray();

            // Assert
            Assert.Equal("Bob", results.Single().Name);
        }

        [Fact]
        //[Description("https://bitbucket.org/Readify/neo4jclient/issue/63")]
        public void DeserializeShouldMapNullCollectResultsWithOtherProperties()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new DriverDeserializer<ModelWithCollect>(client, CypherResultMode.Projection);

            var content = new TestStatementResult(
                new[] {"Fans", "Poster"},
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {"Fans", null},
                    {
                        "Poster", GenerateMockNode(new Dictionary<string, object>()
                        {
                            {"GivenName", "Bob"},
                        }, 740)
                    }
                })
            );

            // Act
            var results = deserializer.Deserialize(content).ToArray();

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
            var deserializer = new DriverDeserializer<AppAlternatives>(client, CypherResultMode.Projection);
            var content = new TestStatementResult(
                new[] {"Application", "Alternatives"},
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {"Application", GenerateMockNode(new Dictionary<string, object>(), 123)},
                    {"Alternatives", null}
                }));

            // Act
            var results = deserializer.Deserialize(content).ToArray();

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
            var deserializer = new DriverDeserializer<ModelWithCollect>(client, CypherResultMode.Projection);

            var content = new TestStatementResult(
                new[] {"Fans"},
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {"Fans", new List<string>(){null, null}}
                }));

            // Act
            var results = deserializer.Deserialize(content).ToArray();

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
            var deserializer = new DriverDeserializer<object>(client, CypherResultMode.Set);

            var content = new TestStatementResult(
                new[] { "db" },
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {"db", null}
                }));

            // Act
            var results = deserializer.Deserialize(content).ToArray();

            // Assert
            Assert.Single(results);
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
            var deserializer = new DriverDeserializer<TAnon>(client, CypherResultMode.Projection);
            var content = new TestStatementResult(
                new[] {"Name", "Population"},
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {"Name", "Tokyo"},
                    {"Population", 13000000}
                }),
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {"Name", "London"},
                    {"Population", 8000000}
                }),
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {"Name", "Sydney"},
                    {"Population", 4000000}
                }));

            // Act
            var results = deserializer.Deserialize(content).ToArray();

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
            var deserializer = new DriverDeserializer<TAnon>(client, CypherResultMode.Projection);
            var content = new TestStatementResult(
                new[] { "Name", "Friends" },
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {"Name", "Jim"},
                    {"Friends", new List<object>(){null}}
                }));

            // Act
            var results = deserializer.Deserialize(content).ToArray();

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

        [Fact]
        public void DeserializeNestedObjectsInTransactionReturningNode()
        {
            var client = Substitute.For<IGraphClient>();
            var deserializer = new DriverDeserializer<StateCityAndLabelWithNode>(client, CypherResultMode.Projection);
            var content = new TestStatementResult(
                new[] {"State", "Labels", "Cities"},
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {
                        "State", new Dictionary<string, object>()
                        {
                            {"Name", "Baja California"}
                        }
                    },
                    {
                        "Labels", new List<string>() {"State"}
                    },
                    {
                        "Cities", new List<INode>()
                        {
                            GenerateMockNode(new Dictionary<string, object>()
                            {
                                {"Name", "Tijuana"},
                                {"Population", 1300000}
                            }, 5),
                            GenerateMockNode(new Dictionary<string, object>()
                            {
                                {"Name", "Mexicali"},
                                {"Population", 500000}
                            }, 4)
                        }
                    }
                })
            );

            var results = deserializer.Deserialize(content).ToArray();
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
            var deserializer = new DriverDeserializer<StateCityAndLabel>(client, CypherResultMode.Projection);
            var content = new TestStatementResult(
                new[] {"State", "Labels", "Cities"},
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {
                        "State", GenerateMockNode(new Dictionary<string, object>()
                        {
                            {"Name", "Baja California"}
                        })
                    },
                    {"Labels", new List<string>() {"State"}},
                    {
                        "Cities", new List<INode>()
                        {
                            GenerateMockNode(new Dictionary<string, object>()
                            {
                                {"Name", "Tijuana"},
                                {"Population", 1300000}
                            }),
                            GenerateMockNode(new Dictionary<string, object>()
                            {
                                {"Name", "Mexicali"},
                                {"Population", 500000}
                            })
                        }
                    }
                }));

            var results = deserializer.Deserialize(content).ToArray();
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
            var deserializer = new DriverDeserializer<CityAndLabel>(client, CypherResultMode.Projection);
            var content = new TestStatementResult(
                new[] {"City", "Labels"},
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {
                        "City", new Dictionary<string, object>()
                        {
                            {"Name", "Sydney"},
                            {"Population", 4000000}
                        }
                    },
                    {
                        "Labels", new List<string>() {"City1"}
                    }
                }),
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {
                        "City", new Dictionary<string, object>()
                        {
                            {"Name", "Tijuana"},
                            {"Population", 1300000}
                        }
                    },
                    {
                        "Labels", new List<string>() {"City2"}
                    }
                }));

            var results = deserializer.Deserialize(content).ToArray();
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
            var deserializer = new DriverDeserializer<City>(client, CypherResultMode.Projection);
            var content = new TestStatementResult(
                new[] {"Name", "Population"},
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {"Name", "Sydney"},
                    {"Population", 4000000}
                }),
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {"Name", "Tijuana"},
                    {"Population", 1300000}
                }));
            var results = deserializer.Deserialize(content).ToArray();
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
            var deserializer = new DriverDeserializer<int>(client, CypherResultMode.Set);
            var content = new TestStatementResult(
                new[] {"count(n)"},
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {"count(n)", 3}
                }));
            var results = deserializer.Deserialize(content).ToArray();
            Assert.Equal(1, results.Length);
            Assert.Equal(3, results[0]);
        }

        [Fact]
        public void DeserializeResultsSetInTransaction()
        {
            var client = Substitute.For<IGraphClient>();
            var deserializer = new DriverDeserializer<City>(client, CypherResultMode.Set);
            var content = new TestStatementResult(
                new[] {"c"},
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {
                        "c", new Dictionary<string, object>()
                        {
                            {"Name", "Sydney"},
                            {"Population", 4000000}
                        }
                    }
                }));
            var results = deserializer.Deserialize(content).ToArray();
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
            var deserializer = new DriverDeserializer<City>(client, CypherResultMode.Set);
            var content = new TestStatementResult(
                new[] {"Cities"},
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {
                        "Cities", new Dictionary<string, object>()
                        {
                            {"Name", "東京"},
                            {"Population", 13000000}
                        }
                    }
                }),
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {
                        "Cities", new Dictionary<string, object>()
                        {
                            {"Name", "London"},
                            {"Population", 8000000}
                        }
                    }
                }),
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {
                        "Cities", new Dictionary<string, object>()
                        {
                            {"Name", "Sydney"},
                            {"Population", 4000000}
                        }
                    }
                }));

            // Act
            var results = deserializer.Deserialize(content).ToArray();

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
            var deserializer = new DriverDeserializer<Asset>(client, CypherResultMode.Set);
            var content = new TestStatementResult(
                new[] {"asset"},
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {
                        "asset", GenerateMockNode(new Dictionary<string, object>()
                        {
                            {"Name", "67"},
                            {"UniqueId", 2}
                        }, 879)
                    }
                }),
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {
                        "asset", GenerateMockNode(new Dictionary<string, object>()
                        {
                            {"Name", "15 Mile"},
                            {"UniqueId", 1}
                        }, 878)
                    }
                }));

            // Act
            var results = deserializer.Deserialize(content).ToArray();

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
        public void BadJsonShouldThrowExceptionThatIncludesFullNameOfTargetType()
        {
            var client = Substitute.For<IGraphClient>();
            var deserializer = new DriverDeserializer<Asset>(client, CypherResultMode.Set);

            var ex = Assert.Throws<ArgumentException>(() =>
                deserializer.Deserialize(new TestStatementResult(new string[0], GenerateMockRecord(new Dictionary<string, object>()
                {
                    {"N", 1 }
                })))
            );
            Assert.Contains(typeof(Asset).FullName, ex.Message);
        }

        [Fact]
        public void ClassWithoutDefaultPublicConstructorShouldThrowExceptionThatExplainsThis()
        {
            var client = Substitute.For<IGraphClient>();
            var deserializer = new DriverDeserializer<ClassWithoutDefaultPublicConstructor>(client, CypherResultMode.Set);

            var content = new TestStatementResult(
                new[] { "Cities" },
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {
                        "Cities", new Dictionary<string, object>()
                        {
                            {"Name", "Tokyo" },
                            {"Population", 13000000 }
                        }
                    }
                }),
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {
                        "Cities", new Dictionary<string, object>()
                        {
                            {"Name", "London" },
                            {"Population", 8000000 }
                        }
                    }
                }),
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {
                        "Cities", new Dictionary<string, object>()
                        {
                            {"Name", "Sydney" },
                            {"Population", 4000000 }
                        }
                    }
                }));

            var ex = Assert.Throws<DeserializationException>(() => deserializer.Deserialize(content).ToArray());
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
            var deserializer = new DriverDeserializer<long?>(client, CypherResultMode.Set);
            var content = new TestStatementResult(
                new []{ "count(distinct registration)" },
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {"count(distinct registration)", 123 }
                }));

            // Act
            var result = deserializer.Deserialize(content).First();

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
            var deserializer = new DriverDeserializer<ModelWithByteArray>(client, CypherResultMode.Projection);
            var content = new TestStatementResult(
                new[] { "Array" },
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {"Array", "AQIDBA==" }
                }));

            // Act
            var result = deserializer.Deserialize(content).First();

            // Assert
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, result.Array);
        }

        [Fact]
        //[Description("https://bitbucket.org/Readify/neo4jclient/issue/114/byte-support")]
        public void DeserializeBase64StringIntoByteArrayInSetResultMode()
        {
            // Arrange
            var client = Substitute.For<IGraphClient>();
            var deserializer = new DriverDeserializer<byte[]>(client, CypherResultMode.Set);
            var content = new TestStatementResult(
                new[]{"column1"},
                GenerateMockRecord(new Dictionary<string, object>()
                {
                    {"column1", "AQIDBA==" }
                }));

            // Act
            var result = deserializer.Deserialize(content).First();

            // Assert
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, result);
        }
    }
}
