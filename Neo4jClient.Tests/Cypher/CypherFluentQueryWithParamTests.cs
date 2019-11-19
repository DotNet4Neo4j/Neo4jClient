using System;
using Neo4jClient.Cypher;
using Newtonsoft.Json.Serialization;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.Cypher
{
    
    public class CypherFluentQueryWithParamTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void WithParam()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .WithParam("foo", 123)
                .Query;

            // Assert
            Assert.Equal("START n=node({p0})", query.QueryText);
            Assert.Equal(2, query.QueryParameters.Count);
            Assert.Equal(3L, query.QueryParameters["p0"]);
            Assert.Equal(123, query.QueryParameters["foo"]);
        }

        [Fact]
        //(Description = "https://bitbucket.org/Readify/neo4jclient/issue/156/passing-cypher-parameters-by-anonymous")
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
            Assert.Equal("START n=node({p0})", query.QueryText);
            Assert.Equal(3, query.QueryParameters.Count);
            Assert.Equal(3L, query.QueryParameters["p0"]);
            Assert.Equal(123, query.QueryParameters["foo"]);
            Assert.Equal("string value", query.QueryParameters["bar"]);
        }

        [Fact]
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
            Assert.Equal("key", ex.ParamName);
            Assert.Equal("A parameter with the given key is already defined in the query." + Environment.NewLine + "Parameter name: key", ex.Message);
        }

        [Fact]
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
            Assert.Equal("key", ex.ParamName);
            Assert.Equal("A parameter with the given key is already defined in the query." + Environment.NewLine + "Parameter name: key", ex.Message);
        }

        public class ComplexObjForWithParamTest
        {
            public long? Id { get; set; }
            public string Name { get; set; }
            public decimal Currency { get; set; }
            public string CamelCaseProperty { get; set; }
        }

        [Theory]
        [InlineData("{obj}")]
        [InlineData("$obj")]
        public void ComplexObjectInWithParam(string param)
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            
            // Act
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference) 3)
                .CreateUnique($"n-[:X]-(leaf {param})")
                .WithParam("obj", CreateComplexObjForWithParamTest())
                .Query;

            // Assert
            Assert.Equal("START n=node(3)" +
                            Environment.NewLine + "CREATE UNIQUE n-[:X]-(leaf {" +
                            Environment.NewLine + "  \"Id\": 123," +
                            Environment.NewLine + "  \"Name\": \"Bar\"," +
                            Environment.NewLine + "  \"Currency\": 12.143," +
                            Environment.NewLine + "  \"CamelCaseProperty\": \"Foo\"" +
                            Environment.NewLine + "})", query.DebugQueryText);
            Assert.Equal(2, query.QueryParameters.Count);
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

        [Theory]
        [InlineData("{obj}")]
        [InlineData("$obj")]
        public void ComplexObjectInWithParamCamelCase(string param)
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            client.JsonContractResolver.Returns(new CamelCasePropertyNamesContractResolver());

            // Act
            var query = new CypherFluentQuery(client)
                .Start("n", (NodeReference)3)
                .CreateUnique($"n-[:X]-(leaf {param})")
                .WithParam("obj", CreateComplexObjForWithParamTest())
                .Query;

            // Assert
            Assert.Equal("START n=node(3)" +
                            Environment.NewLine + "CREATE UNIQUE n-[:X]-(leaf {" +
                            Environment.NewLine + "  \"id\": 123," +
                            Environment.NewLine + "  \"name\": \"Bar\"," +
                            Environment.NewLine + "  \"currency\": 12.143," +
                            Environment.NewLine + "  \"camelCaseProperty\": \"Foo\"" +
                            Environment.NewLine + "})", query.DebugQueryText);
            Assert.Equal(2, query.QueryParameters.Count);
        }
    }
}
