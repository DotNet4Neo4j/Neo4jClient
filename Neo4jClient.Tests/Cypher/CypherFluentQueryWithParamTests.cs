using System.Globalization;
using Neo4jClient.Serialization;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using NSubstitute;
using Neo4jClient.Cypher;
using System;

namespace Neo4jClient.Test.Cypher
{
    [TestFixture]
    public class CypherFluentQueryWithParamTests
    {
        [Test]
        public void WithParam()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .WithParam("foo", 123)
                .Query;

            // Assert
            Assert.AreEqual("START n=node({p0})", query.QueryText);
            Assert.AreEqual(2, query.QueryParameters.Count);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
            Assert.AreEqual(123, query.QueryParameters["foo"]);
        }

        [Test(Description = "https://bitbucket.org/Readify/neo4jclient/issue/156/passing-cypher-parameters-by-anonymous")]
        public void WithParams()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();

            // Act
            const string bar = "string value";
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .WithParams(new {foo = 123, bar})
                .Query;

            // Assert
            Assert.AreEqual("START n=node({p0})", query.QueryText);
            Assert.AreEqual(3, query.QueryParameters.Count);
            Assert.AreEqual(3, query.QueryParameters["p0"]);
            Assert.AreEqual(123, query.QueryParameters["foo"]);
            Assert.AreEqual("string value", query.QueryParameters["bar"]);
        }

        [Test]
        public void ThrowsExceptionForDuplicateManualKey()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .WithParam("foo", 123);

            // Assert
            var ex = Assert.Throws<ArgumentException>(
                () => query.WithParam("foo", 456)
            );
            Assert.AreEqual("key", ex.ParamName);
            Assert.AreEqual("A parameter with the given key is already defined in the query.\r\nParameter name: key", ex.Message);
        }

        [Test]
        public void ThrowsExceptionForDuplicateOfAutoKey()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3);

            // Assert
            var ex = Assert.Throws<ArgumentException>(
                () => query.WithParam("p0", 456)
            );
            Assert.AreEqual("key", ex.ParamName);
            Assert.AreEqual("A parameter with the given key is already defined in the query.\r\nParameter name: key", ex.Message);
        }

        public class ComplexObjForWithParamTest
        {
            public long? Id { get; set; }
            public string Name { get; set; }
            public decimal Currency { get; set; }
            public string CamelCaseProperty { get; set; }
        }

        [Test]
        public void ComplexObjectInWithParam()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            
            // Act
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference) 3)
                .CreateUnique("n-[:X]-(leaf {obj})")
                .WithParam("obj", CreateComplexObjForWithParamTest())
                .Query;

            // Assert
            Assert.AreEqual("START n=node(3)" +
                            "\r\nCREATE UNIQUE n-[:X]-(leaf {" +
                            "\r\n  \"Id\": 123," +
                            "\r\n  \"Name\": \"Bar\"," +
                            "\r\n  \"Currency\": 12.143," +
                            "\r\n  \"CamelCaseProperty\": \"Foo\"" +
                            "\r\n})", query.DebugQueryText);
            Assert.AreEqual(2, query.QueryParameters.Count);
        }

        private ComplexObjForWithParamTest CreateComplexObjForWithParamTest()
        {
            return new ComplexObjForWithParamTest
            {
                Id = 123,
                Name = "Bar",
                Currency = (decimal) 12.143,
                CamelCaseProperty = "Foo"
            };
        }

        [Test]
        public void ComplexObjectInWithParamCamelCase()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            client.JsonContractResolver.Returns(new CamelCasePropertyNamesContractResolver());

            // Act
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .CreateUnique("n-[:X]-(leaf {obj})")
                .WithParam("obj", CreateComplexObjForWithParamTest())
                .Query;

            // Assert
            Assert.AreEqual("START n=node(3)" +
                            "\r\nCREATE UNIQUE n-[:X]-(leaf {" +
                            "\r\n  \"id\": 123," +
                            "\r\n  \"name\": \"Bar\"," +
                            "\r\n  \"currency\": 12.143," +
                            "\r\n  \"camelCaseProperty\": \"Foo\"" +
                            "\r\n})", query.DebugQueryText);
            Assert.AreEqual(2, query.QueryParameters.Count);
        }
    }
}
