using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Neo4jClient.Cypher;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.Cypher
{
    public class CypherFluentQueryInTransactionsTests : IClassFixture<CultureInfoSetupFixture>
    {
        private static IRawGraphClient GraphClient_44
        {
            get
            {
                var client = Substitute.For<IRawGraphClient>();
                client.CypherCapabilities.Returns(CypherCapabilities.Cypher44);
                return client;
            }
        }

        [Fact]
        public void AddsInTransactionsWithNoRows_WhenRowsNotSpecified()
        {
            var client = GraphClient_44;
            var query = new CypherFluentQuery(client)
                .Call("{arlo()}")
                .InTransactions()
                .Query;

            Assert.Equal("CALL {arlo()}\r\nIN TRANSACTIONS", query.QueryText);
        }

        [Fact]
        public void AddsInTransactionsWithRows_WhenRowsSpecified()
        {
            var client = GraphClient_44;
            var query = new CypherFluentQuery(client)
                .Call("{arlo()}")
                .InTransactions(1000)
                .Query;

            Assert.Equal("CALL {arlo()}\r\nIN TRANSACTIONS OF 1000 ROWS", query.QueryText);
        }

        [Fact]
        public void AddsInTransactionsWithNoRows_WhenRowsAreNull()
        {
            var client = GraphClient_44;
            var query = new CypherFluentQuery(client)
                .Call("{arlo()}")
                .InTransactions(null)
                .Query;

            Assert.Equal("CALL {arlo()}\r\nIN TRANSACTIONS", query.QueryText);
        }
    }

    public class CypherFluentQueryCallTests : IClassFixture<CultureInfoSetupFixture>
    {
        private class Foo
        {
            public int Id { get; set; }
        }

        private static IRawGraphClient GraphClient_30
        {
            get
            {
                var client = Substitute.For<IRawGraphClient>();
                client.CypherCapabilities.Returns(CypherCapabilities.Cypher30);
                return client;
            }
        }

        [Fact]
        public void CallsStoredProcedureGiven()
        {
            var client = GraphClient_30;
            var query = new CypherFluentQuery(client)
                .Call("apoc.sp()")
                .Query;

            Assert.Equal("CALL apoc.sp()", query.QueryText);
        }

        [Fact]
        public void ThrowArgumentException_WhenNoStoredProcedureIsGiven()
        {
            var client = GraphClient_30;
            Assert.Throws<ArgumentException>(() => new CypherFluentQuery(client).Call(null).Query);
        }

        [Fact]
        public void ThrowsInvalidOperationException_WhenClientVersionIsLessThan_30()
        {
            var client = GraphClient_30;
            client.CypherCapabilities.Returns(CypherCapabilities.Cypher23);

            Assert.Throws<InvalidOperationException>(() => new CypherFluentQuery(client).Call("apoc.sp").Query);
        }

        [Fact]
        public void Call_SubQueriesAsLambda()
        {
            const string expected = "CALL { MATCH (n)\r\nRETURN count(n) AS c }\r\nRETURN c";

            var client = GraphClient_30;
            
            var query = new CypherFluentQuery(client)
                .Match("(n)")
                .Return(n => new {c = n.Count()});
            
            var callQuery = new CypherFluentQuery(client)
                .Call(() => query)
                .Return(c => c.As<long>());

            callQuery.Query.QueryText.Should().Be(expected);
        }

         [Fact]
         public void Call_SubQueriesAsLambdaWithParameters()
         {
             const string expected = "CALL { MATCH (n)\r\nWHERE (n.Id = $p0)\r\nRETURN count(n) AS c }\r\nRETURN c";

             var client = GraphClient_30;
             var query = new CypherFluentQuery(client)
                 .Match("(n)")
                 .Where((Foo n) => n.Id == 1)
                 .Return(n => new {c = n.Count()});
             
             var callQuery = new CypherFluentQuery(client)
                 .Call(() => query)
                 .Return(c => c.As<long>());

             callQuery.Query.QueryText.Should().Be(expected);
             callQuery.Query.QueryParameters.Should().HaveCount(1);
         }

         [Fact]
         public void Call_SubQueriesAsLambdaWithParametersPriorToCall()
         {
             const string expected = "MATCH (x)\r\nWHERE (x.Id = $p0)\r\nCALL { MATCH (n)\r\nWHERE (n.Id = $p1)\r\nRETURN count(n) AS c }\r\nRETURN c";

             var client = GraphClient_30;
             var query = new CypherFluentQuery(client)
                 .Match("(n)")
                 .Where((Foo n) => n.Id == 1)
                 .Return(n => new {c = n.Count()});
             
             var callQuery = new CypherFluentQuery(client)
                 .Match("(x)")
                 .Where((Foo x) => x.Id == 2)
                 .Call(() => query)
                 .Return(c => c.As<long>());

             callQuery.Query.QueryText.Should().Be(expected);
             callQuery.Query.QueryParameters.Should().HaveCount(2);
         }

         [Fact]
         public void Call_SubQueriesAsLambdaWithParametersAfterToCall()
         {
             const string expected = "CALL { MATCH (n)\r\nWHERE (n.Id = $p0)\r\nRETURN count(n) AS c }\r\nMATCH (x)\r\nWHERE (x.Id = $p1)\r\nRETURN c";

             var client = GraphClient_30;
             var query = new CypherFluentQuery(client)
                 .Match("(n)")
                 .Where((Foo n) => n.Id == 1)
                 .Return(n => new {c = n.Count()});
             
             var callQuery = new CypherFluentQuery(client)
                 .Call(() => query)
                 .Match($"(x)")
                 .Where((Foo x) => x.Id == 2)
                 .Return(c => c.As<long>());

             callQuery.Query.QueryText.Should().Be(expected);
             callQuery.Query.QueryParameters.Should().HaveCount(2);
         }

         [Fact]
         public void Call_SubQueriesAsLambdaWithParametersPriorToCall_WholeWord()
         {
             const string expected = "MATCH (x)\r\nWHERE (x.Id = $p0)\r\nCALL { MATCH (n)\r\nWHERE (n.Id = $p1)\r\nAND (n.Something = $p2)\r\nRETURN count(n) AS c }\r\nRETURN c";

             var client = GraphClient_30;
             var query = new CypherFluentQuery(client)
                 .Match("(n)")
                 .Where("(n.Id = $p0)")
                 .AndWhere("(n.Something = $p1)")
                 .WithParam("p0", 1)
                 .WithParam("p1", 2)
                 .Return(n => new {c = n.Count()});
             
             var callQuery = new CypherFluentQuery(client)
                 .Match("(x)")
                 .Where("(x.Id = $p0)")
                 .WithParam("p0", 3)
                 .Call(() => query)
                 .Return(c => c.As<long>());

             callQuery.Query.QueryText.Should().Be(expected);
             callQuery.Query.QueryParameters.Should().HaveCount(3);
             callQuery.Query.QueryParameters.Should().ContainKeys("p0", "p1", "p2");
         }

         [Fact]
         public void Call_SubQueriesAsLambdaWithParametersPriorAndAfterToCall_WholeWord()
         {
             const string expected = "MATCH (x)\r\nWHERE (x.Id = $p0)\r\nCALL { MATCH (n)\r\nWHERE (n.Id = $p1)\r\nAND (n.Something = $p2)\r\nRETURN count(n) AS c }\r\nWHERE (y.Id = $p3)\r\nRETURN c";

             var client = GraphClient_30;
             var query = new CypherFluentQuery(client)
                 .Match("(n)")
                 .Where("(n.Id = $p0)")
                 .AndWhere("(n.Something = $p1)")
                 .WithParam("p0", 1)
                 .WithParam("p1", 2)
                 .Return(n => new {c = n.Count()});
             
             var callQuery = new CypherFluentQuery(client)
                 .Match("(x)")
                 .Where((IdClass x) => x.Id == 1)
                 .Call(() => query)
                 .Where((IdClass y) => y.Id == 1)
                 .Return(c => c.As<long>());

             callQuery.Query.QueryText.Should().Be(expected);
             callQuery.Query.QueryParameters.Should().HaveCount(4);
             callQuery.Query.QueryParameters.Should().ContainKeys("p0", "p1", "p2", "p3");
         }
         private class IdClass {public int Id { get;set; }}
    }
}