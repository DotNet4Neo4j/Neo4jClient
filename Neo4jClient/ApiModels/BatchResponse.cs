using System.Collections.Generic;
using System.Linq;

namespace Neo4jClient.ApiModels
{
    class BatchResponse : List<BatchStepResult>
    {
        public BatchStepResult this[BatchStep step]
        {
            get { return this.SingleOrDefault(r => r.Id == step.Id); }
        }
    }
}
