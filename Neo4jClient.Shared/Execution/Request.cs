using System.Collections.Specialized;

namespace Neo4jClient.Execution
{
    internal static class Request
    {
        public static IRequestTypeBuilder With(ExecutionConfiguration configuration, NameValueCollection customerHeaders = null, int? maxExecutionTime = null)
        {
            return new RequestTypeBuilder(configuration, customerHeaders, maxExecutionTime);
        }
    }
}
