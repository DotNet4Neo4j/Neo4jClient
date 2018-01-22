using System;
using Neo4jClient.Test.Fixtures;
using Xunit;

namespace Neo4jClient.Test.GraphClientTests
{
    
    public class CreateIndexTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Theory]
        [InlineData(
            IndexFor.Node,
            IndexProvider.lucene,
            IndexType.fulltext,
            "/index/node",
            @"{
                'name': 'foo',
                'config': { 'type': 'fulltext', 'provider': 'lucene' }
            }")]
        [InlineData(
            IndexFor.Node,
            IndexProvider.lucene,
            IndexType.exact,
            "/index/node",
            @"{
                'name': 'foo',
                'config': { 'type': 'exact', 'provider': 'lucene' }
            }")]
        [InlineData(
            IndexFor.Relationship,
            IndexProvider.lucene,
            IndexType.fulltext,
            "/index/relationship",
            @"{
                'name': 'foo',
                'config': { 'type': 'fulltext', 'provider': 'lucene' }
            }")]
        [InlineData(
            IndexFor.Relationship,
            IndexProvider.lucene,
            IndexType.exact,
            "/index/relationship",
            @"{
                'name': 'foo',
                'config': { 'type': 'exact', 'provider': 'lucene' }
            }")]
        public void ShouldCreateIndex(
            IndexFor indexFor,
            IndexProvider indexProvider,
            IndexType indexType,
            string createEndpoint,
            string createJson)
        {
            //Arrange
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.PostJson(createEndpoint, createJson),
                    MockResponse.Http(201)
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();

                var indexConfiguration = new IndexConfiguration
                {
                    Provider = indexProvider,
                    Type = indexType
                };
                graphClient.CreateIndex("foo", indexConfiguration, indexFor);
            }
        }

        [Theory]
        [InlineData(
            IndexFor.Node,
            IndexProvider.lucene,
            IndexType.fulltext,
            "/index/node",
            @"{
                'name': 'foo',
                'config': { 'type': 'fulltext', 'provider': 'lucene' }
            }")]
        [InlineData(
            IndexFor.Node,
            IndexProvider.lucene,
            IndexType.exact,
            "/index/node",
            @"{
                'name': 'foo',
                'config': { 'type': 'exact', 'provider': 'lucene' }
            }")]
        [InlineData(
            IndexFor.Relationship,
            IndexProvider.lucene,
            IndexType.fulltext,
            "/index/relationship",
            @"{
                'name': 'foo',
                'config': { 'type': 'fulltext', 'provider': 'lucene' }
            }")]
        [InlineData(
            IndexFor.Relationship,
            IndexProvider.lucene,
            IndexType.exact,
            "/index/relationship",
            @"{
                'name': 'foo',
                'config': { 'type': 'exact', 'provider': 'lucene' }
            }")]
        public void ShouldThrowExceptionIfHttpCodeIsNot201(
            IndexFor indexFor,
            IndexProvider indexProvider,
            IndexType indexType,
            string createEndpoint,
            string createJson)
        {
            //Arrange
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.PostJson(createEndpoint, createJson),
                    MockResponse.Http(500)
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();

                var indexConfiguration = new IndexConfiguration
                {
                    Provider = indexProvider,
                    Type = indexType
                };
                Assert.Throws<Exception>(() => graphClient.CreateIndex("foo", indexConfiguration, indexFor));
            }
        }
    }
}