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

            var textWithParamNames = parameters.Any()
                ? string.Format(text, paramNames.ToArray())
                : text;

            return new GremlinQuery(baseQuery.Client, baseQuery.QueryText + textWithParamNames, paramsDictionary,baseQuery.QueryDeclarations);
        }

        public static IGremlinQuery AddCopySplitBlock(this IGremlinQuery baseQuery, string text, IGremlinQuery[] queries)
        {
            var declarations = new List<string>();
            var rootQuery = baseQuery.QueryText;
            var paramsDictionary = new Dictionary<string, object>(baseQuery.QueryParameters);
            var paramNames = new List<string>();
            var inlineQueries = new List<string>();
            var nextParameterIndex = baseQuery.QueryParameters.Count;

            foreach (var query in queries)
            {
                var nextParamaterIndex2 = nextParameterIndex;
                var query2 = query;

                declarations.AddRange(query.QueryDeclarations);
                var modifiedQueryText = ParameterReferenceRegex.Replace(
                    query.QueryText,
                    m =>
                    {
                        var parameterIndex = nextParamaterIndex2;
                        nextParamaterIndex2++;

                        var oldParamKey = m.Value;
                        var newParamKey = string.Format("p{0}", parameterIndex);

                        paramNames.Add(newParamKey);
                        paramsDictionary.Add(newParamKey, query2.QueryParameters[oldParamKey]);

                        return newParamKey;
                    });

                foreach (var declareStatement in query.QueryDeclarations)
                {
                    rootQuery = declareStatement + baseQuery.QueryText;
                    modifiedQueryText = modifiedQueryText.Replace(declareStatement, string.Empty);
                }

                inlineQueries.Add(modifiedQueryText);

                nextParameterIndex = nextParamaterIndex2;
            }

            var splitBlockQueries = string.Format(text, inlineQueries.ToArray());

            return new GremlinQuery(baseQuery.Client, rootQuery + splitBlockQueries, paramsDictionary, declarations);
        }

        public static IGremlinQuery AddIfThenElseBlock(this IGremlinQuery baseQuery, string ifthenelse, IGremlinQuery ifExpression, IGremlinQuery ifThen, IGremlinQuery ifElse)
        {
            throw new NotImplementedException();
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
