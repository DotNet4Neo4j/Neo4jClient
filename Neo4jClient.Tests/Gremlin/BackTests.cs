﻿using Neo4jClient.Gremlin;
using Xunit;

namespace Neo4jClient.Tests.Gremlin
{
    
    public class BackTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void BackVShouldAppendStep()
        {
            var query = new NodeReference(123).BackV<object>("foo");
            Assert.Equal("g.v(p0).back(p1)", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal("foo", query.QueryParameters["p1"]);
        }

        [Fact]
        public void BackVShouldReturnTypedNodeEnumerable()
        {
            var query = new NodeReference(123).BackV<object>("foo");
            Assert.IsAssignableFrom<GremlinNodeEnumerable<object>>(query);
        }

        [Fact]
        public void BackEShouldAppendStep()
        {
            var query = new NodeReference(123).BackE("foo");
            Assert.Equal("g.v(p0).back(p1)", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal("foo", query.QueryParameters["p1"]);
        }

        [Fact]
        public void BackEShouldReturnRelationshipEnumerable()
        {
            var query = new NodeReference(123).BackE("foo");
            Assert.IsAssignableFrom<GremlinRelationshipEnumerable>(query);
        }

        [Fact]
        public void BackEWithTDataShouldAppendStep()
        {
            var query = new NodeReference(123).BackE<object>("foo");
            Assert.Equal("g.v(p0).back(p1)", query.QueryText);
            Assert.Equal(123L, query.QueryParameters["p0"]);
            Assert.Equal("foo", query.QueryParameters["p1"]);
        }

        [Fact]
        public void BackEWithTDataShouldReturnRelationshipEnumerable()
        {
            var query = new NodeReference(123).BackE<object>("foo");
            Assert.IsAssignableFrom<GremlinRelationshipEnumerable<object>>(query);
        }
    }
}
