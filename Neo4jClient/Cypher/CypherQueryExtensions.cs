using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo4jClient.Cypher
{
    static class CypherQueryExtensions
    {
        public static ICypherQuery PrependVariablesToBlock(this ICypherQuery baseQuery, ICypherQuery query)
        {
            var declarations = query
                .QueryDeclarations
                .Aggregate(string.Empty, (current, declaration) => !baseQuery.QueryText.Contains(declaration) ? declaration + current : current);
            return new CypherQuery(baseQuery.Client, declarations + baseQuery.QueryText, baseQuery.QueryParameters, query.QueryDeclarations);
        }

        public static ICypherQuery BuildStartById(this ICypherQuery baseQuery, string text, params int[] parameters)
        {
            var paramsDictionary = new Dictionary<string, object>();
            var nextParameterIndex = 0;
            var paramNames = new List<string>();
            foreach (var paramValue in parameters)
            {
                var paramName = string.Format("p{0}", nextParameterIndex);
                paramNames.Add(paramName);
                paramsDictionary.Add(paramName, paramValue);
                nextParameterIndex++;
            }

            var textWithParamNames = parameters.Any()
                ? string.Format(text, paramNames.ToArray())
                : text;

            return new CypherQuery(baseQuery.Client, textWithParamNames, paramsDictionary, baseQuery.QueryDeclarations);
        }

        public static ICypherQuery AddBlock(this ICypherQuery baseQuery, string text, params object[] parameters)
        {
            var paramsDictionary = new Dictionary<string, object>(baseQuery.QueryParameters);
            var nextParameterIndex = baseQuery.QueryParameters.Count;
            var paramNames = new List<string>();
            foreach (var paramValue in parameters)
            {
                var paramName = string.Format("p{0}", nextParameterIndex);
                paramNames.Add(paramName);
                paramsDictionary.Add(paramName, paramValue);
                nextParameterIndex++;
            }

            var textWithParamNames = parameters.Any()
                ? string.Format(text, paramNames.ToArray())
                : text;

            return new CypherQuery(baseQuery.Client, baseQuery.QueryText + textWithParamNames, paramsDictionary,baseQuery.QueryDeclarations);
        }

        public static string ToDebugQueryText(this ICypherQuery query)
        {
            var text = query.QueryText;
            if (query.QueryParameters == null) return text;
            foreach (var key in query.QueryParameters.Keys.Reverse())
            {
                text = text.Replace(key, string.Format("'{0}'", query.QueryParameters[key]));
            }
            return text;
        }
    }
}
