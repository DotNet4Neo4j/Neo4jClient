using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Neo4jClient.Test.Fixtures;
using Neo4jClient.Test.Relationships;
using Neo4jClient.Transactions;
using Xunit;

namespace Neo4jClient.Test.Transactions
{
    
    public class RestCallScenarioTests : IClassFixture<CultureInfoSetupFixture>
    {
        private class TestNode
        {
            public string Foo { get; set; }
        }

        private void ExecuteRestMethodUnderTransaction(
            Action<IGraphClient> restAction,
            TransactionScopeOption option = TransactionScopeOption.Join,
            IEnumerable<KeyValuePair<MockRequest, MockResponse>> requests = null)
        {
            requests = requests ?? Enumerable.Empty<KeyValuePair<MockRequest, MockResponse>>();
            using (var testHarness = new RestTestHarness())
            {
                foreach (var request in requests)
                {
                    testHarness.Add(request.Key, request.Value);
                }
                var client = testHarness.CreateAndConnectTransactionalGraphClient();
                using (var transaction = client.BeginTransaction(option))
                {
                    restAction(client);
                }
            }
        }

        [Fact]
        public void CreateNodeIndexShouldFailUnderTransaction()
        {
            Assert.Throws<InvalidOperationException>(() => ExecuteRestMethodUnderTransaction(
                client => client.CreateIndex("node", new IndexConfiguration(), IndexFor.Node)));
        }

        [Fact]
        public void CreateRelationshipIndexShouldFailUnderTransaction()
        {
            Assert.Throws<InvalidOperationException>(() => ExecuteRestMethodUnderTransaction(
                client => client.CreateIndex("rel", new IndexConfiguration(), IndexFor.Relationship)));
        }

        [Fact]
        public void CreateRelationshipShouldFailUnderTransaction()
        {
            var nodeReference = new NodeReference<RootNode>(1);
            var rel = new OwnedBy(new NodeReference<TestNode>(3));
            Assert.Throws<InvalidOperationException>(() => ExecuteRestMethodUnderTransaction(client => client.CreateRelationship(nodeReference, rel)));
        }

        [Fact]
        public void CreateShouldFailUnderTransaction()
        {
            Assert.Throws<InvalidOperationException>(() =>
                ExecuteRestMethodUnderTransaction(client => client.Create(new object())));
        }

        [Fact]
        public void DeleteAndRelationshipsShouldFailUnderTransaction()
        {
            var pocoReference = new NodeReference<TestNode>(456);
            Assert.Throws<InvalidOperationException>(() => ExecuteRestMethodUnderTransaction(client => client.Delete(pocoReference, DeleteMode.NodeAndRelationships)));
        }

        [Fact]
        public void DeleteIndexEntriesShouldFailUnderTransaction()
        {
            var pocoReference = new NodeReference<TestNode>(456);
            Assert.Throws<InvalidOperationException>(() => ExecuteRestMethodUnderTransaction(client => client.DeleteIndexEntries("rel", pocoReference)));
        }

        [Fact]
        public void DeleteNodeIndexShouldFailUnderTransaction()
        {
            Assert.Throws<InvalidOperationException>(() => ExecuteRestMethodUnderTransaction(client => client.DeleteIndex("node", IndexFor.Node)));
        }

        [Fact]
        public void DeleteNodeShouldFailUnderTransaction()
        {
            var pocoReference = new NodeReference<TestNode>(456);
            Assert.Throws<InvalidOperationException>(() => ExecuteRestMethodUnderTransaction(client => client.Delete(pocoReference, DeleteMode.NodeOnly)));
        }

        [Fact]
        public void DeleteRelationshipIndexShouldFailUnderTransaction()
        {
            Assert.Throws<InvalidOperationException>(() =>
                ExecuteRestMethodUnderTransaction(client => client.DeleteIndex("rel", IndexFor.Relationship)));
        }

        [Fact]
        public void DeleteRelationshipShouldFailUnderTransaction()
        {
            var relReference = new RelationshipReference(1);
            Assert.Throws<InvalidOperationException>(() => ExecuteRestMethodUnderTransaction(client => client.DeleteRelationship(relReference)));
        }

        [Fact]
        public void GetNodeIndexesShouldFailUnderTransaction()
        {
            Assert.Throws<InvalidOperationException>(() => ExecuteRestMethodUnderTransaction(client => client.GetIndexes(IndexFor.Node)));
        }

        [Fact]
        public void GetNodeShouldFailUnderTransaction()
        {
            var nodeReference = new NodeReference<TestNode>(1);
            Assert.Throws<InvalidOperationException>(() => ExecuteRestMethodUnderTransaction(client => client.Get(nodeReference)));
        }

        [Fact]
        public void GetRelationshipIndexesShouldUnderTransaction()
        {
            Assert.Throws<InvalidOperationException>(() => ExecuteRestMethodUnderTransaction(client => client.GetIndexes(IndexFor.Relationship)));
        }

        [Fact]
        public void GetRelationshipShouldFailUnderTransaction()
        {
            var relReference = new RelationshipReference<TestNode>(1);
            Assert.Throws<InvalidOperationException>(() => ExecuteRestMethodUnderTransaction(client => client.Get(relReference)));
        }

        [Fact]
        public void GetShouldSucceedInSuppressedMode()
        {
            var nodeReference = new NodeReference<TestNode>(1);
            var requests = new List<KeyValuePair<MockRequest, MockResponse>>
            {
                new KeyValuePair<MockRequest, MockResponse>(
                    MockRequest.Get("/node/1"),
                    MockResponse.Json(HttpStatusCode.OK,
                        @"{ 'self': 'http://foo/db/data/node/456',
                          'data': { 'Foo': 'foo'
                          },
                          'create_relationship': 'http://foo/db/data/node/1/relationships',
                          'all_relationships': 'http://foo/db/data/node/1/relationships/all',
                          'all_typed relationships': 'http://foo/db/data/node/1/relationships/all/{-list|&|types}',
                          'incoming_relationships': 'http://foo/db/data/node/1/relationships/in',
                          'incoming_typed relationships': 'http://foo/db/data/node/1/relationships/in/{-list|&|types}',
                          'outgoing_relationships': 'http://foo/db/data/node/1/relationships/out',
                          'outgoing_typed relationships': 'http://foo/db/data/node/1/relationships/out/{-list|&|types}',
                          'properties': 'http://foo/db/data/node/1/properties',
                          'property': 'http://foo/db/data/node/1/property/{key}',
                          'traverse': 'http://foo/db/data/node/1/traverse/{returnType}'
                        }")
                    )
            };
            ExecuteRestMethodUnderTransaction(
                client => client.Get(nodeReference),
                TransactionScopeOption.Suppress,
                requests);
        }

        [Fact]
        public void ReIndexShouldFailUnderTransaction()
        {
            var nodeReference = new NodeReference(1);
            var indexEntries = new[] {new IndexEntry("node")};
            Assert.Throws<InvalidOperationException>(() => ExecuteRestMethodUnderTransaction(client => client.ReIndex(nodeReference, indexEntries)));
        }

        [Fact]
        public void UpdateShouldFailUnderTransaction()
        {
            Assert.Throws<InvalidOperationException>(() =>
                ExecuteRestMethodUnderTransaction(client =>
                {
                    var pocoReference = new NodeReference<TestNode>(456);
                    var updatedNode = client.Update(
                        pocoReference, nodeFromDb => { nodeFromDb.Foo = "fooUpdated"; });
                }));
        }
    }
}