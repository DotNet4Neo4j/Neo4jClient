using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neo4jClient.Execution
{
    //TODO : Remove this
    internal class RestExecutionPolicy : GraphClientBasedExecutionPolicy
    {


        public RestExecutionPolicy(IGraphClient client) : base(client)
        {
        }

        public override TransactionExecutionPolicy TransactionExecutionPolicy => TransactionExecutionPolicy.Denied;

        public override void AfterExecution(IDictionary<string, object> executionMetadata, object executionContext)
        {
        }

        public override Uri BaseEndpoint(string database = null, bool autoCommit = false)
        {
            return Replace(Client.RootEndpoint, database); 
        }

        public override Uri AddPath(Uri startUri, object startReference)
        {
            if (startReference == null)
            {
                return startUri;
            }

            if (startReference is NodeReference)
            {
                return AddPath(startUri, startReference as NodeReference);
            }

            if (startReference is RelationshipReference)
            {
                return AddPath(startUri, startReference as RelationshipReference);
            }

            throw new NotImplementedException("Unknown startReference parameter for REST policy");
        }

        public override string Database { get; set; }

        private Uri AddPath(Uri startUri, NodeReference node)
        {
            return startUri.AddPath("node").AddPath(node.Id.ToString());
        }

        private Uri AddPath(Uri startUri, RelationshipReference relationship)
        {
            return startUri.AddPath("relationship").AddPath(relationship.Id.ToString());
        }
    }
}
