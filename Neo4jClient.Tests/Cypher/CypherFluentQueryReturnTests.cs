using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.Cypher
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
                .Match("n")
                .ReturnDistinct<object>("n")
                .Query;

            Assert.Equal("MATCH n" + Environment.NewLine + "RETURN distinct n", query.QueryText);
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        public void ReturnDistinctWithLimit()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("n")
                .ReturnDistinct<object>("n")
                .Limit(5)
                .Query;

            Assert.Equal("MATCH n" + Environment.NewLine + "RETURN distinct n" + Environment.NewLine + "LIMIT $p0", query.QueryText);
            Assert.Equal(5, query.QueryParameters["p0"]);
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        public void ReturnDistinctWithLimitAndOrderBy()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("n")
                .ReturnDistinct<object>("n")
                .OrderBy("n.Foo")
                .Limit(5)
                .Query;

            Assert.Equal("MATCH n" + Environment.NewLine + "RETURN distinct n" + Environment.NewLine + "ORDER BY n.Foo" + Environment.NewLine + "LIMIT $p0", query.QueryText);
            Assert.Equal(5, query.QueryParameters["p0"]);
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        public void ReturnIdentity()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("n")
                .Return<object>("n")
                .Query;

            Assert.Equal("MATCH n" + Environment.NewLine + "RETURN n", query.QueryText);
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
                .Match("n")
                .Return<object>("n")
                .Limit(5)
                .Query;

            Assert.Equal("MATCH n" + Environment.NewLine + "RETURN n" + Environment.NewLine + "LIMIT $p0", query.QueryText);
            Assert.Equal(5, query.QueryParameters["p0"]);
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        public void ReturnWithLimitAndOrderBy()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("n")
                .Return<object>("n")
                .OrderBy("n.Foo")
                .Limit(5)
                .Query;

            Assert.Equal("MATCH n" + Environment.NewLine + "RETURN n" + Environment.NewLine + "ORDER BY n.Foo" + Environment.NewLine + "LIMIT $p0", query.QueryText);
            Assert.Equal(5, query.QueryParameters["p0"]);
            Assert.Equal(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Fact]
        //[Description("https://bitbucket.org/Readify/neo4jclient/issue/42")]
        public void ShouldCombineWithLimitAndOrderBy()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match( "me, viewer")
                .Match("me-[:FRIEND]-common-[:FRIEND]-viewer")
                .Return<Node<object>>("common")
                .Limit(5)
                .OrderBy("common.FirstName")
                .Query;

            Assert.Equal(string.Format("MATCH me, viewer{0}MATCH me-[:FRIEND]-common-[:FRIEND]-viewer{0}RETURN common{0}LIMIT $p0{0}ORDER BY common.FirstName", Environment.NewLine), query.QueryText);
            Assert.Equal(5, query.QueryParameters["p0"]);
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

            Assert.Equal("MATCH (n0),(n1)" + Environment.NewLine + "RETURN [n0,n1]", query.QueryText);
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

            Assert.Equal("MATCH (n0),(n1)" + Environment.NewLine + "RETURN [n0,n1]", query.QueryText);
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
        public void BinaryExpressionIsNotNull()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("(a)")
                .Return(a => new {NotNull = a != null})
                .Query;

            Assert.Equal("MATCH (a)" + Environment.NewLine + "RETURN a IS NOT NULL AS NotNull", query.QueryText);
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

            Assert.Equal("MATCH (a)" + Environment.NewLine + "RETURN a IS NULL AS IsNull", query.QueryText);
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
