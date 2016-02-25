using System;
using System.Collections.Generic;
using System.Linq;
using Neo4jClient.ApiModels.Cypher;
using Neo4jClient.Cypher;
using Neo4jClient.Transactions;

namespace Neo4jClient.Execution
{
    /// <summary>
    /// Describes the behavior for a cypher execution.
    /// </summary>
    internal partial class CypherExecutionPolicy : GraphClientBasedExecutionPolicy
    {

        private INeo4jTransaction GetTransactionInScope()
        {
            return null;
        }

        public override Uri BaseEndpoint
        {
            get
            {
                if(InTransaction)
                    throw new NotImplementedException("Not implemented in the PCL version at present.");

                    return Client.CypherEndpoint;
            }
        }

        public override TransactionExecutionPolicy TransactionExecutionPolicy
        {
            get { return TransactionExecutionPolicy.Allowed; }
        }

     
    }
}
