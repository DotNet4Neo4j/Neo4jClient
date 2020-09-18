using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Neo4j.Driver;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;
using Neo4jClient.Tests.BoltGraphClientTests;
using Neo4jClient.Tests.Transactions;
using Neo4jClient.Transactions;
using Xunit;

namespace Neo4jClient.Tests
{
    public class QueryStatsTests
    {
        public class Bolt
        {
            public class ReturningResults : IClassFixture<CultureInfoSetupFixture>
            {
                [Fact]
                public async Task ReturnsQueryStats_WhenNotInTransaction()
                {
                    BoltClientTestHelper.GetDriverAndSession(out var mockDriver, out _, out _);

                    bool completedRaised = false;
                    QueryStats stats = null;

                    var cypherQuery = new CypherQuery("MATCH (n) RETURN id(n)", new Dictionary<string, object>(), CypherResultMode.Set, CypherResultFormat.Transactional, "neo4j", includeQueryStats:true);

                    using (var testHarness = new BoltTestHarness())
                    {
                        var value = new Dictionary<string, object> {{"id(n)", 4}};
                        var recordMock = new Mock<IRecord>();
                        recordMock.Setup(x => x.Values).Returns(value);
                        recordMock.Setup(x => x["id(n)"]).Returns(4);
                        recordMock.Setup(x => x.Keys).Returns(new[] {"id(n)"});

                        var testStatementResult = new TestStatementResult(new[] { "id(n)" }, recordMock.Object);
                        testHarness.SetupCypherRequestResponse(cypherQuery.QueryText, cypherQuery.QueryParameters, testStatementResult);

                        var client = await testHarness.CreateAndConnectBoltGraphClient();
                        client.OperationCompleted += (o, e) =>
                        {
                            stats = e.QueryStats;
                            completedRaised = true;
                        };

                        
                        var results = (await client.ExecuteGetCypherResultsAsync<long>(cypherQuery)).ToArray();
                        results.First().Should().Be(4);
                    }

                    completedRaised.Should().BeTrue();
                    stats.Should().NotBeNull();
                }

                [Fact]
                public async Task ReturnsQueryStats_WhenInTransaction()
                {
                   // BoltClientTestHelper.GetDriverAndSession(out var mockDriver, out _, out _);

                    bool completedRaised = false;
                    QueryStats stats = null;

                    var cypherQuery = new CypherQuery("MATCH (n) RETURN id(n)", new Dictionary<string, object>(), CypherResultMode.Set, CypherResultFormat.Transactional, "neo4j", includeQueryStats: true);

                    var value = new Dictionary<string, object> { { "id(n)", 4 } };
                    var recordMock = new Mock<IRecord>();
                    recordMock.Setup(x => x.Values).Returns(value);
                    recordMock.Setup(x => x["id(n)"]).Returns(4);
                    recordMock.Setup(x => x.Keys).Returns(new[] { "id(n)" });
                    var testStatementResult = new TestStatementResult(new[] { "id(n)" }, recordMock.Object);

                    using (var testHarness = new BoltTestHarness())
                    {
                        testHarness.SetupCypherRequestResponse(cypherQuery.QueryText, cypherQuery.QueryParameters, testStatementResult);

                        var client = await testHarness.CreateAndConnectBoltGraphClient();
                        client.OperationCompleted += (o, e) =>
                        {
                            stats = e.QueryStats;
                            completedRaised = true;
                        };

                        using (var tx = ((ITransactionalGraphClient) client).BeginTransaction())
                        {
                            var results = (await client.ExecuteGetCypherResultsAsync<long>(cypherQuery)).ToArray();
                            results.First().Should().Be(4);
                        }
                    }

                    completedRaised.Should().BeTrue();
                    stats.Should().NotBeNull();
                }
            }

            public class NoResults : IClassFixture<CultureInfoSetupFixture>
            {
                [Fact]
                public async Task ReturnsQueryStats_WhenNotInTransaction()
                {
                    BoltClientTestHelper.GetDriverAndSession(out var mockDriver, out _, out _);

                    bool completedRaised = false;
                    QueryStats stats = null;

                    var client = new BoltGraphClient(mockDriver);
                    await client.ConnectAsync();

                    client.OperationCompleted += (o, e) =>
                    {
                        stats = e.QueryStats;
                        completedRaised = true;
                    };
                    
                    await client.Cypher.WithQueryStats.Match("n").Return(n => n.Count()).ExecuteWithoutResultsAsync();
                    completedRaised.Should().BeTrue();
                    stats.Should().NotBeNull();
                }

