using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;

namespace Neo4jClient.Test.GraphClientTests.Cypher
{
    [TestFixture]
    public class ExecuteCypherTests
    {
        [Test]
        public void ShouldSendCommandAndNotCareAboutResults()
        {
            // Arrange
            const string queryText = @"START d=node({p0}), e=node({p1}) CREATE UNIQUE d-[:foo]->e";
            var parameters = new Dictionary<string, object>
            {
                {"p0", 215},
                {"p1", 219}
            };

            var cypherQuery = new CypherQuery(queryText, parameters, CypherResultMode.Set);
            var cypherApiQuery = new CypherApiQuery(cypherQuery);

            using (var testHarness = new RestTestHarness
            {
                {
                    MockRequest.PostObjectAsJson("/cypher", cypherApiQuery),
                    MockResponse.Http((int)HttpStatusCode.OK)
                }
            })
            {
                var graphClient = testHarness.CreateAndConnectGraphClient();

                //Act
                graphClient.ExecuteCypher(cypherQuery);
            }
        }
    }
}
