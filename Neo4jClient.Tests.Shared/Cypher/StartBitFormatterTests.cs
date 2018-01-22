using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;
using Neo4jClient.Cypher;
using Neo4jClient.Test.Fixtures;

namespace Neo4jClient.Test.Cypher
{
    
    public class StartBitFormatterTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        //[Description("http://docs.neo4j.org/chunked/2.0.0-M01/query-start.html#start-node-by-id")]
        public void SingleNodeReference()
        {
            var cypher = ToCypher(new
            {
                n1 = (NodeReference) 1
            });

            Assert.Equal("n1=node({p0})", cypher.QueryText);
            Assert.Equal(1, cypher.QueryParameters.Count);
            Assert.Equal(1L, cypher.QueryParameters["p0"]);
        }

        [Fact]
        public void SingleNode()
        {
            var cypher = ToCypher(new
            {
                n1 = new Node<object>(new object(), 123, null)
            });

            Assert.Equal("n1=node({p0})", cypher.QueryText);
            Assert.Equal(1, cypher.QueryParameters.Count);
            Assert.Equal(123L, cypher.QueryParameters["p0"]);
        }

        [Fact]
        public void EnumerableOfNodes()
        {
            var cypher = ToCypher(new
            {
                n1 = new[] {123, 456}.Select(id => new Node<object>(new object(), id, null))
            });

            Assert.Equal("n1=node({p0})", cypher.QueryText);
            Assert.Equal(1, cypher.QueryParameters.Count);
            Assert.Equal(new[] {123L, 456L}, cypher.QueryParameters["p0"]);
        }

        [Fact]
        public void RootNodeReference()
        {
            var cypher = ToCypher(new
            {
                n1 = new RootNode(123)
            });

            Assert.Equal("n1=node({p0})", cypher.QueryText);
            Assert.Equal(1, cypher.QueryParameters.Count);
            Assert.Equal(123L, cypher.QueryParameters["p0"]);
        }

        [Fact]
        //[Description("http://docs.neo4j.org/chunked/2.0.0-M01/query-start.html#start-node-by-id")]
        public void MultipleNodeReferences()
        {
            var cypher = ToCypher(new
            {
                n1 = (NodeReference)1,
                n2 = (NodeReference)2
            });

            Assert.Equal("n1=node({p0}), n2=node({p1})", cypher.QueryText);
            Assert.Equal(2, cypher.QueryParameters.Count);
            Assert.Equal(1L, cypher.QueryParameters["p0"]);
            Assert.Equal(2L, cypher.QueryParameters["p1"]);
        }

        [Fact]
        //[Description("http://docs.neo4j.org/chunked/2.0.0-M01/query-start.html#start-multiple-nodes-by-id")]
        public void ArrayOfNodeReferences()
        {
            var cypher = ToCypher(new
            {
                n1 = new NodeReference[] { 1, 2 }
            });

            Assert.Equal("n1=node({p0})", cypher.QueryText);
            Assert.Equal(1, cypher.QueryParameters.Count);
            Assert.Equal(new[] {1L, 2L}, cypher.QueryParameters["p0"]);
        }

        [Fact]
        //[Description("http://docs.neo4j.org/chunked/2.0.0-M01/query-start.html#start-multiple-nodes-by-id")]
        public void EnumerableOfNodeReferences()
        {
            var cypher = ToCypher(new
            {
                n1 = new[] { 1, 2 }.Select(id => (NodeReference)id)
            });

            Assert.Equal("n1=node({p0})", cypher.QueryText);
            Assert.Equal(1, cypher.QueryParameters.Count);
            Assert.Equal(new[] { 1L, 2L }, cypher.QueryParameters["p0"]);
        }

        [Fact]
        //[Description("http://docs.neo4j.org/chunked/2.0.0-M01/query-start.html#start-multiple-nodes-by-id")]
        public void EnumerableOfTypedNodeReferences()
        {
            var cypher = ToCypher(new
            {
                n1 = new[] { 1, 2 }.Select(id => (NodeReference<object>)id)
            });

            Assert.Equal("n1=node({p0})", cypher.QueryText);
            Assert.Equal(1, cypher.QueryParameters.Count);
            Assert.Equal(new[] { 1L, 2L }, cypher.QueryParameters["p0"]);
        }

