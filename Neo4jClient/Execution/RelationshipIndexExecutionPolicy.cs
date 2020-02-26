// using System;
//
// namespace Neo4jClient.Execution
// {
//     internal class RelationshipIndexExecutionPolicy : RestExecutionPolicy
//     {
//         public RelationshipIndexExecutionPolicy(IGraphClient client)
//             : base(client)
//         {
//         }
//
//         public override Uri BaseEndpoint(string database = null)
//         {
//             return Replace(Client.RelationshipIndexEndpoint, database);
//         }
//     }
// }