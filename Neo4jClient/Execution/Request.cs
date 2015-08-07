namespace Neo4jClient.Execution
{
    internal static class Request
    {
        public static IRequestTypeBuilder With(ExecutionConfiguration configuration)
        {
            return new RequestTypeBuilder(configuration);
        }
    }
}
