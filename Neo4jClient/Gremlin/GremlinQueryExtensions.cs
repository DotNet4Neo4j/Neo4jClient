using System.Collections.Generic;

namespace Neo4jClient.Gremlin
{
    static class GremlinQueryExtensions
    {
        public static IGremlinQuery AddBlock(this IGremlinQuery baseQuery, string text, params object[] parameters)
        {
            var paramsDictionary = new Dictionary<string, object>(baseQuery.QueryParameters);
            var nextParameterIndex = baseQuery.QueryParameters.Count;
            var paramNames = new List<string>();
            foreach (var paramValue in parameters)
            {
                var paramName = string.Format("p{0}", nextParameterIndex);
                paramNames.Add(paramName);
                paramsDictionary.Add(paramName, paramValue);
            }

            var textWithParamNames = string.Format(text, paramNames.ToArray());

            return new GremlinQuery(baseQuery.Client, baseQuery.QueryText + textWithParamNames, paramsDictionary);
        }
    }
}
