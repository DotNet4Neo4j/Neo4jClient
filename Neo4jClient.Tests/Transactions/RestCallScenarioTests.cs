using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Neo4jClient.Test.Relationships;
using Neo4jClient.Transactions;
using NUnit.Framework;

namespace Neo4jClient.Test.Transactions
{
    [TestFixture]
    public class RestCallScenarioTests
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

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateShouldFailUnderTransaction()
        {
            ExecuteRestMethodUnderTransaction(client => client.Create(new object()));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UpdateShouldFailUnderTransaction()
        {
            ExecuteRestMethodUnderTransaction(client =>
            {
                var pocoReference = new NodeReference<TestNode>(456);
                var updatedNode = client.Update(
                    pocoReference, nodeFromDb =>
                    {
                        nodeFromDb.Foo = "fooUpdated";
                    });
            });
        }

        [Test]
        [ExpectedException(typeof (InvalidOperationException))]
        public void DeleteNodeShouldFailUnderTransaction()
        {
            var pocoReference = new NodeReference<TestNode>(456);
            ExecuteRestMethodUnderTransaction(client => client.Delete(pocoReference, DeleteMode.NodeOnly));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DeleteAndRelationshipsShouldFailUnderTransaction()
        {
            var pocoReference = new NodeReference<TestNode>(456);
            ExecuteRestMethodUnderTransaction(client => client.Delete(pocoReference, DeleteMode.NodeAndRelationships));
        }

        [Test]
        [ExpectedException(typeof (InvalidOperationException))]
        public void DeleteNodeIndexShouldFailUnderTransaction()
        {
            ExecuteRestMethodUnderTransaction(client => client.DeleteIndex("node", IndexFor.Node));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DeleteRelationshipIndexShouldFailUnderTransaction()
        {
            ExecuteRestMethodUnderTransaction(client => client.DeleteIndex("rel", IndexFor.Relationship));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DeleteIndexEntriesShouldFailUnderTransaction()
        {
            var pocoReference = new NodeReference<TestNode>(456);
            ExecuteRestMethodUnderTransaction(client => client.DeleteIndexEntries("rel", pocoReference));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DeleteRelationshipShouldFailUnderTransaction()
        {
            var relReference = new RelationshipReference(1);
            ExecuteRestMethodUnderTransaction(client => client.DeleteRelationship(relReference));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetNodeShouldFailUnderTransaction()
        {
            var nodeReference = new NodeReference<TestNode>(1);
            ExecuteRestMethodUnderTransaction(client => client.Get(nodeReference));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetRelationshipShouldFailUnderTransaction()
        {
            var relReference = new RelationshipReference<TestNode>(1);
            ExecuteRestMethodUnderTransaction(client => client.Get(relReference));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetNodeIndexesShouldFailUnderTransaction()
        {
            ExecuteRestMethodUnderTransaction(client => client.GetIndexes(IndexFor.Node));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetRelationshipIndexesShouldUnderTransaction()
        {
            ExecuteRestMethodUnderTransaction(client => client.GetIndexes(IndexFor.Relationship));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ReIndexShouldFailUnderTransaction()
        {
            var nodeReference = new NodeReference(1);
            var indexEntries = new IndexEntry[] {new IndexEntry("node")};
            ExecuteRestMethodUnderTransaction(client => client.ReIndex(nodeReference, indexEntries));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateRelationshipShouldFailUnderTransaction()
        {
            var nodeReference = new NodeReference<RootNode>(1);
            var rel = new OwnedBy(new NodeReference<TestNode>(3));
            ExecuteRestMethodUnderTransaction(client => client.CreateRelationship(nodeReference, rel));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateNodeIndexShouldFailUnderTransaction()
        {
            ExecuteRestMethodUnderTransaction(
                client => client.CreateIndex("node", new IndexConfiguration(), IndexFor.Node));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateRelationshipIndexShouldFailUnderTransaction()
        {
            ExecuteRestMethodUnderTransaction(
                client => client.CreateIndex("rel", new IndexConfiguration(), IndexFor.Relationship));
        }

        [Test]
        public void GetShouldSucceedInSuppressedMode()
        {
            var nodeReference = new NodeReference<TestNode>(1);
            var requests = new List<KeyValuePair<MockRequest, MockResponse>>()
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

    }
}