                [Fact]
                public async Task ReturnsQueryStats_WhenInTransaction()
                {
                    BoltClientTestHelper.GetDriverAndSession(out var mockDriver, out _, out _);

                    bool completedRaised = false;
                    QueryStats stats = null;

                    var client = new BoltGraphClient(mockDriver);
                    client.OperationCompleted += (o, e) =>
                    {
                        stats = e.QueryStats;
                        completedRaised = true;
                    };


                    await client.ConnectAsync();
                    using (var tx = client.BeginTransaction())
                    {
                        await client.Cypher.WithQueryStats.Match("n").Return(n => n.Count()).ExecuteWithoutResultsAsync();
                        completedRaised.Should().BeTrue();
                        stats.Should().NotBeNull();
                    }
                }
            }
        }

        public class Http
        {
            public class ReturningResults : IClassFixture<CultureInfoSetupFixture>
            {
                [Fact]
                public async Task ReturnsQueryStats_WhenNotInTransaction()
                {
                    var response = MockResponse.Json(200, @"{
            'results' : [ {
                'columns' : [ 'id(n)' ],
                'data' : [ {
                    'row' : [ 4 ],
                    'meta' : [ null ]
                } ],
                'stats' : {
                    'contains_updates' : true,
                    'nodes_created' : 1,
                    'nodes_deleted' : 0,
                    'properties_set' : 0,
                    'relationships_created' : 0,
                    'relationship_deleted' : 0,
                    'labels_added' : 0,
                    'labels_removed' : 0,
                    'indexes_added' : 0,
                    'indexes_removed' : 0,
                    'constraints_added' : 0,
                    'constraints_removed' : 0,
                    'contains_system_updates' : false,
                    'system_updates' : 0
                }
            } ],
            'errors' : [ ]
        }
        ");
                    const string queryText = @"MATCH (n) RETURN id(n)";

                    var cypherQuery = new CypherQuery(queryText, null, CypherResultMode.Set, CypherResultFormat.Rest, "neo4j", includeQueryStats: true);
                    var transactionApiQuery = new CypherStatementList {new CypherTransactionStatement(cypherQuery)};

                    using (var testHarness = new RestTestHarness
                    {
                        {MockRequest.PostObjectAsJson("/transaction/commit", transactionApiQuery), response}
                    })
                    {
                        var graphClient = await testHarness.CreateAndConnectGraphClient();
                        var completedRaised = false;
                        QueryStats stats = null;
                        graphClient.OperationCompleted += (o, e) =>
                        {
                            stats = e.QueryStats;
                            completedRaised = true;
                        };

                        var results = await graphClient.ExecuteGetCypherResultsAsync<int>(cypherQuery);
                        results.First().Should().Be(4);

                        completedRaised.Should().BeTrue();
                        stats.Should().NotBeNull();
                        stats.NodesCreated.Should().Be(1);
                    }
                }

                [Fact]
                public async Task ReturnsQueryStats_WhenInTransaction()
                {
                    var response = MockResponse.Json(201, @"{
    'results': [
        {
            'columns': [
                'id(n)'
            ],
            'data': [
                {
                    'row': [
                        4
                    ],
                    'meta': [
                        null
                    ]
                }
            ],
            'stats': {
                'contains_updates': false,
                'nodes_created': 0,
                'nodes_deleted': 0,
                'properties_set': 0,
                'relationships_created': 0,
                'relationship_deleted': 0,
                'labels_added': 0,
                'labels_removed': 0,
                'indexes_added': 0,
                'indexes_removed': 0,
                'constraints_added': 0,
                'constraints_removed': 0,
                'contains_system_updates': false,
                'system_updates': 0
            }
        }
    ],
    'errors': [],
    'commit': 'http://localhost:7474/db/neo4j/tx/2/commit',
    'transaction': {
        'expires': 'Wed, 16 Sep 2020 09:21:48 GMT'
    }
}");
                    const string database = "neo4j";
                    var initTransactionRequest = MockRequest.PostJson($"/db/{database}/tx", @"{
  'statements': [
    {
      'statement': 'MATCH (n)\r\nRETURN id(n)',
      'resultDataContents': [],
      'parameters': {},
      'includeStats': true
    }
  ]
}");
                    var rollbackTransactionRequest = MockRequest.Delete($"/db/{database}/tx/1");
                    using (var testHarness = new RestTestHarness(false, "http://foo:7474")
                    {
                        {
                            initTransactionRequest, response
                        },
                        {rollbackTransactionRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")}
                    })
                    {
                        var graphClient = await testHarness.CreateAndConnectTransactionalGraphClient(RestTestHarness.Neo4jVersion.Neo40);
                        var completedRaised = false;
                        QueryStats stats = null;
                        graphClient.OperationCompleted += (o, e) =>
                        {
                            stats = e.QueryStats;
                            completedRaised = true;
                        };

                        using (var tx = graphClient.BeginTransaction())
                        {
                            var query = graphClient.Cypher.WithQueryStats.Match("(n)").Return(n => n.Id());
                            query.Query.IncludeQueryStats.Should().BeTrue();

                            var results = await query.ResultsAsync;
                            results.First().Should().Be(4);

                            completedRaised.Should().BeTrue();
                            stats.Should().NotBeNull();
                        }
                    }
                }
            }

