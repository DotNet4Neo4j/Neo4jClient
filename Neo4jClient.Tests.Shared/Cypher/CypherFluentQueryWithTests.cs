using System;
using Newtonsoft.Json.Serialization;
using NSubstitute;
using Xunit;
using Neo4jClient.Cypher;
using Neo4jClient.Test.Fixtures;
using Newtonsoft.Json;

namespace Neo4jClient.Test.Cypher
{
    
    public class CypherFluentQueryWithTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void With()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .With("foo")
                .Query;

            Assert.Equal("START n=node({p0})\r\nWITH foo", query.QueryText);
            Assert.Equal(3L, query.QueryParameters["p0"]);
        }

        [Fact]
        public void ShouldReturnCountOnItsOwn()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .With(item => item.Count())
                .Query;

            Assert.Equal("WITH count(item)", query.QueryText);
        }

        [Fact]
        public void ShouldReturnCountAllOnItsOwn()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .With(() => All.Count())
                .Query;

            Assert.Equal("WITH count(*)", query.QueryText);
        }

        [Fact]
        public void ShouldReturnCustomFunctionCall()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .With(() => new { baz = "sum(foo.bar)" })
                .Query;

            Assert.Equal("WITH sum(foo.bar) AS baz", query.QueryText);
        }

        [Fact]
        public void ShouldReturnSpecificPropertyTakingIntoAccountJsonProperty()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .With(a => a.As<Cypher.FooWithJsonProperties>().Bar)
                .Query;

            Assert.Equal("WITH a.bar", query.QueryText);
        }

        [Fact]
        public void ShouldReturnSpecificPropertyOnItsOwn()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .With(a => a.As<Commodity>().Name)
                .Query;

            Assert.Equal("WITH a.Name", query.QueryText);
        }

        [Fact]
        public void ShouldReturnSpecificPropertyWithAliasWithNullableSuffixInCypher19()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.CypherCapabilities.Returns(CypherCapabilities.Cypher19);
            var query = new CypherFluentQuery(client)
                .With(a => new
                {
                    SomeAlias = a.As<Commodity>().Name
                })
                .Query;

            Assert.Equal("WITH a.Name? AS SomeAlias", query.QueryText);
        }

        [Fact]
        public void ShouldReturnSpecificPropertyWithAliasWithoutNullableSuffixInCypher20()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.CypherCapabilities.Returns(CypherCapabilities.Cypher20);
            var query = new CypherFluentQuery(client)
                .With(a => new
                {
                    SomeAlias = a.As<Commodity>().Name
                })
                .Query;

            Assert.Equal("WITH a.Name AS SomeAlias", query.QueryText);
        }

        [Fact]
        public void ShouldReturnSpecificPropertyOnItsOwnCamelAs()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.JsonContractResolver = new CamelCasePropertyNamesContractResolver();
            var query = new CypherFluentQuery(client)
                .With(a => new Commodity(){ Name = a.As<Commodity>().Name})
                .Query;

            Assert.Equal("WITH a.name AS Name", query.QueryText);
        }

        [Fact]
        public void ShouldReturnSpecificPropertyOnItsOwnCamel()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.JsonContractResolver = new CamelCasePropertyNamesContractResolver();
            var query = new CypherFluentQuery(client)
                .With(a => a.As<Commodity>().Name)
                .Query;

            Assert.Equal("WITH a.name", query.QueryText);
        }

        [Fact]
        public void ShouldThrowForMemberExpressionOffMethodOtherThanAs()
        {
            var client = Substitute.For<IRawGraphClient>();
            Assert.Throws<ArgumentException>(
                () => new CypherFluentQuery(client).With(a => a.Type().Length));
        }

        [Fact]
        public void ShouldTranslateAnonymousObjectWithExplicitPropertyNames()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .With(a => new { Foo = a })
                .Query;

            Assert.Equal("WITH a AS Foo", query.QueryText);
        }

        [Fact]
        public void ShouldTranslateAnonymousObjectWithImplicitPropertyNames()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .With(a => new { a })
                .Query;

            Assert.Equal("WITH a", query.QueryText);
        }

        [Fact]
        public void ShouldTranslateAnonymousObjectWithMultipleProperties()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .With((a, b) => new { a, b })
                .Query;

            Assert.Equal("WITH a, b", query.QueryText);
        }

        [Fact]
        public void ShouldTranslateAnonymousObjectWithMixedProperties()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .With((a, b) => new
                {
                    a,
                    foo = b.Count(),
                    bar = b.CollectAs<object>()
                })
                .Query;

            Assert.Equal("WITH a, count(b) AS foo, collect(b) AS bar", query.QueryText);
        }

        [Fact]
        public void ShouldUseProjectionResultModeForNamedObjectReturnWithConcreteProperties()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .With(a => new ProjectionResult
                {
                    Commodity = a.As<Commodity>()
                })
                .Query;
            
            Assert.Equal("WITH a AS Commodity", query.QueryText);
        }

        [Fact]
        public void ShouldUseProjectionResultModeForNamedObjectReturnWithICypherResultItems()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .With(a => new CypherProjectionResult
                {
                    Foo = a
                })
                .Query;

            Assert.Equal("WITH a AS Foo", query.QueryText);
        }

        class FooWithJsonProperties
        {
            [JsonProperty("bar")]
            public string Bar { get; set; }
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

        public class CypherProjectionResult
        {
            public ICypherResultItem Foo { get; set; }
        }
    }
}
