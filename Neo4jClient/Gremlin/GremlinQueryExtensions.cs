using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo4jClient.Gremlin
{
    static class GremlinQueryExtensions
    {
        public static IGremlinQuery PrependVariablesToBlock(this IGremlinQuery baseQuery, IGremlinQuery query)
        {
            var declarations = query.QueryDeclarations.Aggregate(string.Empty, (current, declaration) => current + declaration);
            return new GremlinQuery(baseQuery.Client, declarations + baseQuery.QueryText, baseQuery.QueryParameters, query.QueryDeclarations);
        }

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
                nextParameterIndex++;
            }

            var textWithParamNames = parameters.Length > 0 ? string.Format(text, paramNames.ToArray()) : text;

            return new GremlinQuery(baseQuery.Client, baseQuery.QueryText + textWithParamNames, paramsDictionary,baseQuery.QueryDeclarations);
        }

        public static IGremlinQuery AddCopySplitBlock(this IGremlinQuery baseQuery, string text, IGremlinQuery[] queries)
        {
            var declarations = new List<string>();
            var rootQuery = baseQuery.QueryText;
            var paramsDictionary = new Dictionary<string, object>(baseQuery.QueryParameters);
            var nextParameterIndex = baseQuery.QueryParameters.Count;
            var paramNames = new List<string>();
            var inlineQueries = new List<string>();

            foreach (var query in queries)
            {
                declarations.AddRange(query.QueryDeclarations);
                var modifiedQueryText = query.QueryText;
                foreach (var param in query.QueryParameters)
                {
                    var oldParamKey = param.Key;
                    var newParamKey = string.Format("p{0}", nextParameterIndex);
                    paramNames.Add(newParamKey);
                    paramsDictionary.Add(newParamKey, param.Value);
                    nextParameterIndex++;
                    modifiedQueryText = modifiedQueryText.Replace(oldParamKey, newParamKey);
                }

                foreach (var declareStatement in query.QueryDeclarations)
                {
                    rootQuery = declareStatement + baseQuery.QueryText;
                    modifiedQueryText = modifiedQueryText.Replace(declareStatement, string.Empty);
                }

                inlineQueries.Add(modifiedQueryText);
            }

            var splitBlockQueries = string.Format(text, inlineQueries.ToArray());
            var textWithParamNames = string.Format(splitBlockQueries, paramNames.ToArray());


            return new GremlinQuery(baseQuery.Client, rootQuery + textWithParamNames, paramsDictionary, declarations);
        }

        public static IGremlinQuery AddFilterBlock(this IGremlinQuery baseQuery, string text, IEnumerable<Filter> filters, StringComparison comparison)
        {
            var formattedFilter = FilterFormatters.FormatGremlinFilter(filters, comparison, baseQuery);

            var newQueryText = baseQuery.QueryText + text + formattedFilter.FilterText;

            var newParams = new Dictionary<string, object>(baseQuery.QueryParameters);
            foreach (var key in formattedFilter.FilterParameters.Keys)
                newParams.Add(key, formattedFilter.FilterParameters[key]);

            return new GremlinQuery(baseQuery.Client, newQueryText, newParams, baseQuery.QueryDeclarations);
        }

        public static string ToDebugQueryText(this IGremlinQuery query)
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
