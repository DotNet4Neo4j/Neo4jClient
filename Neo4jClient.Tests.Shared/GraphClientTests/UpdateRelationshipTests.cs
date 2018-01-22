using System.Net;
using Neo4jClient.Test.Fixtures;
using Xunit;

namespace Neo4jClient.Test.GraphClientTests
{
    
    public class UpdateRelationshipTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void ShouldUpdatePayload()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get("/relationship/456/properties"),
                    MockResponse.Json(HttpStatusCode.OK, "{ 'Foo': 'foo', 'Bar': 'bar', 'Baz': 'baz' }")
                },
                {
                    MockRequest.PutObjectAsJson(
                        "/relationship/456/properties",
                        new TestPayload { Foo = "fooUpdated", Bar = "bar", Baz = "bazUpdated" }),
                    MockResponse.Http((int)HttpStatusCode.NoContent)
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();

                graphClient.Update(
                    new RelationshipReference<TestPayload>(456),
                    payloadFromDb =>
                        {
                            payloadFromDb.Foo = "fooUpdated";
                            payloadFromDb.Baz = "bazUpdated";
                        }
                    );
            }
        }

        [Fact]
        public void ShouldInitializePayloadDuringUpdate()
        {
            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get("/relationship/456/properties"),
                    MockResponse.Http((int)HttpStatusCode.NoContent)
                },
                {
                    MockRequest.PutObjectAsJson(
                        "/relationship/456/properties",
                        new TestPayload { Foo = "fooUpdated", Baz = "bazUpdated" }),
                    MockResponse.Http((int)HttpStatusCode.NoContent)
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();

                graphClient.Update(
                    new RelationshipReference<TestPayload>(456),
                    payloadFromDb =>
                    {
                        payloadFromDb.Foo = "fooUpdated";
                        payloadFromDb.Baz = "bazUpdated";
                    }
                    );
            }
        }

        public class TestPayload
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
            public string Baz { get; set; }
        }
    }
}
