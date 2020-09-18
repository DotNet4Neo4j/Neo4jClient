// using System;
//
// namespace Neo4jClient.Execution
// {
//     internal class NodeIndexExecutionPolicy : RestExecutionPolicy
//     {
//         public NodeIndexExecutionPolicy(IGraphClient client)
//             : base(client)
//         {
//         }
//
//         public override Uri BaseEndpoint(string database = null)
//         {
//             return Replace(Client.NodeIndexEndpoint, database);
//         }
//     }
// }