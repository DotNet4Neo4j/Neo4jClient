using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NSubstitute;
using NUnit.Framework;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;

namespace Neo4jClient.Test.Cypher
{
    using System.Linq.Expressions;
    using System.Text.RegularExpressions;

    [TestFixture]
    public class CypherFluentQueryReturnTests
    {
        [Test]
        public void ReturnDistinct()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .ReturnDistinct<object>("n")
                .Query;

            Assert.AreEqual("START n=node({p0})\r\nRETURN distinct n", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
            Assert.AreEqual(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Test]
        public void ReturnDistinctWithLimit()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .ReturnDistinct<object>("n")
                .Limit(5)
                .Query;

            Assert.AreEqual("START n=node({p0})\r\nRETURN distinct n\r\nLIMIT {p1}", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
            Assert.AreEqual(5, query.QueryParameters["p1"]);
            Assert.AreEqual(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Test]
        public void ReturnDistinctWithLimitAndOrderBy()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .ReturnDistinct<object>("n")
                .OrderBy("n.Foo")
                .Limit(5)
                .Query;

            Assert.AreEqual("START n=node({p0})\r\nRETURN distinct n\r\nORDER BY n.Foo\r\nLIMIT {p1}", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
            Assert.AreEqual(5, query.QueryParameters["p1"]);
            Assert.AreEqual(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Test]
        public void ReturnIdentity()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .Return<object>("n")
                .Query;

            Assert.AreEqual("START n=node({p0})\r\nRETURN n", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
            Assert.AreEqual(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Test]
        public void ReturnWithLimit()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .Return<object>("n")
                .Limit(5)
                .Query;

            Assert.AreEqual("START n=node({p0})\r\nRETURN n\r\nLIMIT {p1}", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
            Assert.AreEqual(5, query.QueryParameters["p1"]);
            Assert.AreEqual(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Test]
        public void ReturnWithLimitAndOrderBy()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .Return<object>("n")
                .OrderBy("n.Foo")
                .Limit(5)
                .Query;

            Assert.AreEqual("START n=node({p0})\r\nRETURN n\r\nORDER BY n.Foo\r\nLIMIT {p1}", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
            Assert.AreEqual(5, query.QueryParameters["p1"]);
            Assert.AreEqual(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Test]
        [Description("https://bitbucket.org/Readify/neo4jclient/issue/42")]
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

            Assert.AreEqual(@"START me=node({p0}), viewer=node({p1})
MATCH me-[:FRIEND]-common-[:FRIEND]-viewer
RETURN common
LIMIT {p2}
ORDER BY common.FirstName", query.QueryText);
            Assert.AreEqual(123, query.QueryParameters["p0"]);
            Assert.AreEqual(456, query.QueryParameters["p1"]);
            Assert.AreEqual(5, query.QueryParameters["p2"]);
            Assert.AreEqual(CypherResultFormat.Rest, query.ResultFormat);
        }

        [Test]
        public void ShouldReturnRawFunctionCall()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return<long>("count(item)")
                .Query;

            Assert.AreEqual("RETURN count(item)", query.QueryText);
            Assert.AreEqual(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Test]
        public void ReturnsWhenIdentityIsACollection()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("(n0),(n1)")
                .Return<IEnumerable<object>>("[n0,n1]")
                .Query;

            Assert.AreEqual("MATCH (n0),(n1)\r\nRETURN [n0,n1]", query.QueryText);
            Assert.AreEqual(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Test]
        public void ReturnsWhenIdentityIsACollectionRegardlessOfPadding()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("(n0),(n1)")
                .Return<IEnumerable<object>>("  [n0,n1]  ")
                .Query;

            Assert.AreEqual("MATCH (n0),(n1)\r\nRETURN [n0,n1]", query.QueryText);
            Assert.AreEqual(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }
        
        [Test]
        public void ShouldThrowWhenIdentityIsCollectionButResultIsNot()
        {
            var client = Substitute.For<IRawGraphClient>();
            var ex = Assert.Throws<ArgumentException>(
                () => new CypherFluentQuery(client).Return<object>("[foo,bar]")
            );
            StringAssert.StartsWith(CypherFluentQuery.IdentityLooksLikeACollectionButTheResultIsNotEnumerableMessage, ex.Message);
        }

        [Test]
        public void ShouldThrowWhenIdentityLooksLikeAMultiColumnStatement()
        {
            var client = Substitute.For<IRawGraphClient>();
            var ex = Assert.Throws<ArgumentException>(
                () => new CypherFluentQuery(client).Return<long>("foo,bar")
            );
            StringAssert.StartsWith(CypherFluentQuery.IdentityLooksLikeAMultiColumnStatementExceptionMessage, ex.Message);
        }

        [Test]
        public void ShouldReturnCountOnItsOwn()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(item => item.Count())
                .Query;

            Assert.AreEqual("RETURN count(item)", query.QueryText);
        }

        [Test]
        public void ShouldReturnCountAllOnItsOwn()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(() => All.Count())
                .Query;

            Assert.AreEqual("RETURN count(*)", query.QueryText);
            Assert.AreEqual(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Test]
        public void ShouldReturnCustomFunctionCall()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(() => Return.As<long>("sum(foo.bar)"))
                .Query;

            Assert.AreEqual("RETURN sum(foo.bar)", query.QueryText);
            Assert.AreEqual(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Test]
        public void ShouldReturnSpecificPropertyOnItsOwn()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(a => a.As<Commodity>().Name)
                .Query;

            Assert.AreEqual("RETURN a.Name", query.QueryText);
            Assert.AreEqual(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Test]
        public void ShouldThrowForMemberExpressionOffMethodOtherThanAs()
        {
            var client = Substitute.For<IRawGraphClient>();
            Assert.Throws<ArgumentException>(
                () => new CypherFluentQuery(client).Return(a => a.Type().Length));
        }

        [Test]
        public void ShouldUseSetResultModeForIdentityBasedReturn()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return<object>("foo")
                .Query;

            Assert.AreEqual(CypherResultMode.Set, query.ResultMode);
            Assert.AreEqual(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Test]
        public void ShouldUseSetResultModeForRawFunctionCallReturn()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return<long>("count(foo)")
                .Query;

            Assert.AreEqual(CypherResultMode.Set, query.ResultMode);
            Assert.AreEqual(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Test]
        public void ShouldUseSetResultModeForSingleFunctionReturn()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(item => item.Count())
                .Query;

            Assert.AreEqual(CypherResultMode.Set, query.ResultMode);
            Assert.AreEqual(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Test]
        public void ShouldUseSetResultModeForAllFunctionReturn()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(() => All.Count())
                .Query;

            Assert.AreEqual(CypherResultMode.Set, query.ResultMode);
            Assert.AreEqual(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Test]
        public void ShouldUseSetResultModeForSpecificPropertyReturn()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(a => a.As<Commodity>().Name)
                .Query;

            Assert.AreEqual(CypherResultMode.Set, query.ResultMode);
            Assert.AreEqual(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Test]
        public void ShouldUseProjectionResultModeForAnonymousObjectReturn()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(a => new { Foo = a.As<object>() })
                .Query;

            Assert.AreEqual(CypherResultMode.Projection, query.ResultMode);
            Assert.AreEqual(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Test]
        public void ShouldUseProjectionResultModeForNamedObjectReturn()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(a => new ProjectionResult { Commodity = a.As<Commodity>() })
                .Query;

            Assert.AreEqual(CypherResultMode.Projection, query.ResultMode);
            Assert.AreEqual(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Test]
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

                Assert.AreEqual(3, results.Count());

                var result = results[0];
                Assert.AreEqual("Antimony", result.Foo.Name);
                Assert.AreEqual(38, result.Foo.UniqueId);

                result = results[1];
                Assert.AreEqual("Bauxite", result.Foo.Name);
                Assert.AreEqual(24, result.Foo.UniqueId);

                result = results[2];
                Assert.AreEqual("Bismuth", result.Foo.Name);
                Assert.AreEqual(37, result.Foo.UniqueId);
            }
        }

        [Test]
        public void BinaryExpressionIsNotNull()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("(a)")
                .Return(a => new {NotNull = a != null})
                .Query;

            Assert.AreEqual("MATCH (a)\r\nRETURN a IS NOT NULL AS NotNull", query.QueryText);
            Assert.AreEqual(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Test]
        public void BinaryExpressionIsNull()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("(a)")
                .Return(a => new { IsNull = a == null })
                .Query;

            Assert.AreEqual("MATCH (a)\r\nRETURN a IS NULL AS IsNull", query.QueryText);
            Assert.AreEqual(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Test]
        public void BinaryExpressionThrowNotSupportedExceptionForUnsupportedExpressionComparison()
        {
            var client = Substitute.For<IRawGraphClient>();
            var ex = Assert.Throws<NotSupportedException>(() => new CypherFluentQuery(client).Return(a => new { IsNull = a.As<int>() == 10 }));

            StringAssert.StartsWith(CypherReturnExpressionBuilder.UnsupportedBinaryExpressionComparisonExceptionMessage, ex.Message);
        }

        [Test]
        public void BinaryExpressionThrowNotSupportedExceptionForUnsupportedExpressionTypes()
        {
            var client = Substitute.For<IRawGraphClient>();
            var ex = Assert.Throws<NotSupportedException>(() => new CypherFluentQuery(client).Return(a => new {IsNull = a.As<int>() > 10}));

            var message = string.Format(CypherReturnExpressionBuilder.UnsupportedBinaryExpressionExceptionMessageFormat, ExpressionType.GreaterThan);
            StringAssert.StartsWith(message, ex.Message);
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
