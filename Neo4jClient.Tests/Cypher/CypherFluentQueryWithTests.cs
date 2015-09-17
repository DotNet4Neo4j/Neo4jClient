using System;
using Newtonsoft.Json.Serialization;
using NSubstitute;
using NUnit.Framework;
using Neo4jClient.Cypher;
using Newtonsoft.Json;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class CypherFluentQueryWithTests
    {
        [Test]
        public void With()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .With("foo")
                .Query;

            Assert.AreEqual("START n=node({p0})\r\nWITH foo", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
        }

        [Test]
        public void ShouldReturnCountOnItsOwn()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .With(item => item.Count())
                .Query;

            Assert.AreEqual("WITH count(item)", query.QueryText);
        }

        [Test]
        public void ShouldReturnCountAllOnItsOwn()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .With(() => All.Count())
                .Query;

            Assert.AreEqual("WITH count(*)", query.QueryText);
        }

        [Test]
        public void ShouldReturnCustomFunctionCall()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .With(() => new { baz = "sum(foo.bar)" })
                .Query;

            Assert.AreEqual("WITH sum(foo.bar) AS baz", query.QueryText);
        }

        [Test]
        public void ShouldReturnSpecificPropertyTakingIntoAccountJsonProperty()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .With(a => a.As<Cypher.FooWithJsonProperties>().Bar)
                .Query;

            Assert.AreEqual("WITH a.bar", query.QueryText);
        }

        [Test]
        public void ShouldReturnSpecificPropertyOnItsOwn()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .With(a => a.As<Commodity>().Name)
                .Query;

            Assert.AreEqual("WITH a.Name", query.QueryText);
        }

        [Test]
        public void ShouldReturnSpecificPropertyOnItsOwnCamelAs()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.JsonContractResolver = new CamelCasePropertyNamesContractResolver();
            var query = new CypherFluentQuery(client)
                .With(a => new Commodity(){ Name = a.As<Commodity>().Name})
                .Query;

            Assert.AreEqual("WITH a.name? AS Name", query.QueryText);
        }

        [Test]
        public void ShouldReturnSpecificPropertyOnItsOwnCamel()
        {
            var client = Substitute.For<IRawGraphClient>();
            client.JsonContractResolver = new CamelCasePropertyNamesContractResolver();
            var query = new CypherFluentQuery(client)
                .With(a => a.As<Commodity>().Name)
                .Query;

            Assert.AreEqual("WITH a.name", query.QueryText);
        }

        [Test]
        public void ShouldThrowForMemberExpressionOffMethodOtherThanAs()
        {
            var client = Substitute.For<IRawGraphClient>();
            Assert.Throws<ArgumentException>(
                () => new CypherFluentQuery(client).With(a => a.Type().Length));
        }

        [Test]
        public void ShouldTranslateAnonymousObjectWithExplicitPropertyNames()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .With(a => new { Foo = a })
                .Query;

            Assert.AreEqual("WITH a AS Foo", query.QueryText);
        }

        [Test]
        public void ShouldTranslateAnonymousObjectWithImplicitPropertyNames()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .With(a => new { a })
                .Query;

            Assert.AreEqual("WITH a", query.QueryText);
        }

        [Test]
        public void ShouldTranslateAnonymousObjectWithMultipleProperties()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .With((a, b) => new { a, b })
                .Query;

            Assert.AreEqual("WITH a, b", query.QueryText);
        }

        [Test]
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

            Assert.AreEqual("WITH a, count(b) AS foo, collect(b) AS bar", query.QueryText);
        }

        [Test]
        public void ShouldUseProjectionResultModeForNamedObjectReturnWithConcreteProperties()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .With(a => new ProjectionResult
                {
                    Commodity = a.As<Commodity>()
                })
                .Query;
            
            Assert.AreEqual("WITH a AS Commodity", query.QueryText);
        }

        [Test]
        public void ShouldUseProjectionResultModeForNamedObjectReturnWithICypherResultItems()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .With(a => new CypherProjectionResult
                {
                    Foo = a
                })
                .Query;

            Assert.AreEqual("WITH a AS Foo", query.QueryText);
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
