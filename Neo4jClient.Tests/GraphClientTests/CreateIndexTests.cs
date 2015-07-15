using System;
using NUnit.Framework;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class CreateIndexTests
    {
        [Test]
        [TestCase(
            IndexFor.Node,
            IndexProvider.lucene,
            IndexType.fulltext,
            "/index/node",
            @"{
                'name': 'foo',
                'config': { 'type': 'fulltext', 'provider': 'lucene' }
            }")]
        [TestCase(
            IndexFor.Node,
            IndexProvider.lucene,
            IndexType.exact,
            "/index/node",
            @"{
                'name': 'foo',
                'config': { 'type': 'exact', 'provider': 'lucene' }
            }")]
        [TestCase(
            IndexFor.Relationship,
            IndexProvider.lucene,
            IndexType.fulltext,
            "/index/relationship",
            @"{
                'name': 'foo',
                'config': { 'type': 'fulltext', 'provider': 'lucene' }
            }")]
        [TestCase(
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

        [Test]
        [ExpectedException(typeof(ApplicationException))]
        [TestCase(
            IndexFor.Node,
            IndexProvider.lucene,
            IndexType.fulltext,
            "/index/node",
            @"{
                'name': 'foo',
                'config': { 'type': 'fulltext', 'provider': 'lucene' }
            }")]
        [TestCase(
            IndexFor.Node,
            IndexProvider.lucene,
            IndexType.exact,
            "/index/node",
            @"{
                'name': 'foo',
                'config': { 'type': 'exact', 'provider': 'lucene' }
            }")]
        [TestCase(
            IndexFor.Relationship,
            IndexProvider.lucene,
            IndexType.fulltext,
            "/index/relationship",
            @"{
                'name': 'foo',
                'config': { 'type': 'fulltext', 'provider': 'lucene' }
            }")]
        [TestCase(
            IndexFor.Relationship,
            IndexProvider.lucene,
            IndexType.exact,
            "/index/relationship",
            @"{
                'name': 'foo',
                'config': { 'type': 'exact', 'provider': 'lucene' }
            }")]
        public void ShouldThrowApplicationExceptionIfHttpCodeIsNot201(
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
                graphClient.CreateIndex("foo", indexConfiguration, indexFor);
            }
        }
    }
}