using System;
using FluentAssertions;
using Neo4jClient.Test.Fixtures;
using Xunit;

namespace Neo4jClient.Test.GraphClientTests
{
    
    public class DeleteRelationshipTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void ShouldThrowInvalidOperationExceptionIfNotConnected()
        {
            var client = new GraphClient(new Uri("http://foo"));
            Assert.Throws<InvalidOperationException>(() => client.DeleteRelationship(123));
        }

        [Fact]
        public void ShouldDeleteRelationship()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Delete("/relationship/456"),
                    MockResponse.Http(204)
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();
                graphClient.DeleteRelationship(456);
            }
        }

        [Fact]
        public void ShouldThrowExceptionWhenDeleteFails()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Delete("/relationship/456"),
                    MockResponse.Http(404)
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();
                var ex = Assert.Throws<Exception>(() => graphClient.DeleteRelationship(456));
                ex.Message.Should().Be("Unable to delete the relationship. The response status was: 404 NotFound");
            }
        }
    }
}
