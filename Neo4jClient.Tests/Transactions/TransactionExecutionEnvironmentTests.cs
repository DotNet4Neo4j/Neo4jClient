using System;
using Neo4jClient.Execution;
using Neo4jClient.Test.Fixtures;
using Neo4jClient.Transactions;
using Xunit;

namespace Neo4jClient.Test.Transactions
{
    
    public class TransactionExecutionEnvironmentTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void ResourceManagerIdDefaultValueIsSet()
        {
            var configuration = new ExecutionConfiguration();
            var executionEnvironment = new TransactionExecutionEnvironment(configuration);
            Assert.Equal(configuration.ResourceManagerId, executionEnvironment.ResourceManagerId);
        }

        [Fact]
        public void UserCanSetResourceManagerId()
        {
            var resourceManagerId = Guid.NewGuid();
            var configuration = new ExecutionConfiguration {ResourceManagerId = resourceManagerId};
            var executionEnvironment = new TransactionExecutionEnvironment(configuration);
            Assert.Equal(resourceManagerId, executionEnvironment.ResourceManagerId);
        }
    }
}