        [Fact]
        public void SingleRelationshipReference()
        {
            var cypher = ToCypher(new
            {
                r1 = (RelationshipReference)1
            });

            Assert.Equal("r1=relationship({p0})", cypher.QueryText);
            Assert.Equal(1, cypher.QueryParameters.Count);
            Assert.Equal(1L, cypher.QueryParameters["p0"]);
        }

        [Fact]
        public void ArrayOfRelationshipReferences()
        {
            var cypher = ToCypher(new
            {
                r1 = new RelationshipReference[] { 1, 2 }
            });

            Assert.Equal("r1=relationship({p0})", cypher.QueryText);
            Assert.Equal(1, cypher.QueryParameters.Count);
            Assert.Equal(new[] {1L, 2L}, cypher.QueryParameters["p0"]);
        }

        [Fact]
        public void EnumerableOfRelationshipReferences()
        {
            var cypher = ToCypher(new
            {
                r1 = new[] { 1, 2 }.Select(id => (RelationshipReference)id)
            });

            Assert.Equal("r1=relationship({p0})", cypher.QueryText);
            Assert.Equal(1, cypher.QueryParameters.Count);
            Assert.Equal(new[] { 1L, 2L }, cypher.QueryParameters["p0"]);
        }

        [Fact]
        //[Description("http://docs.neo4j.org/chunked/2.0.0-M01/query-start.html#start-all-nodes")]
        public void AllNodes()
        {
            var cypher = ToCypher(new
            {
                n = All.Nodes
            });

            Assert.Equal("n=node(*)", cypher.QueryText);
            Assert.Equal(0, cypher.QueryParameters.Count);
        }

        [Fact]
        public void CustomString()
        {
            var cypher = ToCypher(new
            {
                n1 = "foo"
            });

            Assert.Equal("n1=foo", cypher.QueryText);
            Assert.Equal(0, cypher.QueryParameters.Count);
        }

        [Fact]
        //[Description("http://docs.neo4j.org/chunked/2.0.0-M01/query-start.html#start-node-by-index-lookup")]
        public void NodeByIndexLookup()
        {
            var cypher = ToCypher(new
            {
                n = Node.ByIndexLookup("someIndex", "name", "A")
            });

            Assert.Equal("n=node:`someIndex`(name = {p0})", cypher.QueryText);
            Assert.Equal(1, cypher.QueryParameters.Count);
            Assert.Equal("A", cypher.QueryParameters["p0"]);
        }

        [Fact]
        //[Description("http://docs.neo4j.org/chunked/2.0.0-M01/query-start.html#start-node-by-index-query")]
        public void NodeByIndexQuery()
        {
            var cypher = ToCypher(new
            {
                n = Node.ByIndexQuery("someIndex", "name:A")
            });

            Assert.Equal("n=node:`someIndex`({p0})", cypher.QueryText);
            Assert.Equal(1, cypher.QueryParameters.Count);
            Assert.Equal("name:A", cypher.QueryParameters["p0"]);
        }

        [Fact]
        //[Description("http://docs.neo4j.org/chunked/2.0.0-M01/query-start.html#start-relationship-by-index-lookup")]
        public void RelationshipByIndexLookup()
        {
            var cypher = ToCypher(new
            {
                r = Relationship.ByIndexLookup("someIndex", "name", "A")
            });

            Assert.Equal("r=relationship:`someIndex`(name = {p0})", cypher.QueryText);
            Assert.Equal(1, cypher.QueryParameters.Count);
            Assert.Equal("A", cypher.QueryParameters["p0"]);
        }

        [Fact]
        public void RelationshipByIndexQuery()
        {
            var cypher = ToCypher(new
            {
                r = Relationship.ByIndexQuery("someIndex", "name:A")
            });

            Assert.Equal("r=relationship:`someIndex`({p0})", cypher.QueryText);
            Assert.Equal(1, cypher.QueryParameters.Count);
            Assert.Equal("name:A", cypher.QueryParameters["p0"]);
        }

