using System;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace Neo4jClient.Tests.GraphClientTests
{
    
    public class DefaultDatabaseTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Theory]
        [InlineData("FOO")]
        [InlineData("foo")] 
        [InlineData("Foo")] 
        [InlineData("fOO")] 
        [InlineData("FoO")]
        public void ShouldAlwaysBeLowercase(string database)
        {
            const string expected = "foo";
            var client = new GraphClient(new Uri("http://foo")){DefaultDatabase = database};
            client.DefaultDatabase.Should().Be(expected);
        }

        [Fact]
        public void ShouldBeNeo4jIfNotSet()
        {
            const string expected = "neo4j";
            var client = new GraphClient(new Uri("http://foo"));
            client.DefaultDatabase.Should().Be(expected);
        }
    }
}
