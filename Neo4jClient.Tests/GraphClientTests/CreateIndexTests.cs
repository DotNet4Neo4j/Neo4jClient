using System;
using System.Threading.Tasks;
using Xunit;

namespace Neo4jClient.Tests.GraphClientTests
{
    
    // public class CreateIndexTests : IClassFixture<CultureInfoSetupFixture>
    // {
    //     [Theory]
    //     [InlineData(
    //         IndexFor.Node,
    //         IndexProvider.lucene,
    //         IndexType.fulltext,
    //         "/index/node",
    //         @"{
    //             'name': 'foo',
    //             'config': { 'type': 'fulltext', 'provider': 'lucene' }
    //         }")]
    //     [InlineData(
    //         IndexFor.Node,
    //         IndexProvider.lucene,
    //         IndexType.exact,
    //         "/index/node",
    //         @"{
    //             'name': 'foo',
    //             'config': { 'type': 'exact', 'provider': 'lucene' }
    //         }")]
    //     [InlineData(
    //         IndexFor.Relationship,
    //         IndexProvider.lucene,
    //         IndexType.fulltext,
    //         "/index/relationship",
    //         @"{
    //             'name': 'foo',
    //             'config': { 'type': 'fulltext', 'provider': 'lucene' }
    //         }")]
    //     [InlineData(
    //         IndexFor.Relationship,
    //         IndexProvider.lucene,
    //         IndexType.exact,
    //         "/index/relationship",
    //         @"{
    //             'name': 'foo',
    //             'config': { 'type': 'exact', 'provider': 'lucene' }
    //         }")]
    //     public async Task ShouldCreateIndex(
    //         IndexFor indexFor,
    //         IndexProvider indexProvider,
    //         IndexType indexType,
    //         string createEndpoint,
    //         string createJson)
    //     {
    //         //Arrange
    //         using (var testHarness = new RestTestHarness
    //         {
    //             {
    //                 MockRequest.PostJson(createEndpoint, createJson),
    //                 MockResponse.Http(201)
    //             }
    //         })
    //         {
    //             var graphClient = await testHarness.CreateAndConnectGraphClient();
    //
    //             var indexConfiguration = new IndexConfiguration
    //             {
    //                 Provider = indexProvider,
    //                 Type = indexType
    //             };
    //             await graphClient.CreateIndexAsync("foo", indexConfiguration, indexFor);
    //         }
    //     }
    //
    //     [Theory]
    //     [InlineData(
    //         IndexFor.Node,
    //         IndexProvider.lucene,
    //         IndexType.fulltext,
    //         "/index/node",
    //         @"{
    //             'name': 'foo',
    //             'config': { 'type': 'fulltext', 'provider': 'lucene' }
    //         }")]
    //     [InlineData(
    //         IndexFor.Node,
    //         IndexProvider.lucene,
    //         IndexType.exact,
    //         "/index/node",
    //         @"{
    //             'name': 'foo',
    //             'config': { 'type': 'exact', 'provider': 'lucene' }
    //         }")]
    //     [InlineData(
    //         IndexFor.Relationship,
    //         IndexProvider.lucene,
    //         IndexType.fulltext,
    //         "/index/relationship",
    //         @"{
    //             'name': 'foo',
    //             'config': { 'type': 'fulltext', 'provider': 'lucene' }
    //         }")]
    //     [InlineData(
    //         IndexFor.Relationship,
    //         IndexProvider.lucene,
    //         IndexType.exact,
    //         "/index/relationship",
    //         @"{
    //             'name': 'foo',
    //             'config': { 'type': 'exact', 'provider': 'lucene' }
    //         }")]
    //     public async Task ShouldThrowExceptionIfHttpCodeIsNot201(
    //         IndexFor indexFor,
    //         IndexProvider indexProvider,
    //         IndexType indexType,
    //         string createEndpoint,
    //         string createJson)
    //     {
    //         //Arrange
    //         using (var testHarness = new RestTestHarness
    //         {
    //             {
    //                 MockRequest.PostJson(createEndpoint, createJson),
    //                 MockResponse.Http(500)
    //             }
    //         })
    //         {
    //             var graphClient = await testHarness.CreateAndConnectGraphClient();
    //
    //             var indexConfiguration = new IndexConfiguration
    //             {
    //                 Provider = indexProvider,
    //                 Type = indexType
    //             };
    //             await Assert.ThrowsAsync<Exception>(async () => await graphClient.CreateIndexAsync("foo", indexConfiguration, indexFor));
    //         }
    //     }
    // }
}