        [Fact]
        public void Mixed()
        {
            var nodeRef = (NodeReference)2;
            var relRef = (RelationshipReference)3;
            var relRef2 = (RelationshipReference)4;

            var cypher = ToCypher(new
            {
                n1 = "custom",
                n2 = nodeRef,
                n3 = Node.ByIndexLookup("indexName", "property", "value"),
                n4 = Node.ByIndexQuery("indexName", "query"),
                r1 = relRef,
                moreRels = new[] { relRef, relRef2 },
                r2 = Relationship.ByIndexLookup("indexName", "property", "value"),
                r3 = Relationship.ByIndexQuery("indexName", "query"),
                all = All.Nodes
            });

            const string expected =
                "n1=custom, " +
                "n2=node({p0}), " +
                "n3=node:`indexName`(property = {p1}), " +
                "n4=node:`indexName`({p2}), " +
                "r1=relationship({p3}), " +
                "moreRels=relationship({p4}), " +
                "r2=relationship:`indexName`(property = {p5}), " +
                "r3=relationship:`indexName`({p6}), " +
                "all=node(*)";

            Assert.Equal(expected, cypher.QueryText);
            Assert.Equal(7, cypher.QueryParameters.Count);
            Assert.Equal(2L, cypher.QueryParameters["p0"]);
            Assert.Equal("value", cypher.QueryParameters["p1"]);
            Assert.Equal("query", cypher.QueryParameters["p2"]);
            Assert.Equal(3L, cypher.QueryParameters["p3"]);
            Assert.Equal(new[] {3L, 4L}, cypher.QueryParameters["p4"]);
            Assert.Equal("value", cypher.QueryParameters["p5"]);
            Assert.Equal("query", cypher.QueryParameters["p6"]);
        }

        [Fact]
        public void ThrowArgumentExceptionForEmptyObject()
        {
            var emptyObject = new {};
            var ex = Assert.Throws<ArgumentException>(
                () => StartBitFormatter.FormatAsCypherText(emptyObject, null)
            );
            Assert.Equal("startBits", ex.ParamName);
        }

        [Fact]
        public void DontThrowArgumentExceptionForEmptyDictionary()
        {
            var emptyDictionary = new Dictionary<string, object>();
            var ex = Record.Exception(() => StartBitFormatter.FormatAsCypherText(emptyDictionary, null));
            ex.Should().BeNull();
        }

        [Fact]
        public void ThrowNotSupportedExceptionForUnknownType()
        {
            var badObject = new { n1 = new StartBitFormatterTests() };
            Assert.Throws<NotSupportedException>(
                () => StartBitFormatter.FormatAsCypherText(badObject, null)
            );
        }

        [Fact]
        public void NotSupportedExceptionForUnknownTypeIncludesIdentityName()
        {
            var badObject = new { n1 = new StartBitFormatterTests() };
            var exception = Assert.Throws<NotSupportedException>(
                () => StartBitFormatter.FormatAsCypherText(badObject, null)
            );
            Assert.Contains("n1", exception.Message);
        }

        [Fact]
        public void NotSupportedExceptionForUnknownTypeIncludesTypeName()
        {
            var badObject = new { n1 = new StartBitFormatterTests() };
            var exception = Assert.Throws<NotSupportedException>(
                () => StartBitFormatter.FormatAsCypherText(badObject, null)
            );
            Assert.Contains(typeof(StartBitFormatterTests).FullName, exception.Message);
        }

        [Fact]
        public void ThrowArgumentExceptionForNullValue()
        {
            var startBits = new { foo = (object)null };
            Assert.Throws<ArgumentException>(
                () => StartBitFormatter.FormatAsCypherText(startBits, null)
            );
        }

        [Fact]
        public void ArgumentExceptionForNullValueIncludesPropertyName()
        {
            var startBits = new { foo = (object)null };
            var ex = Assert.Throws<ArgumentException>(
                () => StartBitFormatter.FormatAsCypherText(startBits, null)
            );
            Assert.Contains("foo", ex.Message);
        }

        static CypherQuery ToCypher(object startBits)
        {
            var parameters = new Dictionary<string, object>();
            Func<object, string> createParameter = value =>
            {
                var name = "p" + parameters.Count;
                parameters.Add(name, value);
                return string.Format("{{{0}}}", name);
            };

            var cypherText = StartBitFormatter.FormatAsCypherText(startBits, createParameter);

            var query = new CypherQuery(cypherText, parameters, CypherResultMode.Projection, CypherResultFormat.Rest);
            return query;
        }
    }
}
