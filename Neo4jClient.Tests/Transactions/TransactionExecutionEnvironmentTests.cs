using System;
using Neo4jClient.Execution;
using Neo4jClient.Transactions;
using NUnit.Framework;

namespace Neo4jClient.Test.Transactions
{
    [TestFixture]
    public class TransactionExecutionEnvironmentTests
    {
        [Test]
        public void ResourceManagerIdDefaultValueIsSet()
        {
            var configuration = new ExecutionConfiguration();
            var executionEnvironment = new TransactionExecutionEnvironment(configuration);
            Assert.AreEqual(configuration.ResourceManagerId, executionEnvironment.ResourceManagerId);
        }

        [Test]
        public void UserCanSetResourceManagerId()
        {
            var resourceManagerId = Guid.NewGuid();
            var configuration = new ExecutionConfiguration {ResourceManagerId = resourceManagerId};
            var executionEnvironment = new TransactionExecutionEnvironment(configuration);
            Assert.AreEqual(resourceManagerId, executionEnvironment.ResourceManagerId);
        }
    }
}
