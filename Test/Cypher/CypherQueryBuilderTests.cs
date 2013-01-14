using NUnit.Framework;
using Neo4jClient.Cypher;
using System.Linq.Expressions;
using System;
using NSubstitute;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class CypherQueryBuilderTests
    {
        [Test]
        public void ShouldUseProjectionResultModeForLambdaBasedReturn()
        {
            Expression<Func<ICypherResultItem, object>> expression =
                a => new { Foo = a.As<object>() };

            var query = new CypherQueryBuilder()
                .SetReturn(expression, false)
                .ToQuery();

            Assert.AreEqual(CypherResultMode.Projection, query.ResultMode);
        }
    }
}
