using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo4jClient.Gremlin
{
    static class GremlinQueryExtensions
    {
        public static IGremlinQuery PrepentVariableToBlock(this IGremlinQuery baseQuery, string variable)
        {
            return new GremlinQuery(baseQuery.Client, variable + baseQuery.QueryText, baseQuery.QueryParameters);
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

            string textWithParamNames = parameters.Length > 0 ? string.Format(text, paramNames.ToArray()) : text;

            return new GremlinQuery(baseQuery.Client, baseQuery.QueryText + textWithParamNames, paramsDictionary);
        }

        public static IGremlinQuery AddCopySplitBlock(this IGremlinQuery baseQuery, string text, IGremlinQuery[] queries)
        {
            var paramsDictionary = new Dictionary<string, object>(baseQuery.QueryParameters);
            var nextParameterIndex = baseQuery.QueryParameters.Count;
            var paramNames = new List<string>();
            var inlineQueries = new List<string>();

            foreach (var query in queries)
            {
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
                    inlineQueries.Add(modifiedQueryText);
            }

            var splitBlockQueries = string.Format(text, inlineQueries.ToArray());
            var textWithParamNames = string.Format(splitBlockQueries, paramNames.ToArray());

            return new GremlinQuery(baseQuery.Client, baseQuery.QueryText + textWithParamNames, paramsDictionary);
        }

        public static IGremlinQuery AddFilterBlock(this IGremlinQuery baseQuery, string text, IEnumerable<Filter> filters, StringComparison comparison)
        {
            var formattedFilter = FilterFormatters.FormatGremlinFilter(filters, comparison, baseQuery);

            var newQueryText = baseQuery.QueryText + text + formattedFilter.FilterText;

            var newParams = new Dictionary<string, object>(baseQuery.QueryParameters);
            foreach (var key in formattedFilter.FilterParameters.Keys)
                newParams.Add(key, formattedFilter.FilterParameters[key]);

            return new GremlinQuery(baseQuery.Client, newQueryText, newParams);
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
