using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Neo4jClient.Gremlin
{
    static class GremlinQueryExtensions
    {
        static readonly Regex ParameterReferenceRegex = new Regex(@"(?<=\W)p\d+(?!\w)");

        public static IGremlinQuery PrependVariablesToBlock(this IGremlinQuery baseQuery, IGremlinQuery query)
        {
            var declarations = query
                .QueryDeclarations
                .Aggregate(string.Empty, (current, declaration) => !baseQuery.QueryText.Contains(declaration) ? declaration + current : current);
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

            var textWithParamNames = parameters.Any()
                ? string.Format(text, paramNames.ToArray())
                : text;

            return new GremlinQuery(baseQuery.Client, baseQuery.QueryText + textWithParamNames, paramsDictionary,baseQuery.QueryDeclarations);
        }

        public static IGremlinQuery AddCopySplitBlock(this IGremlinQuery baseQuery, string text, IGremlinQuery[] queries)
        {
            var declarations = new List<string>();
            var rootQuery = baseQuery.QueryText;
            var inlineQueries = new List<string>();

            var paramsDictionary = new Dictionary<string, object>(baseQuery.QueryParameters);
            var nextParameterIndex = baseQuery.QueryParameters.Count;

            foreach (var query in queries)
            {
                var modifiedQueryText = RebuildParametersAndDeclarations(query, paramsDictionary, declarations, ref nextParameterIndex, ref rootQuery);
                inlineQueries.Add(modifiedQueryText);
            }

            var splitBlockQueries = string.Format(text, inlineQueries.ToArray());

            return new GremlinQuery(baseQuery.Client, rootQuery + splitBlockQueries, paramsDictionary, declarations);
        }

        public static IGremlinQuery AddIfThenElseBlock(this IGremlinQuery baseQuery, string ifThenElseText, IGremlinQuery ifExpression, IGremlinQuery ifThen, IGremlinQuery ifElse)
        {
            var declarations = new List<string>();
            var rootQuery = baseQuery.QueryText;
            var paramsDictionary = new Dictionary<string, object>(baseQuery.QueryParameters);
            var nextParameterIndex = baseQuery.QueryParameters.Count;

            var modifiedQueryTextifExpression = RebuildParametersAndDeclarations(ifExpression, paramsDictionary, declarations, ref nextParameterIndex, ref rootQuery);
            var modifiedQueryTextifThen = RebuildParametersAndDeclarations(ifThen, paramsDictionary, declarations, ref nextParameterIndex, ref rootQuery);
            var modifiedQueryTextifElse = RebuildParametersAndDeclarations(ifElse, paramsDictionary, declarations, ref nextParameterIndex, ref rootQuery);

            var newQueryText = string.Format(ifThenElseText, modifiedQueryTextifExpression, modifiedQueryTextifThen, modifiedQueryTextifElse);

            return new GremlinQuery(baseQuery.Client, rootQuery + newQueryText, paramsDictionary, declarations);
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

        static string RebuildParametersAndDeclarations(IGremlinQuery query, Dictionary<string, object> paramsDictionary,
            List<string> declarations, ref int nextParamaterIndex, ref string rootQuery)
        {
            var updatedIndex = nextParamaterIndex;
            var paramNames = new List<string>();

            declarations.AddRange(query.QueryDeclarations);
            var modifiedQueryText = ParameterReferenceRegex.Replace(
                query.QueryText,
                m =>
                {
                    var parameterIndex = updatedIndex;
                    updatedIndex++;

                    var oldParamKey = m.Value;
                    var newParamKey = string.Format("p{0}", parameterIndex);

                    paramNames.Add(newParamKey);
                    paramsDictionary.Add(newParamKey, query.QueryParameters[oldParamKey]);

                    return newParamKey;
                });

            nextParamaterIndex = updatedIndex;

            foreach (var declareStatement in query.QueryDeclarations)
            {
                rootQuery = declareStatement + rootQuery;
                modifiedQueryText = modifiedQueryText.Replace(declareStatement, string.Empty);
            }
            return modifiedQueryText;
        }
    }
}
