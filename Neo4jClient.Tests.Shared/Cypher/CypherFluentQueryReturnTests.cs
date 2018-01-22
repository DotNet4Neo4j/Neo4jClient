using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using Newtonsoft.Json.Serialization;
using NSubstitute;
using Xunit;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;
using Neo4jClient.Test.Fixtures;
using Newtonsoft.Json;

namespace Neo4jClient.Test.Cypher
{
    
    class FooWithJsonProperties
    {
        [JsonProperty("bar")]
        public string Bar { get; set; }
    }

    
    public class CypherFluentQueryReturnTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void ReturnDistinct()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .ReturnDistinct<object>("n")
                .Query;

            Assert.Equal("START n=node({p0})\r\nRETURN distinct n", query.QueryText);
            Assert.Equal(3L, query.QueryParameters["p0"]);
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        public void ReturnDistinctWithLimit()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .ReturnDistinct<object>("n")
                .Limit(5)
                .Query;

            Assert.Equal("START n=node({p0})\r\nRETURN distinct n\r\nLIMIT {p1}", query.QueryText);
            Assert.Equal(3L, query.QueryParameters["p0"]);
            Assert.Equal(5, query.QueryParameters["p1"]);
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        public void ReturnDistinctWithLimitAndOrderBy()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .ReturnDistinct<object>("n")
                .OrderBy("n.Foo")
                .Limit(5)
                .Query;

            Assert.Equal("START n=node({p0})\r\nRETURN distinct n\r\nORDER BY n.Foo\r\nLIMIT {p1}", query.QueryText);
            Assert.Equal(3L, query.QueryParameters["p0"]);
            Assert.Equal(5, query.QueryParameters["p1"]);
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        public void ReturnIdentity()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .Return<object>("n")
                .Query;

            Assert.Equal("START n=node({p0})\r\nRETURN n", query.QueryText);
            Assert.Equal(3L, query.QueryParameters["p0"]);
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        public void ReturnsUsingJsonPropertyValueForNameOfProperty()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(c => c.As<FooWithJsonProperties>().Bar)
                .Query;

