﻿using System;
using System.Collections.Generic;
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

        public QueryWriter()
        {
            queryTextBuilder = new StringBuilder();
            queryParameters = new Dictionary<string, object>();
            resultMode = CypherResultMode.Set;
        }

        QueryWriter(
            StringBuilder queryTextBuilder,
            IDictionary<string, object> queryParameters,
            CypherResultMode resultMode)
        {
            this.queryTextBuilder = queryTextBuilder;
            this.queryParameters = queryParameters;
            this.resultMode = resultMode;
        }

        public CypherResultMode ResultMode
        {
            get { return resultMode; }
            set { resultMode = value; }
        }

        public QueryWriter Clone()
        {
            var clonedQueryTextBuilder = new StringBuilder(queryTextBuilder.ToString());
            var clonedParameters = new Dictionary<string, object>(queryParameters);
            return new QueryWriter(clonedQueryTextBuilder, clonedParameters, resultMode);
        }

        public CypherQuery ToCypherQuery(IContractResolver contractResolver = null)
        {
            var queryText = queryTextBuilder
                .ToString()
                .TrimEnd(Environment.NewLine.ToCharArray());

            return new CypherQuery(
                queryText,
                new Dictionary<string, object>(queryParameters),
                resultMode, 
                contractResolver);
        }

        public string CreateParameter(object paramValue)
        {
            var paramName = string.Format("p{0}", queryParameters.Count);
            queryParameters.Add(paramName, paramValue);
            return "{" + paramName + "}";
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

        string CreateParameterAndReturnPlaceholder(object paramValue)
        {
            var paramName = string.Format("p{0}", queryParameters.Count);
            queryParameters.Add(paramName, paramValue);
            return "{" + paramName + "}";
        }
    }
}
