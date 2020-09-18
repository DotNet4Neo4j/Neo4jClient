using System;
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
                .Match("root")
                .CreateUnique("root-[:X]-(leaf {name:'D'} )")
                .Return<object>("leaf")
                .Query;

            Assert.Equal($"MATCH root{Environment.NewLine}CREATE UNIQUE root-[:X]-(leaf {{name:'D'}} ){Environment.NewLine}RETURN leaf", query.QueryText);
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
                .Match("root-[:X]-foo")
                .CreateUnique("foo-[:Y]-(leaf {name:'D'} )")
                .Return<object>("leaf")
                .Query;

            Assert.Equal($"MATCH root-[:X]-foo{Environment.NewLine}CREATE UNIQUE foo-[:Y]-(leaf {{name:'D'}} ){Environment.NewLine}RETURN leaf", query.QueryText);
        }
    }
}
