using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using FluentAssertions;
using Neo4j.Driver.V1;
using Neo4jClient.Test.Fixtures;
using Neo4jClient.Transactions;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Test.Transactions
{

    public class BoltQueriesInTransactionTests : IClassFixture<CultureInfoSetupFixture>
    {
        private class Foo { }
        #region Helper Methods

        private static IStatementResult GetDbmsComponentsResponse()
        {
            var record = Substitute.For<IRecord>();
            record["name"].Returns("neo4j kernel");
            record["versions"].Returns(new List<object> {"3.1.0"});

            var response = new List<IRecord> {record};

            var statementResult = Substitute.For<IStatementResult>();
            statementResult.GetEnumerator().Returns(response.GetEnumerator());
            return statementResult;
        }

        private static void GetDriverAndSession(out IDriver driver, out ISession session, out Neo4j.Driver.V1.ITransaction transaction)
        {
            var mockNode = Substitute.For<INode>();
            mockNode["Name"].Returns("Value");
            mockNode.Labels.Returns(new List<string>() {"Node"});
            mockNode.Properties.Returns(new Dictionary<string, object>() {{"Name", "Value"}});

            var mockRecord = Substitute.For<IRecord>();
            mockRecord.Keys.Returns(new List<string>(){"Node"});
            mockRecord["Node"].Returns(mockNode);
            mockRecord.Values["Node"].Returns(mockNode);

            var mockStatementResult = Substitute.For<IStatementResult>();
            mockStatementResult.GetEnumerator().Returns(new List<IRecord>(new[]{mockRecord}).GetEnumerator());

            var mockTransaction = Substitute.For<Neo4j.Driver.V1.ITransaction>();
            mockTransaction.Run(Arg.Any<string>(), Arg.Any<IDictionary<string, object>>()).Returns(mockStatementResult);

            var mockSession = Substitute.For<ISession>();
            var dbmsReturn = GetDbmsComponentsResponse();
            mockSession.Run("CALL dbms.components()").Returns(dbmsReturn);
            mockSession.BeginTransaction().Returns(mockTransaction);
            
            var mockDriver = Substitute.For<IDriver>();
            mockDriver.Session().Returns(mockSession);
            mockDriver.Session(Arg.Any<AccessMode>()).Returns(mockSession);
            mockDriver.Session(Arg.Any<AccessMode>(), Arg.Any<IEnumerable<string>>()).Returns(mockSession);
            mockDriver.Session(Arg.Any<IEnumerable<string>>()).Returns(mockSession);
            mockDriver.Uri.Returns(new Uri("bolt://localhost"));

            driver = mockDriver;
            session = mockSession;
            transaction = mockTransaction;
        }

        private static void GetAndConnectGraphClient(out IGraphClient graphClient, out IDriver driver, out ISession session, out Neo4j.Driver.V1.ITransaction transaction)
        {
            GetDriverAndSession(out driver, out session, out transaction);
            var client = new BoltGraphClient(driver);
            client.Connect();

            driver.ClearReceivedCalls();
            session.ClearReceivedCalls();
            graphClient = client;
        }

        #endregion Helper Methods

        public class TransactionGraphClientTests : IClassFixture<CultureInfoSetupFixture>
        {
            [Fact]
            public void SimulateMultipleQueries_AsSingleTransaction()
            {
                ISession session;
                IDriver driver;
                Neo4j.Driver.V1.ITransaction transaction;
                IGraphClient graphClient;

                GetAndConnectGraphClient(out graphClient, out driver, out session, out transaction);

                var query = graphClient.Cypher.Match("(n)").Set("n.Value = 'test'").Query;

                var rawGraphClient = (IRawGraphClient) graphClient;
                rawGraphClient.ExecuteMultipleCypherQueriesInTransaction(new[]{query});

                driver.Received(1).Session((IEnumerable<string>) null);
                session.Received(1).BeginTransaction();
                transaction.Received(1).Success();
            }

            [Fact]
            public void SimpleTransaction_AsTransactionalGc_1Query()
            {
                ISession session;
                IDriver driver;
                Neo4j.Driver.V1.ITransaction transaction;
                IGraphClient graphClient;

                GetAndConnectGraphClient(out graphClient, out driver, out session, out transaction);

                ITransactionalGraphClient txGc = (ITransactionalGraphClient) graphClient;
                using (var tx = txGc.BeginTransaction())
                {
                    txGc.Cypher.Match("(n)").Set("n.Value = 'test'").ExecuteWithoutResults();
                    tx.Commit();
                }

                driver.Received(1).Session((IEnumerable<string>)null);
                session.Received(1).BeginTransaction();
                transaction.Received(1).Success();
            }

            private class MockNode
            {
                public string Name { get; set; }
            }

            [Fact]
            public void SimpleTransaction_RetrieveAndSerializeAnonymousResult()
            {
                ISession session;
                IDriver driver;
                Neo4j.Driver.V1.ITransaction transaction;
                IGraphClient graphClient;

                GetAndConnectGraphClient(out graphClient, out driver, out session, out transaction);

                ITransactionalGraphClient txGc = (ITransactionalGraphClient)graphClient;
                using (var tx = txGc.BeginTransaction())
                {
                    var node = txGc.Cypher.Match("(n:Node)").Return(n => new {Node = n.As<MockNode>()}).Results.SingleOrDefault();

                    node.Node.Name.Should().Be("Value");

                    tx.Commit();
                }

                driver.Received(1).Session((IEnumerable<string>) null);
                session.Received(1).BeginTransaction();
                transaction.Received(1).Success();
            }
        }

        public class TransactionScopeTests : IClassFixture<CultureInfoSetupFixture>
        {
            [Fact]
            public void NestedJoinedTransactionScope()
            {
                ISession session;
                IDriver driver;
                Neo4j.Driver.V1.ITransaction transaction;
                IGraphClient graphClient;

                GetAndConnectGraphClient(out graphClient, out driver, out session, out transaction);
                using (var scope = new TransactionScope())
                {
                    using (var scope2 = new TransactionScope())
                    {
                        graphClient.Cypher
                            .Match("n")
                            .Return(n => n.Count())
                            .ExecuteWithoutResults();

                        // this will not commit
                        scope2.Complete();
                    }

                    // this should generate a request to the known transaction ID
                    graphClient.Cypher
                        .Match("n")
                        .Return(n => n.Count())
                        .ExecuteWithoutResults();
                }

                driver.Received(1).Session(Arg.Any<AccessMode>(),(IEnumerable<string>) null);
                transaction.Received(1).Failure();
                transaction.Received(1).Dispose();
            }

            [Fact]
            public void SimpleTransaction_1Query()
            {
                ISession session;
                IDriver driver;
                Neo4j.Driver.V1.ITransaction transaction;
                IGraphClient graphClient;

                GetAndConnectGraphClient(out graphClient, out driver, out session, out transaction);

                using (var scope = new TransactionScope())
                {
                    graphClient.Cypher.Match("(n)").Set("n.Value = 'test'").ExecuteWithoutResults();
                    scope.Complete();
                }

                driver.Received(1).Session(Arg.Any<AccessMode>(), (IEnumerable<string>)null);
                session.Received(1).BeginTransaction();
                transaction.Received(1).Run(Arg.Any<string>(), Arg.Any<Dictionary<string, object>>());
                transaction.Received(1).Success();
            }



            [Fact]
            public void SimpleTransaction_2Queries()
            {
                ISession session;
                IDriver driver;
                Neo4j.Driver.V1.ITransaction transaction;
                IGraphClient graphClient;

                GetAndConnectGraphClient(out graphClient, out driver, out session, out transaction);

                using (var scope = new TransactionScope())
                {
                    graphClient.Cypher.Match("(n)").Set("n.Value = 'test'").ExecuteWithoutResults();
                    graphClient.Cypher.Match("(n)").Set("n.Value = 'test'").ExecuteWithoutResults();
                    scope.Complete();
                }

                driver.Received(1).Session(Arg.Any<AccessMode>(), (IEnumerable<string>)null);
                session.Received(1).BeginTransaction();
                transaction.Received(1).Success();
                transaction.Received(1).Dispose();
                session.Received(1).Dispose();
            }

            [Fact]
            public void SimpleTransaction_Subsequent_2Queries()
            {
                ISession session;
                IDriver driver;
                Neo4j.Driver.V1.ITransaction transaction;
                IGraphClient graphClient;

                GetAndConnectGraphClient(out graphClient, out driver, out session, out transaction);

                using (var scope = new TransactionScope())
                {
                    graphClient.Cypher.Match("(n)").Set("n.Value = 'test'").ExecuteWithoutResults();
                    graphClient.Cypher.Match("(n)").Set("n.Value = 'test'").ExecuteWithoutResults();
                    scope.Complete();
                }

                using (var scope = new TransactionScope())
                {
                    graphClient.Cypher.Match("(n)").Set("n.Value = 'test'").ExecuteWithoutResults();
                    graphClient.Cypher.Match("(n)").Set("n.Value = 'test'").ExecuteWithoutResults();
                    scope.Complete();
                }

                driver.Received(2).Session(Arg.Any<AccessMode>(), (IEnumerable<string>)null);
                session.Received(2).BeginTransaction();
                transaction.Received(2).Success();
                transaction.Received(2).Dispose();
                session.Received(2).Dispose();
            }

            [Fact]
            public void SimpleTransaction_Subsequent_WithResults_2Queries()
            {
                ISession session;
                IDriver driver;
                Neo4j.Driver.V1.ITransaction transaction;
                IGraphClient graphClient;

                GetAndConnectGraphClient(out graphClient, out driver, out session, out transaction);

                using (var scope = new TransactionScope())
                {
                    graphClient.Cypher.Match("(n)").Set("n.Value = 'test'").ExecuteWithoutResults();
                    var res = graphClient.Cypher.Match("(n)").Return(n => n.As<Foo>()).Results;
                    graphClient.Cypher.Match("(n)").Set("n.Value = 'test'").ExecuteWithoutResults();
                    scope.Complete();
                }

                using (var scope = new TransactionScope())
                {
                    graphClient.Cypher.Match("(n)").Set("n.Value = 'test'").ExecuteWithoutResults();
                    var res = graphClient.Cypher.Match("(n)").Return(n => n.As<Foo>()).Results;
                    graphClient.Cypher.Match("(n)").Set("n.Value = 'test'").ExecuteWithoutResults();
                    scope.Complete();
                }

                driver.Received(2).Session(Arg.Any<AccessMode>(), (IEnumerable<string>)null);
                session.Received(2).BeginTransaction();
                transaction.Received(2).Success();
                transaction.Received(2).Dispose();
                session.Received(2).Dispose();
                driver.Received(0).Dispose();
            }

            [Fact]
            public void SimpleTransaction_1Query_Rollback()
            {
                ISession session;
                IDriver driver;
                Neo4j.Driver.V1.ITransaction transaction;
                IGraphClient graphClient;

                GetAndConnectGraphClient(out graphClient, out driver, out session, out transaction);

                using (var scope = new TransactionScope())
                {
                    graphClient.Cypher.Match("(n)").Set("n.Value = 'test'").ExecuteWithoutResults();
                }

                driver.Received(1).Session(Arg.Any<AccessMode>(), (IEnumerable<string>)null);
                transaction.Received(1).Failure();
            }
        }
    }
}

