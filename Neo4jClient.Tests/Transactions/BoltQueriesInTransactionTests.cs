using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Neo4j.Driver;
using Neo4jClient.Transactions;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.Transactions
{
    public class BoltQueriesInTransactionTests : IClassFixture<CultureInfoSetupFixture>
    {
        private class Foo
        {
        }


        public class TransactionGraphClientTests : IClassFixture<CultureInfoSetupFixture>
        {


            private class MockNode
            {
                public string Name { get; set; }
            }

            [Fact]
            public async Task SimpleTransaction_AsTransactionalGc_1Query()
            {
                BoltClientTestHelper.GetAndConnectGraphClient(out var graphClient, out var driver, out var session, out var transaction);

                var txGc = (ITransactionalGraphClient) graphClient;
                using (var tx = txGc.BeginTransaction())
                {
                    await txGc.Cypher.Match("(n)").Set("n.Value = 'test'").ExecuteWithoutResultsAsync();
                    await tx.CommitAsync();
                }

                driver.Received(1).AsyncSession(Arg.Any<Action<SessionConfigBuilder>>()); 
                await session.Received(1).BeginTransactionAsync();
                await transaction.Received(1).CommitAsync();
            }

            [Fact]
            public async Task SimpleTransaction_AsTransactionalGc_1Query_Moq()
            {
                using (var harness = new BoltTestHarness())
                {
                    var graphClient = await harness.CreateAndConnectBoltGraphClient();

                    var txGc = (ITransactionalGraphClient) graphClient;
                    using (var tx = txGc.BeginTransaction())
                    {
                        var query = txGc.Cypher.Match("(n)").Set("n.Value = 'test'");
                        await query.ExecuteWithoutResultsAsync();
                        await tx.CommitAsync();
                    }

                    harness.MockDriver.Verify(md => md.AsyncSession(It.IsAny<Action<SessionConfigBuilder>>()), Times.Exactly(1));
                }
            }

            [Fact]
            public async Task SimpleTransaction_RetrieveAndSerializeAnonymousResult()
            {
                BoltClientTestHelper.GetAndConnectGraphClient(out var graphClient, out var driver, out var session, out var transaction);

                var txGc = (ITransactionalGraphClient) graphClient;
                using (var tx = txGc.BeginTransaction())
                {
                    var node = (await txGc.Cypher.Match("(n:Node)").Return(n => new {Node = n.As<MockNode>()}).ResultsAsync).SingleOrDefault();
                    node?.Node.Name.Should().Be("Value");
                    await tx.CommitAsync();
                }

                driver.Received(1).AsyncSession(Arg.Any<Action<SessionConfigBuilder>>());
                await session.Received(1).BeginTransactionAsync();
                await transaction.Received(1).CommitAsync();
            }
        }
    }
}