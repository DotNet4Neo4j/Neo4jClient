using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Neo4jClient.Test.GraphClientTests.Cypher
{
    [TestFixture]
    public class ResultsTests
    {
        readonly Uri fakeEndpoint = new Uri("http://test.example.com/foo");

        [Test]
        public void ReturnColumnAlias()
        {
            // http://docs.neo4j.org/chunked/1.6/query-return.html#return-column-alias
            // START a=node(1)
            // RETURN a.Age AS SomethingTotallyDifferent

            var client = new GraphClient(fakeEndpoint);
            var results = client
                .Cypher
                .Start("a", (NodeReference)1)
                .Return(a => new ReturnPropertyQueryResult
                {
                    SomethingTotallyDifferent = a.As<FooNode>().Age
                })
                .Results;

            Assert.IsInstanceOf<IEnumerable<ReturnPropertyQueryResult>>(results);
        }

        public class FooNode
        {
            public int Age { get; set; }
        }

        public class ReturnPropertyQueryResult
        {
            public int SomethingTotallyDifferent { get; set; }
        }
    }
}
