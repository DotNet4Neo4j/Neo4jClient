namespace Neo4jClient.Cypher
{
    public static class ReturnStep
    {

        //ToDo The Return method should return a cypher table result with generic overloads on column types.
        public static ICypherQuery Return(this ICypherQuery query, string column1Name)
        {
            var returnText = string.Format(" return {0}", column1Name);
            query = query.AddBlock(returnText, query.QueryParameters);
            return query;
        }
    }
}
