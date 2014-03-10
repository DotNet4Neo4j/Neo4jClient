namespace Neo4jClient.Execution
{
    internal interface IRequestWithPendingContentBuilder
    {
        IResponseBuilder WithContent(string content);
        IResponseBuilder WithJsonContent(string jsonContent);
    }
}