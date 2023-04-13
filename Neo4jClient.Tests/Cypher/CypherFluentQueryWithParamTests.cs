using System;
using FluentAssertions;
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
                .Match("n")
                .WithParam("foo", 123)
                .Query;

            // Assert
            Assert.Equal("MATCH n", query.QueryText);
            Assert.Equal(1, query.QueryParameters.Count);
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
                .Match("n")
                .WithParams(new {foo = 123, bar})
                .Query;

            // Assert
            Assert.Equal("MATCH n", query.QueryText);
            Assert.Equal(2, query.QueryParameters.Count);
            Assert.Equal(123, query.QueryParameters["foo"]);
            Assert.Equal("string value", query.QueryParameters["bar"]);
        }

        [Fact]
        public void ThrowsExceptionForDuplicateManualKey()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("n")
                .WithParam("foo", 123)
                .WithParam("bar", 123);

            // Assert
            var ex = Assert.Throws<ArgumentException>(
                () => query.WithParam("foo", 456)
            );
            Assert.Equal("key", ex.ParamName);
            Assert.Equal("A parameter with the given key 'foo' is already defined in the query. (Parameter 'key')", ex.Message);

            ex = Assert.Throws<ArgumentException>(
                () => query.WithParams( new { foo = 456 })
            );
            Assert.Equal("parameters", ex.ParamName);
            Assert.Equal("A parameter with the given key 'foo' is already defined in the query. (Parameter 'parameters')", ex.Message);

            ex = Assert.Throws<ArgumentException>(
                () => query.WithParams(new { foo = 456, bar = 456 })
            );
            Assert.Equal("parameters", ex.ParamName);
            Assert.Equal("Parameters with the given keys 'foo, bar' are already defined in the query. (Parameter 'parameters')", ex.Message);
        }

        [Fact]
        public void ThrowsExceptionForDuplicateOfAutoKey()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            var query = new CypherFluentQuery(client)
                .Match("n")
                .Where((FooWithJsonProperties n) => n.Bar == "test");

            // Assert
            var ex = Assert.Throws<ArgumentException>(
                () => query.WithParam("p0", 456)
            );
            Assert.Equal("key", ex.ParamName);
            Assert.Equal("A parameter with the given key 'p0' is already defined in the query. (Parameter 'key')", ex.Message);
        }

        [Fact]
        //(Description = https://github.com/DotNet4Neo4j/Neo4jClient/issues/458)
        public void ThrowsExceptionForInvalidParamName()
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();

            // Assert WithParam
            var ex = Assert.Throws<ArgumentException>(() => new CypherFluentQuery(client).WithParam("$uuid", ""));
            ex.Should().NotBeNull();
            ex.Message.Should().Be("The parameter with the given key '$uuid' is not valid. Parameters may consist of letters and numbers, and any combination of these, but cannot start with a number or a currency symbol. (Parameter 'key')");

            ex = Assert.Throws<ArgumentException>(() => new CypherFluentQuery(client).WithParam("0uuid", ""));
            ex.Should().NotBeNull();
            ex.Message.Should().Be("The parameter with the given key '0uuid' is not valid. Parameters may consist of letters and numbers, and any combination of these, but cannot start with a number or a currency symbol. (Parameter 'key')");

            ex = Assert.Throws<ArgumentException>(() => new CypherFluentQuery(client).WithParam("{uuid}", ""));
            ex.Should().NotBeNull();
            ex.Message.Should().Be("The parameter with the given key '{uuid}' is not valid. Parameters may consist of letters and numbers, and any combination of these, but cannot start with a number or a currency symbol. (Parameter 'key')");

            // no exception for correct usage
            var ex2 = Record.Exception(() => new CypherFluentQuery(client).WithParam("uuid", ""));
            Assert.Null(ex2);

            // Assert WithParams
            //ex = Assert.Throws<ArgumentException>(() => new CypherFluentQuery(client).WithParams(new { $uuid = "" })); // this will not compile anyways

            // no exception for correct usage
            ex2 = Record.Exception(() => new CypherFluentQuery(client).WithParams(new { uuid = "" }));
            Assert.Null(ex2);
        }

        public class ComplexObjForWithParamTest
        {
            public long? Id { get; set; }
            public string Name { get; set; }
            public decimal Currency { get; set; }
            public string CamelCaseProperty { get; set; }
        }

        [Theory]
        [InlineData("$obj")]
        public void ComplexObjectInWithParam(string param)
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            
            // Act
            var query = new CypherFluentQuery(client)
                .Match("n")
                .CreateUnique($"n-[:X]-(leaf {param})")
                .WithParam("obj", CreateComplexObjForWithParamTest())
                .Query;

            // Assert
            Assert.Equal("MATCH n" +
                            Environment.NewLine + "CREATE UNIQUE n-[:X]-(leaf {" +
                            Environment.NewLine + "  Id: 123," +
                            Environment.NewLine + "  Name: \"Bar\"," +
                            Environment.NewLine + "  Currency: 12.143," +
                            Environment.NewLine + "  CamelCaseProperty: \"Foo\"" +
                            Environment.NewLine + "})", query.DebugQueryText);
            Assert.Equal(1, query.QueryParameters.Count);
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
        [InlineData("$obj")]
        public void ComplexObjectInWithParamCamelCase(string param)
        {
            // Arrange
            var client = Substitute.For<IRawGraphClient>();
            client.JsonContractResolver.Returns(new CamelCasePropertyNamesContractResolver());

            // Act
            var query = new CypherFluentQuery(client)
                .Match("n")
                .CreateUnique($"n-[:X]-(leaf {param})")
                .WithParam("obj", CreateComplexObjForWithParamTest())
                .Query;

            // Assert
            Assert.Equal("MATCH n" +
                            Environment.NewLine + "CREATE UNIQUE n-[:X]-(leaf {" +
                            Environment.NewLine + "  id: 123," +
                            Environment.NewLine + "  name: \"Bar\"," +
                            Environment.NewLine + "  currency: 12.143," +
                            Environment.NewLine + "  camelCaseProperty: \"Foo\"" +
                            Environment.NewLine + "})", query.DebugQueryText);
            Assert.Equal(1, query.QueryParameters.Count);
        }
    }
}
