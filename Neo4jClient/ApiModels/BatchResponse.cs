using System.Collections.Generic;
using System.Linq;

namespace Neo4jClient.ApiModels
{
    internal class BatchResponse : List<BatchStepResult>
    {
        public BatchStepResult this[BatchStep step]
        {
            get { return this.Where(r => r.Id == step.Id).SingleOrDefault(); }
        }
    }
}
