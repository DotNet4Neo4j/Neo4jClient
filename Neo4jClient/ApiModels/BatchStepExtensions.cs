using System.Collections.Generic;
using System.Net.Http;

namespace Neo4jClient.ApiModels
{
    static class BatchStepExtensions
    {
        public static BatchStep Add(this IList<BatchStep> list, HttpMethod method, string to, object body)
        {
            var id = list.Count;
            var step = new BatchStep {Method = method, To = to, Body = body, Id = id};
            list.Add(step);
            return step;
        }
    }
}
