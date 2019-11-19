﻿using System;
using Neo4jClient.Cypher;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.Cypher
{
    public class CypherFluentQueryCreateUniqueTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void CreateNodeWithValuesViaCreateUnique()
        {
            // http://docs.neo4j.org/chunked/1.8.M03/query-relate.html#relate-create-nodes-with-values
            //START root=node(2)
            //CREATE UNIQUE root-[:X]-(leaf {name:'D'} )
            //RETURN leaf

            var client = Substitute.For<IRawGraphClient>();
            client.ServerVersion.Returns(new Version(1, 8));
            var query = new CypherFluentQuery(client)
                .Start("root", (NodeReference)2)
                .CreateUnique("root-[:X]-(leaf {name:'D'} )")
                .Return<object>("leaf")
                .Query;

            Assert.Equal($"START root=node({{p0}}){Environment.NewLine}CREATE UNIQUE root-[:X]-(leaf {{name:'D'}} ){Environment.NewLine}RETURN leaf", query.QueryText);
            Assert.Equal(2l, query.QueryParameters["p0"]);
        }

        [Fact]
        public void CreateNodeWithValuesViaCreateUniqueAfterMatch()
        {
            //START root=node(2)
            //MATCH root-[:X]-foo
            //CREATE UNIQUE foo-[:Y]-(leaf {name:'D'} )
            //RETURN leaf

            var client = Substitute.For<IRawGraphClient>();
            client.ServerVersion.Returns(new Version(1, 8));
            var query = new CypherFluentQuery(client)
                .Start("root", (NodeReference)2)
                .Match("root-[:X]-foo")
                .CreateUnique("foo-[:Y]-(leaf {name:'D'} )")
                .Return<object>("leaf")
                .Query;

            Assert.Equal($"START root=node({{p0}}){Environment.NewLine}MATCH root-[:X]-foo{Environment.NewLine}CREATE UNIQUE foo-[:Y]-(leaf {{name:'D'}} ){Environment.NewLine}RETURN leaf", query.QueryText);
            Assert.Equal(2l, query.QueryParameters["p0"]);
        }
    }
}