            public class NoResults : IClassFixture<CultureInfoSetupFixture>
            {
                [Fact]
                public async Task ReturnsQueryStats_WhenNotInTransaction()
                {
                    var response = MockResponse.Json(200, @"{
            'results' : [ {
                'columns' : [ 'id(n)' ],
                'data' : [ {
                    'row' : [ 4 ],
                    'meta' : [ null ]
                } ],
                'stats' : {
                    'contains_updates' : true,
                    'nodes_created' : 1,
                    'nodes_deleted' : 0,
                    'properties_set' : 0,
                    'relationships_created' : 0,
                    'relationship_deleted' : 0,
                    'labels_added' : 0,
                    'labels_removed' : 0,
                    'indexes_added' : 0,
                    'indexes_removed' : 0,
                    'constraints_added' : 0,
                    'constraints_removed' : 0,
                    'contains_system_updates' : false,
                    'system_updates' : 0
                }
            } ],
            'errors' : [ ]
        }
        ");
                    const string queryText = @"MATCH (n) RETURN id(n)";

                    var cypherQuery = new CypherQuery(queryText, null, CypherResultMode.Set, CypherResultFormat.Rest, "neo4j", includeQueryStats: true);
                    var transactionApiQuery = new CypherStatementList {new CypherTransactionStatement(cypherQuery)};

                    using (var testHarness = new RestTestHarness
                    {
                        {MockRequest.PostObjectAsJson("/transaction/commit", transactionApiQuery), response}
                    })
                    {
                        var graphClient = await testHarness.CreateAndConnectGraphClient();
                        QueryStats stats = null;
                        graphClient.OperationCompleted += (o, e) => { stats = e.QueryStats; };
                        await graphClient.ExecuteCypherAsync(cypherQuery);

                        stats.Should().NotBeNull();
                        stats.NodesCreated.Should().Be(1);
                    }
                }

                [Fact]
                public async Task ReturnsQueryStats_WhenInTransaction()
                {
                    var response = MockResponse.Json(201, @"{
    'results': [
        {
            'columns': [
                'id(n)'
            ],
            'data': [
                {
                    'row': [
                        4
                    ],
                    'meta': [
                        null
                    ]
                }
            ],
            'stats': {
                'contains_updates': false,
                'nodes_created': 0,
                'nodes_deleted': 0,
                'properties_set': 0,
                'relationships_created': 0,
                'relationship_deleted': 0,
                'labels_added': 0,
                'labels_removed': 0,
                'indexes_added': 0,
                'indexes_removed': 0,
                'constraints_added': 0,
                'constraints_removed': 0,
                'contains_system_updates': false,
                'system_updates': 0
            }
        }
    ],
    'errors': [],
    'commit': 'http://localhost:7474/db/neo4j/tx/2/commit',
    'transaction': {
        'expires': 'Wed, 16 Sep 2020 09:21:48 GMT'
    }
}");
                    const string database = "neo4j";
                    var initTransactionRequest = MockRequest.PostJson($"/db/{database}/tx", @"{
  'statements': [
    {
      'statement': 'MATCH (n)\r\nRETURN id(n)',
      'resultDataContents': [],
      'parameters': {},
      'includeStats': true
    }
  ]
}");
                    var rollbackTransactionRequest = MockRequest.Delete($"/db/{database}/tx/1");
                    using (var testHarness = new RestTestHarness(false, "http://foo:7474")
                    {
                        {
                            initTransactionRequest, response
                        },
                        {rollbackTransactionRequest, MockResponse.Json(200, @"{'results':[], 'errors':[] }")}
                    })
                    {
                        var graphClient = await testHarness.CreateAndConnectTransactionalGraphClient(RestTestHarness.Neo4jVersion.Neo40);
                        var completedRaised = false;
                        QueryStats stats = null;
                        graphClient.OperationCompleted += (o, e) =>
                        {
                            stats = e.QueryStats;
                            completedRaised = true;
                        };

                        using (var tx = graphClient.BeginTransaction())
                        {
                            var query = graphClient.Cypher.WithQueryStats.Match("(n)").Return(n => n.Id());
                            query.Query.IncludeQueryStats.Should().BeTrue();

                            await query.ExecuteWithoutResultsAsync();

                            completedRaised.Should().BeTrue();
                            stats.Should().NotBeNull();
                        }
                    }
                }
            }
        }
    }
}