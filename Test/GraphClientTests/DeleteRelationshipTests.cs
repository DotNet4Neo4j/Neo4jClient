using System;
using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using RestSharp;

namespace Neo4jClient.Test.GraphClientTests
{
    [TestFixture]
    public class DeleteRelationshipTests
    {
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ShouldThrowInvalidOperationExceptionIfNotConnected()
        {
            var client = new GraphClient(new Uri("http://foo"));
            client.DeleteRelationship(123);
        }

        [Test]
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

        [Test]
        [ExpectedException(typeof(ApplicationException), ExpectedMessage = "Unable to delete the relationship. The response status was: 404 NotFound")]
        public void ShouldThrowApplicationExceptionWhenDeleteFails()
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
                graphClient.DeleteRelationship(456);
            }
        }
    }
}
