using Neo4jClient;
using Neo4jClient.Cypher;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;

namespace TestNetStandardTransactions
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new BoltGraphClient(new Uri("bolt://localhost:7687"), "test", "test");
            client.Connect();

            var tags = client.Cypher.Match("(tag:Tag)")
                .Return<string>(() => Return.As<string>("tag.name"))
                .Results;

            Console.WriteLine($"Tags {string.Join(',',tags)}");

            using(var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await client.Cypher.Create("(n:Test {Value:'Test'})").ExecuteWithoutResultsAsync();
                await client.Cypher.Create("(n:Test {Value:'Test'})").ExecuteWithoutResultsAsync();
                scope.Complete();
            }

            Console.ReadKey();
        }
    }
}
