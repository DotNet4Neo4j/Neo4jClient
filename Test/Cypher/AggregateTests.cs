﻿using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Neo4jClient.Cypher;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class AggregateTests
    {
        [Test]
        public void Length()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(n => new { Foo = n.Length() })
                .Query;

            Assert.AreEqual("RETURN length(n) AS Foo", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count());
            Assert.AreEqual(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Test]
        public void Type()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(n => new { Foo = n.Type() })
                .Query;

            Assert.AreEqual("RETURN type(n) AS Foo", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count());
            Assert.AreEqual(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Test]
        public void Id()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(n => new { Foo = n.Id() })
                .Query;

            Assert.AreEqual("RETURN id(n) AS Foo", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count());
            Assert.AreEqual(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Test]
        public void Count()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(n => new { Foo = n.Count() })
                .Query;

            Assert.AreEqual("RETURN count(n) AS Foo", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count());
            Assert.AreEqual(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

 


        [Test]
        public void CountDistinct()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(n => new { Foo = n.CountDistinct() })
                .Query;

            Assert.AreEqual("RETURN count(distinct n) AS Foo", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count());
            Assert.AreEqual(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Test]
        public void CountAll()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(() => new
                {
                    Foo = All.Count()
                })
                .Query;

            Assert.AreEqual("RETURN count(*) AS Foo", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count());
            Assert.AreEqual(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }

        [Test]
        public void CountUsingICypderFluentQuery()
        {
            var client = Substitute.For<IRawGraphClient>();
            ICypherFluentQuery query = new CypherFluentQuery(client);

            var resultQuery =
                query.Return(() => new { Foo = All.Count() })
            .Query;

            Assert.AreEqual("RETURN count(*) AS Foo", resultQuery.QueryText);
            Assert.AreEqual(0, resultQuery.QueryParameters.Count());
            Assert.AreEqual(CypherResultFormat.DependsOnEnvironment, resultQuery.ResultFormat);
        }

        [Test]
        public void CountAllWithOtherIdentitiesWithNode()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(bar => new
                {
                    Foo = All.Count(),
                    Baz = bar.CollectAs<Node<object>>()
                })
                .Query;

            Assert.AreEqual("RETURN count(*) AS Foo, collect(bar) AS Baz", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count());
            Assert.AreEqual(CypherResultFormat.Rest, query.ResultFormat);
        }

        [Test]
        public void CountAllWithOtherIdentities()
        {
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Return(bar => new
                {
                    Foo = All.Count(),
                    Baz = bar.CollectAs<object>()
                })
                .Query;

            Assert.AreEqual("RETURN count(*) AS Foo, collect(bar) AS Baz", query.QueryText);
            Assert.AreEqual(0, query.QueryParameters.Count());
            Assert.AreEqual(CypherResultFormat.DependsOnEnvironment, query.ResultFormat);
        }
    }
}
