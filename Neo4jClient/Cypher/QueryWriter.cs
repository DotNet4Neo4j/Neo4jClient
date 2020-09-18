using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Neo4j.Driver;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient.Cypher
{
    public class QueryWriter
    {
        readonly StringBuilder queryTextBuilder;
        readonly IDictionary<string, object> queryParameters;
        CypherResultMode resultMode;
        private CypherResultFormat resultFormat;
        private readonly List<Bookmark> bookmarks = new List<Bookmark>();
    
        public QueryWriter(string databaseName = null)
        {
            queryTextBuilder = new StringBuilder();
            queryParameters = new Dictionary<string, object>();
            resultMode = CypherResultMode.Set;
            resultFormat = CypherResultFormat.DependsOnEnvironment;
            DatabaseName = databaseName;
        }

        QueryWriter(
            StringBuilder queryTextBuilder,
            IDictionary<string, object> queryParameters,
            CypherResultMode resultMode,
            CypherResultFormat resultFormat,
            List<Bookmark> bookmarks,
            string identifier)
        {
            this.queryTextBuilder = queryTextBuilder;
            this.queryParameters = queryParameters;
            this.resultMode = resultMode;
            this.resultFormat = resultFormat;
            this.Identifier = identifier;
            this.bookmarks = bookmarks;
        }

        public List<Bookmark> Bookmarks => bookmarks;

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

        //TODO: Do I want this here? Largely I don't care about QW
        public string DatabaseName { get; set; }

        public QueryWriter Clone()
        {
            var clonedQueryTextBuilder = new StringBuilder(queryTextBuilder.ToString());
            var clonedParameters = new Dictionary<string, object>(queryParameters);
            var clonedBookmarks = new List<Bookmark>(bookmarks);
            
            return new QueryWriter(clonedQueryTextBuilder, clonedParameters, resultMode, resultFormat, clonedBookmarks, Identifier)
            {
                MaxExecutionTime = MaxExecutionTime,
                CustomHeaders = CustomHeaders,
                DatabaseName = DatabaseName
            };
        }

        public CypherQuery ToCypherQuery(IContractResolver contractResolver = null, bool isWrite = true, bool includeQueryStats = false)
        {
            var queryText = queryTextBuilder
                .ToString()
                .TrimEnd(Environment.NewLine.ToCharArray());

            return new CypherQuery(
                queryText,
                new Dictionary<string, object>(queryParameters),
                resultMode,
                resultFormat,
                DatabaseName,
                contractResolver,
                MaxExecutionTime,
                CustomHeaders,
                isWrite,
                Bookmarks,
                Identifier,
                includeQueryStats
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
            var paramName = $"p{queryParameters.Count}";
            queryParameters.Add(paramName, paramValue);
            return $"${paramName}";
        }
    }
}
