using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient.Cypher
{
    public class QueryWriter
    {
        readonly StringBuilder queryTextBuilder;
        readonly IDictionary<string, object> queryParameters;
        CypherResultMode resultMode;
        private CypherResultFormat resultFormat;
        private readonly List<string> bookmarks = new List<string>();
    
        public QueryWriter()
        {
            queryTextBuilder = new StringBuilder();
            queryParameters = new Dictionary<string, object>();
            resultMode = CypherResultMode.Set;
            resultFormat = CypherResultFormat.DependsOnEnvironment;
        }

        QueryWriter(
            StringBuilder queryTextBuilder,
            IDictionary<string, object> queryParameters,
            CypherResultMode resultMode,
            CypherResultFormat resultFormat,
            List<string> bookmarks,
            string identifier)
        {
            this.queryTextBuilder = queryTextBuilder;
            this.queryParameters = queryParameters;
            this.resultMode = resultMode;
            this.resultFormat = resultFormat;
            this.Identifier = identifier;
            this.bookmarks = bookmarks;
        }

        public List<string> Bookmarks => bookmarks;

        public CypherResultMode ResultMode
        {
            get => resultMode;
            set => resultMode = value;
        }

        public CypherResultFormat ResultFormat
        {
            get => resultFormat;
            set => resultFormat = value;
        }

        public int? MaxExecutionTime { get; set; }

        public NameValueCollection CustomHeaders { get; set; }
        public string Identifier { get; set; }

        public QueryWriter Clone()
        {
            var clonedQueryTextBuilder = new StringBuilder(queryTextBuilder.ToString());
            var clonedParameters = new Dictionary<string, object>(queryParameters);
            var clonedBookmarks = new List<string>(bookmarks);
            
            return new QueryWriter(clonedQueryTextBuilder, clonedParameters, resultMode, resultFormat, clonedBookmarks, Identifier)
            {
                MaxExecutionTime = MaxExecutionTime,
                CustomHeaders = CustomHeaders
            };
        }

        public CypherQuery ToCypherQuery(IContractResolver contractResolver = null, bool isWrite = true)
        {
            var queryText = queryTextBuilder
                .ToString()
                .TrimEnd(Environment.NewLine.ToCharArray());

            return new CypherQuery(
                queryText,
                new Dictionary<string, object>(queryParameters),
                resultMode,
                resultFormat,
				contractResolver,
                MaxExecutionTime,
                CustomHeaders,
                isWrite,
                Bookmarks,
                Identifier
                );
        }

        public string CreateParameter(object paramValue)
        {
            var paramName = $"p{queryParameters.Count}";
            queryParameters.Add(paramName, paramValue);
            return $"${paramName}";
        }

        public void CreateParameter(string key, object value)
        {
            queryParameters.Add(key, value);
        }

        public void CreateParameters(IDictionary<string,object> parameters)
        {
            foreach(var parameter in parameters)
                queryParameters.Add(parameter.Key, parameter.Value);
        }

        public bool ContainsParameterWithKey(string key)
        {
            return queryParameters.ContainsKey(key);
        }

        public QueryWriter AppendClause(string clause, params object[] paramValues)
        {
            if (paramValues.Any())
            {
                var paramPlaceholders = paramValues
                    .Select(CreateParameterAndReturnPlaceholder)
                    .Cast<object>()
                    .ToArray();
                clause = string.Format(clause, paramPlaceholders);
            }

            // Only needed while migrating off CypherQueryBuilder
            if (queryTextBuilder.Length > 0 &&
                !queryTextBuilder.ToString().EndsWith(Environment.NewLine))
            {
                queryTextBuilder.AppendLine();
            }

            queryTextBuilder.AppendLine(clause);

            return this;
        }
        public QueryWriter AppendToClause(string appendedData, params object[] paramValues)
        {
            if (paramValues.Any())
            {
                var paramPlaceholders = paramValues
                    .Select(CreateParameterAndReturnPlaceholder)
                    .Cast<object>()
                    .ToArray();
                appendedData = string.Format(appendedData, paramPlaceholders);
            }

            if (queryTextBuilder.Length > 0 &&
                queryTextBuilder.ToString().EndsWith(Environment.NewLine))
            {
                queryTextBuilder.Remove(queryTextBuilder.Length - Environment.NewLine.Length, Environment.NewLine.Length);
            }

            queryTextBuilder.Append(appendedData);

            return this;
        }

        string CreateParameterAndReturnPlaceholder(object paramValue)
        {
            var paramName = string.Format("p{0}", queryParameters.Count);
            queryParameters.Add(paramName, paramValue);
            return $"${paramName}";
        }
    }
}
