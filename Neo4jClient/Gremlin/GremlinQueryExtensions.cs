using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo4jClient.Gremlin
{
    static class GremlinQueryExtensions
    {
        public static IGremlinQuery PrependToBlock(this IGremlinQuery baseQuery, string text, params object[] parameters)
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

            var textWithParamNames = string.Format(text, paramNames.ToArray());

            return new GremlinQuery(baseQuery.Client, textWithParamNames + baseQuery.QueryText, paramsDictionary);
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

            var textWithParamNames = string.Format(text, paramNames.ToArray());

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
