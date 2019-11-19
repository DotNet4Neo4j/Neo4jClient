namespace Neo4jClient.Cypher
{
    /// <summary>
    /// Exposes low level return api to allow more control when developing libraries on top of Neo4jClient
    /// </summary>
    public interface ICypherFluentQueryAdvanced
    {
        ICypherFluentQuery<TResult> Return<TResult>(ReturnExpression returnExpression);

        ICypherFluentQuery<TResult> ReturnDistinct<TResult>(ReturnExpression returnExpression);

        ICypherFluentQuery<TResult> SetClient<TResult>(IGraphClient graphClient);
        ICypherFluentQuery SetClient(IGraphClient graphClient);
    }
}
