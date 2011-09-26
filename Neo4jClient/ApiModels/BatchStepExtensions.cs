using System.Collections.Generic;
using RestSharp;

namespace Neo4jClient.ApiModels
{
    static class BatchStepExtensions
    {
        public static BatchStep Add(this IList<BatchStep> list, Method method, string to, object body)
        {
            var id = list.Count;
            var step = new BatchStep {Method = method, To = to, Body = body, Id = id};
            list.Add(step);
            return step;
        }
    }
}