            Assert.Equal("RETURN c.bar", query.QueryText);
        }

        [Fact]
        public void ReturnWithLimit()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .Return<object>("n")
                .Limit(5)
                .Query;

            Assert.Equal("START n=node({p0})\r\nRETURN n\r\nLIMIT {p1}", query.QueryText);
            Assert.Equal(3L, query.QueryParameters["p0"]);
            Assert.Equal(5, query.QueryParameters["p1"]);
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        public void ReturnWithLimitAndOrderBy()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .Return<object>("n")
                .OrderBy("n.Foo")
                .Limit(5)
                .Query;

            Assert.Equal("START n=node({p0})\r\nRETURN n\r\nORDER BY n.Foo\r\nLIMIT {p1}", query.QueryText);
            Assert.Equal(3L, query.QueryParameters["p0"]);
            Assert.Equal(5, query.QueryParameters["p1"]);
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        //[Description("https://bitbucket.org/Readify/neo4jclient/issue/42")]
        public void ShouldCombineWithLimitAndOrderBy()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start(new
                {
                    me = (NodeReference)123,
                    viewer = (NodeReference)456
                })
                .Match("me-[:FRIEND]-common-[:FRIEND]-viewer")
                .Return<Node<object>>("common")
                .Limit(5)
                .OrderBy("common.FirstName")
                .Query;

            Assert.Equal(string.Format("START me=node({{p0}}), viewer=node({{p1}}){0}MATCH me-[:FRIEND]-common-[:FRIEND]-viewer{0}RETURN common{0}LIMIT {{p2}}{0}ORDER BY common.FirstName", Environment.NewLine), query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal(456L, query.QueryParameters["p1"]);
            Assert.Equal(5, query.QueryParameters["p2"]);
            Assert.Equal(CypherResultFormat.Rest, query.ResultFormat);
        }

        [Fact]
        public void ShouldReturnRawFunctionCall()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return<long>("count(item)")
                .Query;

            Assert.Equal("RETURN count(item)", query.QueryText);
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        public void ReturnsWhenIdentityIsACollection()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("(n0),(n1)")
                .Return<IEnumerable<object>>("[n0,n1]")
                .Query;

            Assert.Equal("MATCH (n0),(n1)\r\nRETURN [n0,n1]", query.QueryText);
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        public void ReturnsWhenIdentityIsACollectionRegardlessOfPadding()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("(n0),(n1)")
                .Return<IEnumerable<object>>("  [n0,n1]  ")
                .Query;

            Assert.Equal("MATCH (n0),(n1)\r\nRETURN [n0,n1]", query.QueryText);
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }
        
        [Fact]
        public void ShouldThrowWhenIdentityIsCollectionButResultIsNot()
        {
            var client = Substitute.For<IRawGraphClient>();
            var ex = Assert.Throws<ArgumentException>(
                () => new CypherFluentQuery(client).Return<object>("[foo,bar]")
            );
            Assert.StartsWith(CypherFluentQuery.IdentityLooksLikeACollectionButTheResultIsNotEnumerableMessage, ex.Message);
        }

        [Fact]
        public void ShouldThrowWhenIdentityLooksLikeAMultiColumnStatement()
        {
            var client = Substitute.For<IRawGraphClient>();
            var ex = Assert.Throws<ArgumentException>(
                () => new CypherFluentQuery(client).Return<long>("foo,bar")
            );
            Assert.StartsWith(CypherFluentQuery.IdentityLooksLikeAMultiColumnStatementExceptionMessage, ex.Message);
        }

        [Fact]
        public void ShouldReturnCountOnItsOwn()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(item => item.Count())
                .Query;

            Assert.Equal("RETURN count(item)", query.QueryText);
        }

        [Fact]
        public void ShouldReturnCountAllOnItsOwn()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(() => All.Count())
                .Query;

            Assert.Equal("RETURN count(*)", query.QueryText);
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        public void ShouldReturnCustomFunctionCall()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(() => Return.As<long>("sum(foo.bar)"))
                .Query;

            Assert.Equal("RETURN sum(foo.bar)", query.QueryText);
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        public void ShouldReturnSpecificPropertyOnItsOwn()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(a => a.As<Commodity>().Name)
                .Query;

            Assert.Equal("RETURN a.Name", query.QueryText);
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        public void ShouldReturnSpecificPropertyOnItsOwnCamel()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.JsonContractResolver = new CamelCasePropertyNamesContractResolver();
            var query = new CypherFluentQuery(client)
                .Return(a => a.As<Commodity>().Name)
                .Query;

            Assert.Equal("RETURN a.name", query.QueryText);
        }

        [Fact]
        public void ShouldThrowForMemberExpressionOffMethodOtherThanAs()
        {
            var client = Substitute.For<IRawGraphClient>();
            Assert.Throws<ArgumentException>(
                () => new CypherFluentQuery(client).Return(a => a.Type().Length));
        }

        [Fact]
        public void ShouldUseSetResultModeForIdentityBasedReturn()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return<object>("foo")
                .Query;

            Assert.Equal(CypherResultMode.Set, query.ResultMode);
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        public void ShouldUseSetResultModeForRawFunctionCallReturn()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return<long>("count(foo)")
                .Query;

            Assert.Equal(CypherResultMode.Set, query.ResultMode);
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        public void ShouldUseSetResultModeForSimpleLambdaReturn()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(item => item.As<object>())
                .Query;

            Assert.Equal(CypherResultMode.Set, query.ResultMode);
        }

        [Fact]
        public void ShouldUseSetResultModeForSingleFunctionReturn()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(item => item.Count())
                .Query;

            Assert.Equal(CypherResultMode.Set, query.ResultMode);
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        public void ShouldUseSetResultModeForAllFunctionReturn()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(() => All.Count())
                .Query;

            Assert.Equal(CypherResultMode.Set, query.ResultMode);
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        public void ShouldUseSetResultModeForSpecificPropertyReturn()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(a => a.As<Commodity>().Name)
                .Query;

            Assert.Equal(CypherResultMode.Set, query.ResultMode);
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        public void ShouldUseSetResultModeForSpecificPropertyReturnCamel()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.JsonContractResolver = new CamelCasePropertyNamesContractResolver();
            var query = new CypherFluentQuery(client)
                .Return(a => a.As<Commodity>().Name)
                .Query;

            Assert.Equal(CypherResultMode.Set, query.ResultMode);
        }

        [Fact]
        public void ShouldUseProjectionResultModeForAnonymousObjectReturn()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(a => new { Foo = a.As<object>() })
                .Query;

            Assert.Equal(CypherResultMode.Projection, query.ResultMode);
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        public void ShouldUseProjectionResultModeForNamedObjectReturn()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(a => new ProjectionResult { Commodity = a.As<Commodity>() })
                .Query;

            Assert.Equal(CypherResultMode.Projection, query.ResultMode);
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        public void ShouldUseProjectionResultModeForNamedObjectReturnCamel()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.JsonContractResolver = new CamelCasePropertyNamesContractResolver();
            var query = new CypherFluentQuery(client)
                .Return(a => new ProjectionResult { Commodity = a.As<Commodity>() })
                .Query;

            Assert.Equal(CypherResultMode.Projection, query.ResultMode);
        }

        [Fact]
        public void ShouldSupportAnonymousReturnTypesEndToEnd()
        {
            const string queryText = "START root=node({p0})\r\nMATCH root-->other\r\nRETURN other AS Foo";
            var parameters = new Dictionary<string, object>
            {
                {"p0", 123}
            };

            var cypherQuery = new CypherQuery(queryText, parameters, CypherResultMode.Projection, CypherResultFormat.Rest);
            var cypherApiQuery = new CypherApiQuery(cypherQuery);

            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.PostObjectAsJson("/cypher", cypherApiQuery),
                    MockResponse.Json(HttpStatusCode.OK, @"{
  'columns' : [ 'Foo' ],
  'data' : [ [ {
    'outgoing_relationships' : 'http://localhost:8000/db/data/node/748/relationships/out',
    'data' : {
      'Name' : 'Antimony',
      'UniqueId' : 38
    },
    'all_typed_relationships' : 'http://localhost:8000/db/data/node/748/relationships/all/{-list|&|types}',
    'traverse' : 'http://localhost:8000/db/data/node/748/traverse/{returnType}',
    'self' : 'http://localhost:8000/db/data/node/748',
    'property' : 'http://localhost:8000/db/data/node/748/properties/{key}',
    'outgoing_typed_relationships' : 'http://localhost:8000/db/data/node/748/relationships/out/{-list|&|types}',
    'properties' : 'http://localhost:8000/db/data/node/748/properties',
    'incoming_relationships' : 'http://localhost:8000/db/data/node/748/relationships/in',
    'extensions' : {
    },
    'create_relationship' : 'http://localhost:8000/db/data/node/748/relationships',
    'paged_traverse' : 'http://localhost:8000/db/data/node/748/paged/traverse/{returnType}{?pageSize,leaseTime}',
    'all_relationships' : 'http://localhost:8000/db/data/node/748/relationships/all',
    'incoming_typed_relationships' : 'http://localhost:8000/db/data/node/748/relationships/in/{-list|&|types}'
  } ], [ {
    'outgoing_relationships' : 'http://localhost:8000/db/data/node/610/relationships/out',
    'data' : {
      'Name' : 'Bauxite',
      'UniqueId' : 24
    },
    'all_typed_relationships' : 'http://localhost:8000/db/data/node/610/relationships/all/{-list|&|types}',
    'traverse' : 'http://localhost:8000/db/data/node/610/traverse/{returnType}',
    'self' : 'http://localhost:8000/db/data/node/610',
    'property' : 'http://localhost:8000/db/data/node/610/properties/{key}',
    'outgoing_typed_relationships' : 'http://localhost:8000/db/data/node/610/relationships/out/{-list|&|types}',
    'properties' : 'http://localhost:8000/db/data/node/610/properties',
    'incoming_relationships' : 'http://localhost:8000/db/data/node/610/relationships/in',
    'extensions' : {
    },
    'create_relationship' : 'http://localhost:8000/db/data/node/610/relationships',
    'paged_traverse' : 'http://localhost:8000/db/data/node/610/paged/traverse/{returnType}{?pageSize,leaseTime}',
    'all_relationships' : 'http://localhost:8000/db/data/node/610/relationships/all',
    'incoming_typed_relationships' : 'http://localhost:8000/db/data/node/610/relationships/in/{-list|&|types}'
  } ], [ {
    'outgoing_relationships' : 'http://localhost:8000/db/data/node/749/relationships/out',
    'data' : {
      'Name' : 'Bismuth',
      'UniqueId' : 37
    },
    'all_typed_relationships' : 'http://localhost:8000/db/data/node/749/relationships/all/{-list|&|types}',
    'traverse' : 'http://localhost:8000/db/data/node/749/traverse/{returnType}',
    'self' : 'http://localhost:8000/db/data/node/749',
    'property' : 'http://localhost:8000/db/data/node/749/properties/{key}',
    'outgoing_typed_relationships' : 'http://localhost:8000/db/data/node/749/relationships/out/{-list|&|types}',
    'properties' : 'http://localhost:8000/db/data/node/749/properties',
    'incoming_relationships' : 'http://localhost:8000/db/data/node/749/relationships/in',
    'extensions' : {
    },
    'create_relationship' : 'http://localhost:8000/db/data/node/749/relationships',
    'paged_traverse' : 'http://localhost:8000/db/data/node/749/paged/traverse/{returnType}{?pageSize,leaseTime}',
    'all_relationships' : 'http://localhost:8000/db/data/node/749/relationships/all',
    'incoming_typed_relationships' : 'http://localhost:8000/db/data/node/749/relationships/in/{-list|&|types}'
  } ] ]
}")
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();

                var results = graphClient
                    .Cypher
                    .Start("root", graphClient.RootNode)
                    .Match("root-->other")
                    .Return(other => new
                    {
                        Foo = other.As<Commodity>()
                    })
                    .Results
                    .ToList();

                Assert.Equal(3L, results.Count());

                var result = results[0];
                Assert.Equal("Antimony", result.Foo.Name);
                Assert.Equal(38, result.Foo.UniqueId);

                result = results[1];
                Assert.Equal("Bauxite", result.Foo.Name);
                Assert.Equal(24, result.Foo.UniqueId);

                result = results[2];
                Assert.Equal("Bismuth", result.Foo.Name);
                Assert.Equal(37, result.Foo.UniqueId);
            }
        }

        [Fact]
        public void ShouldSupportAnonymousReturnTypesEndToEndCamel()
        {
            const string queryText = "START root=node({p0})\r\nMATCH root-->other\r\nRETURN other AS Foo";
            var parameters = new Dictionary<string, object>
            {
                {"p0", 123}
            };

            var cypherQuery = new CypherQuery(queryText, parameters, CypherResultMode.Projection);
            var cypherApiQuery = new CypherApiQuery(cypherQuery);

            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.PostObjectAsJson("/cypher", cypherApiQuery),
                    MockResponse.Json(HttpStatusCode.OK, @"{
  'columns' : [ 'Foo' ],
  'data' : [ [ {
    'outgoing_relationships' : 'http://localhost:8000/db/data/node/748/relationships/out',
    'data' : {
      'name' : 'Antimony',
      'uniqueId' : 38
    },
    'all_typed_relationships' : 'http://localhost:8000/db/data/node/748/relationships/all/{-list|&|types}',
    'traverse' : 'http://localhost:8000/db/data/node/748/traverse/{returnType}',
    'self' : 'http://localhost:8000/db/data/node/748',
    'property' : 'http://localhost:8000/db/data/node/748/properties/{key}',
    'outgoing_typed_relationships' : 'http://localhost:8000/db/data/node/748/relationships/out/{-list|&|types}',
    'properties' : 'http://localhost:8000/db/data/node/748/properties',
    'incoming_relationships' : 'http://localhost:8000/db/data/node/748/relationships/in',
    'extensions' : {
    },
    'create_relationship' : 'http://localhost:8000/db/data/node/748/relationships',
    'paged_traverse' : 'http://localhost:8000/db/data/node/748/paged/traverse/{returnType}{?pageSize,leaseTime}',
    'all_relationships' : 'http://localhost:8000/db/data/node/748/relationships/all',
    'incoming_typed_relationships' : 'http://localhost:8000/db/data/node/748/relationships/in/{-list|&|types}'
  } ], [ {
    'outgoing_relationships' : 'http://localhost:8000/db/data/node/610/relationships/out',
    'data' : {
      'name' : 'Bauxite',
      'uniqueId' : 24
    },
    'all_typed_relationships' : 'http://localhost:8000/db/data/node/610/relationships/all/{-list|&|types}',
    'traverse' : 'http://localhost:8000/db/data/node/610/traverse/{returnType}',
    'self' : 'http://localhost:8000/db/data/node/610',
    'property' : 'http://localhost:8000/db/data/node/610/properties/{key}',
    'outgoing_typed_relationships' : 'http://localhost:8000/db/data/node/610/relationships/out/{-list|&|types}',
    'properties' : 'http://localhost:8000/db/data/node/610/properties',
    'incoming_relationships' : 'http://localhost:8000/db/data/node/610/relationships/in',
    'extensions' : {
    },
    'create_relationship' : 'http://localhost:8000/db/data/node/610/relationships',
    'paged_traverse' : 'http://localhost:8000/db/data/node/610/paged/traverse/{returnType}{?pageSize,leaseTime}',
    'all_relationships' : 'http://localhost:8000/db/data/node/610/relationships/all',
    'incoming_typed_relationships' : 'http://localhost:8000/db/data/node/610/relationships/in/{-list|&|types}'
  } ], [ {
    'outgoing_relationships' : 'http://localhost:8000/db/data/node/749/relationships/out',
    'data' : {
      'name' : 'Bismuth',
      'uniqueId' : 37
    },
    'all_typed_relationships' : 'http://localhost:8000/db/data/node/749/relationships/all/{-list|&|types}',
    'traverse' : 'http://localhost:8000/db/data/node/749/traverse/{returnType}',
    'self' : 'http://localhost:8000/db/data/node/749',
    'property' : 'http://localhost:8000/db/data/node/749/properties/{key}',
    'outgoing_typed_relationships' : 'http://localhost:8000/db/data/node/749/relationships/out/{-list|&|types}',
    'properties' : 'http://localhost:8000/db/data/node/749/properties',
    'incoming_relationships' : 'http://localhost:8000/db/data/node/749/relationships/in',
    'extensions' : {
    },
    'create_relationship' : 'http://localhost:8000/db/data/node/749/relationships',
    'paged_traverse' : 'http://localhost:8000/db/data/node/749/paged/traverse/{returnType}{?pageSize,leaseTime}',
    'all_relationships' : 'http://localhost:8000/db/data/node/749/relationships/all',
    'incoming_typed_relationships' : 'http://localhost:8000/db/data/node/749/relationships/in/{-list|&|types}'
  } ] ]
}")
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();
                graphClient.JsonContractResolver = new CamelCasePropertyNamesContractResolver();
                var results = graphClient
                    .Cypher
                    .Start("root", graphClient.RootNode)
                    .Match("root-->other")
                    .Return(other => new
                    {
                        Foo = other.As<Commodity>()
                    })
                    .Results
                    .ToList();

                Assert.Equal(3L, results.Count());

                var result = results[0];
                Assert.Equal("Antimony", result.Foo.Name);
                Assert.Equal(38, result.Foo.UniqueId);

                result = results[1];
                Assert.Equal("Bauxite", result.Foo.Name);
                Assert.Equal(24, result.Foo.UniqueId);

                result = results[2];
                Assert.Equal("Bismuth", result.Foo.Name);
                Assert.Equal(37, result.Foo.UniqueId);
            }
        }

        [Fact]
        public void BinaryExpressionIsNotNull()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("(a)")
                .Return(a => new {NotNull = a != null})
                .Query;

            Assert.Equal("MATCH (a)\r\nRETURN a IS NOT NULL AS NotNull", query.QueryText);
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        public void BinaryExpressionIsNull()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("(a)")
                .Return(a => new { IsNull = a == null })
                .Query;

            Assert.Equal("MATCH (a)\r\nRETURN a IS NULL AS IsNull", query.QueryText);
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        public void BinaryExpressionThrowNotSupportedExceptionForUnsupportedExpressionComparison()
        {
            var client = Substitute.For<IRawGraphClient>();
            var ex = Assert.Throws<NotSupportedException>(() => new CypherFluentQuery(client).Return(a => new { IsNull = a.As<int>() == 10 }));

            Assert.StartsWith(CypherReturnExpressionBuilder.UnsupportedBinaryExpressionComparisonExceptionMessage, ex.Message);
        }

        [Fact]
        public void BinaryExpressionThrowNotSupportedExceptionForUnsupportedExpressionTypes()
        {
            var client = Substitute.For<IRawGraphClient>();
            var ex = Assert.Throws<NotSupportedException>(() => new CypherFluentQuery(client).Return(a => new {IsNull = a.As<int>() > 10}));

            var message = string.Format(CypherReturnExpressionBuilder.UnsupportedBinaryExpressionExceptionMessageFormat, ExpressionType.GreaterThan);
            Assert.StartsWith(message, ex.Message);
        }

        public class Commodity
        {
            public string Name { get; set; }
            public long UniqueId { get; set; }
        }

        public class ProjectionResult
        {
            public Commodity Commodity { get; set; }
        }
    }
}
