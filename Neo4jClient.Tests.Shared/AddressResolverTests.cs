using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Neo4j.Driver.V1;
using Neo4jClient.Test.Fixtures;
using Xunit;

namespace Neo4jClient.Tests.Shared
{
    public class AddressResolverTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void CreatesEmptyListIfNullIsPassedIn()
        {
            var ar = new AddressResolver(new Uri("bolt://virtual.uri:1234"), null);
            var response = ar.Resolve(ServerAddress.From("virtual.uri", 1234));
            response.Should().HaveCount(0);
        }

        [Fact]
        public void PassesBackCorrectUris()
        {
            const string uri1 = "x.acme.com";
            const string uri2 = "y.acme.com";

            var ar = new AddressResolver("bolt://virtual.uri", new []{$"bolt://{uri1}", "bolt://" + uri2});
            var response = ar.Resolve(null).ToList();
            response.Should().HaveCount(2);
            response.Any(x => x.Host == uri1).Should().BeTrue();
            response.Any(x => x.Host == uri2).Should().BeTrue();
        }
    }

    public class UriCreatorTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Theory]
        [InlineData("x.foo.com", "scheme://x.foo.com:7687/", 7687)]
        [InlineData("x.foo.com:7687", "scheme://x.foo.com:7687/", 7687)]
        [InlineData("x.foo.com:7688", "scheme://x.foo.com:7688/", 7688)]
        [InlineData("bolt://x.foo.com:7688", "bolt://x.foo.com:7688/", 7688)]
        public void GeneratesTheCorrectUri(string input, string expectedUri, int expectedPort)
        {
            var response = UriCreator.From(input);
            response.AbsoluteUri.Should().Be(expectedUri);
            response.Port.Should().Be(expectedPort);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void ReturnsNullWhenNullOrWhitespacePassedIn(string uri)
        {
            UriCreator.From(uri).Should().BeNull();
        }
    }
}