using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neo4jClient.Execution
{
    internal class RestExecutionPolicy : GraphClientBasedExecutionPolicy
    {
        public RestExecutionPolicy(IGraphClient client) : base(client)
        {
        }

        public override TransactionExecutionPolicy TransactionExecutionPolicy
        {
            get { return TransactionExecutionPolicy.Denied; }
        }

        public override void AfterExecution(IDictionary<string, object> executionMetadata, object executionContext)
        {
        }

        public override Uri BaseEndpoint
        {
            get { return Client.RootEndpoint; }
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
