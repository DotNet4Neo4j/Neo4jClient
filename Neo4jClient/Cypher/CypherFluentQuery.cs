using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json.Serialization;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Threading.Tasks;

namespace Neo4jClient.Cypher
{
    [DebuggerDisplay("{Query.DebugQueryText}")]
    public partial class CypherFluentQuery :
        IOrderedCypherFluentQuery,
        IAttachedReference
    {
        private readonly Version minimumCypherParserVersion = new Version(1, 9);
        internal readonly IRawGraphClient Client;
        protected readonly QueryWriter QueryWriter;
        protected readonly bool CamelCaseProperties;
        protected bool IncludeQueryStats { get; private set; }
        internal bool IsWrite { get; private set; }

        public string Database
        {
            get => QueryWriter.DatabaseName;
            set => QueryWriter.DatabaseName = value;
        }

        public ICypherFluentQuery WithDatabase(string databaseName)
        {
            if(Client.InTransaction)
                throw new InvalidOperationException("This query is in a Transaction, you can't set the database for individual queries within a Transaction.");

            databaseName = databaseName?.ToLowerInvariant();
            Database = databaseName;
            QueryWriter.DatabaseName = databaseName;
            return this;
        }

        public ICypherFluentQuery Read
        {
            get
            {
                IsWrite = false;
                return this;
            }
        }

        public ICypherFluentQuery WithQueryStats
        {
            get
            {
                IncludeQueryStats = true;
                return this;
            }
        }

        public ICypherFluentQuery Write
        {
            get
            {
                IsWrite = true;
                return this;
            }
        }

        public CypherFluentQuery(IGraphClient client, bool isWrite = true, bool includeQueryStats = false)
            : this(client, new QueryWriter(client.DefaultDatabase), isWrite, includeQueryStats)
        {
            IsWrite = isWrite;
        }

        internal CypherFluentQuery(IGraphClient client, QueryWriter queryWriter, bool isWrite = true, bool includeQueryStats = false)
        {
            Client = client as IRawGraphClient ?? throw new ArgumentException("The supplied graph client also needs to implement IRawGraphClient", nameof(client));
            QueryWriter = queryWriter;
            CamelCaseProperties = Client.JsonContractResolver is CamelCasePropertyNamesContractResolver;
            Advanced = new CypherFluentQueryAdvanced(Client, QueryWriter, isWrite, includeQueryStats);
            IsWrite = isWrite;
            IncludeQueryStats = includeQueryStats;
        }

        private IOrderedCypherFluentQuery MutateOrdered(Action<QueryWriter> callback)
        {
            var newWriter = QueryWriter.Clone();
            callback(newWriter);
            return new CypherFluentQuery(Client, newWriter, IsWrite, IncludeQueryStats);
        }

        protected IOrderedCypherFluentQuery<TResult> MutateOrdered<TResult>(Action<QueryWriter> callback)
        {
            var newWriter = QueryWriter.Clone();
            callback(newWriter);
            return new CypherFluentQuery<TResult>(Client, newWriter, IsWrite, IncludeQueryStats);
        }

        private ICypherFluentQuery Mutate(Action<QueryWriter> callback)
        {
            var newWriter = QueryWriter.Clone();
            callback(newWriter);
            return new CypherFluentQuery(Client, newWriter, IsWrite, IncludeQueryStats);
        }

        protected ICypherFluentQuery<TResult> Mutate<TResult>(Action<QueryWriter> callback)
        {
            var newWriter = QueryWriter.Clone();
            callback(newWriter);
            return new CypherFluentQuery<TResult>(Client, newWriter, IsWrite, IncludeQueryStats);
        }

        public ICypherFluentQuery WithParam(string key, object value)
        {
            if (QueryWriter.ContainsParameterWithKey(key))
                throw new ArgumentException("A parameter with the given key is already defined in the query.", nameof(key));
            return Mutate(w => w.CreateParameter(key, value));
        }

        public ICypherFluentQuery WithParams(IDictionary<string, object> parameters)
        {
            if (parameters == null || parameters.Count == 0) return this;

            if (parameters.Keys.Any(key => QueryWriter.ContainsParameterWithKey(key)))
                throw new ArgumentException("A parameter with the given key is already defined in the query.", nameof(parameters));

            return Mutate(w => w.CreateParameters(parameters));
        }

        public ICypherFluentQuery WithParams(object parameters)
        {
            if (parameters == null) return this;
            var keyValuePairs = new Dictionary<string, object>();
            foreach (var propertyDescriptor in (parameters.GetType().GetTypeInfo().DeclaredProperties))
            {
                var key = propertyDescriptor.Name;
                var value = propertyDescriptor.GetValue(parameters);
                keyValuePairs.Add(key, value);
            }
            return WithParams(keyValuePairs);
        }

        /// <inheritdoc cref="ICypherFluentQuery.Use"/>
        public ICypherFluentQuery Use(string database)
        {
            return Mutate(w => w.AppendClause($"USE {database}"));
        }

        public ICypherFluentQuery Match(params string[] matchText)
        {
            return Mutate(w =>
                w.AppendClause($"MATCH {string.Join(", ", matchText)}"));
        }

        public ICypherFluentQuery UsingIndex(string index)
        {
            if (string.IsNullOrEmpty(index))
            {
                throw new ArgumentException("Index description is required");
            }

            return Mutate(w =>
                w.AppendClause($"USING INDEX {index}"));
        }

        public ICypherFluentQuery OptionalMatch(string pattern)
        {
            return Mutate(w =>
                w.AppendClause($"OPTIONAL MATCH {pattern}"));
        }

        public ICypherFluentQuery Merge(string mergeText)
        {
            return Mutate(w => w.AppendClause($"MERGE {mergeText}"));
        }

        public ICypherFluentQuery OnCreate()
        {
            return Mutate(w => w.AppendClause("ON CREATE"));
        }

        public ICypherFluentQuery OnMatch()
        {
            return Mutate(w => w.AppendClause("ON MATCH"));
        }

        
        public ICypherFluentQuery Call(string storedProcedureText)
        {
            if (!Client.CypherCapabilities.SupportsStoredProcedures)
                throw new InvalidOperationException("CALL not supported in Neo4j versions older than 3.0");

           if(string.IsNullOrWhiteSpace(storedProcedureText))
                throw new ArgumentException("The stored procedure to call can't be null or whitespace.", nameof(storedProcedureText));

            return Mutate(w =>
                w.AppendClause($"CALL {storedProcedureText}"));
        }

        public ICypherFluentQuery Yield(string yieldText)
        {
            if (!Client.CypherCapabilities.SupportsStoredProcedures)
                throw new InvalidOperationException("YIELD not supported in Neo4j versions older than 3.0");

            if (string.IsNullOrWhiteSpace(yieldText))
                throw new ArgumentException("The value to yield can't be null or whitespace.", nameof(yieldText));

            return Mutate(w =>
                w.AppendClause($"YIELD {yieldText}"));
        }

        public ICypherFluentQuery CreateUnique(string createUniqueText)
        {
            return Mutate(w => w.AppendClause($"CREATE UNIQUE {createUniqueText}"));
        }

        public ICypherFluentQuery Create(string createText)
        {
            return Mutate(w => w.AppendClause($"CREATE {createText}"));
        }

        [Obsolete("Use Create(string) with explicitly named params instead. For example, instead of Create(\"(c:Customer {0})\", customer), use Create(\"(c:Customer {customer})\").WithParams(new { customer }).")]
        public ICypherFluentQuery Create(string createText, params object[] objects)
        {
            objects
                .ToList()
                .ForEach(o =>
                {
                    if (o == null)
                        throw new ArgumentException("Array includes a null entry", nameof(objects));

                    var objectType = o.GetType();
                    if (objectType.GetTypeInfo().IsGenericType &&
                        objectType.GetGenericTypeDefinition() == typeof(Node<>))
                    {
                        throw new ArgumentException(string.Format(
                            "You're trying to pass in a Node<{0}> instance. Just pass the {0} instance instead.",
                            objectType.GetGenericArguments()[0].Name),
                            nameof(objects));
                    }

                    var validationContext = new ValidationContext(o, null, null);
                    Validator.ValidateObject(o, validationContext);
                });

            return Mutate(w => w.AppendClause("CREATE " + createText, objects));
        }

        public ICypherFluentQuery Create<TNode>(string identity, TNode node)
            where TNode : class
        {
            if (typeof(TNode).GetTypeInfo().IsGenericType &&
                typeof(TNode).GetGenericTypeDefinition() == typeof(Node<>)) {
                throw new ArgumentException(string.Format(
                   "You're trying to pass in a Node<{0}> instance. Just pass the {0} instance instead.",
                   typeof(TNode).GetGenericArguments()[0].Name),
                   nameof(node));
            }

            if (node == null)
                throw new ArgumentNullException(nameof(node));

            var validationContext = new ValidationContext(node, null, null);
            Validator.ValidateObject(node, validationContext);

            return Mutate(w => w.AppendClause($"CREATE ({identity} {{0}})", node));
        }

        public ICypherFluentQuery CreateUniqueConstraint(string identity, string property)
        {
            return Mutate(w => w.AppendClause($"CREATE CONSTRAINT ON ({identity}) ASSERT {property} IS UNIQUE"));
        }

        public ICypherFluentQuery DropUniqueConstraint(string identity, string property)
        {
            return Mutate(w => w.AppendClause($"DROP CONSTRAINT ON ({identity}) ASSERT {property} IS UNIQUE"));
        }

        public ICypherFluentQuery Delete(string identities)
        {
            return Mutate(w =>
                w.AppendClause($"DELETE {identities}"));
        }

        public ICypherFluentQuery DetachDelete(string identities)
        {
            if (!Client.CypherCapabilities.SupportsStartsWith)
                throw new InvalidOperationException("DETACH DELETE not supported in Neo4j versions older than 2.3.0");

            if(identities.Contains("."))
                throw new InvalidOperationException("Unable to DETACH DELETE properties, you can only delete nodes & relationships.");

            return Mutate(w =>
                w.AppendClause($"DETACH DELETE {identities}"));
        }

        public ICypherFluentQuery Drop(string dropText)
        {
            return Mutate(w =>
                w.AppendClause($"DROP {dropText}"));
        }

        public ICypherFluentQuery Set(string setText)
        {
            return Mutate(w =>
                w.AppendClause($"SET {setText}"));
        }

        public ICypherFluentQuery Remove(string removeText)
        {
            return Mutate(w =>
                w.AppendClause($"REMOVE {removeText}"));
        }

        public ICypherFluentQuery ForEach(string text)
        {
            return Mutate(w => w.AppendClause("FOREACH " + text));
        }

        public ICypherFluentQuery LoadCsv(Uri fileUri, string identifier, bool withHeaders = false, string fieldTerminator = null, int? periodicCommit = null)
        {
            if(fileUri == null)
                throw new ArgumentException("File URI must be supplied.", nameof(fileUri));

            string periodicCommitText = string.Empty;
            if (periodicCommit != null)
            {
                periodicCommitText = "USING PERIODIC COMMIT";
                if (periodicCommit > 0)
                    periodicCommitText += $" {periodicCommit.Value}";
            }

            string withHeadersEnabledText = string.Empty;
            string fieldSeperatorEnabledText = string.Empty;
            if (withHeaders)
            {
                withHeadersEnabledText = " WITH HEADERS";
            }

            if (!string.IsNullOrEmpty(fieldTerminator))
            {
                fieldSeperatorEnabledText = $" FIELDTERMINATOR '{fieldTerminator}'";
            }

            return Mutate(w => w.AppendClause($"{periodicCommitText} LOAD CSV{withHeadersEnabledText} FROM '{fileUri.AbsoluteUri}' AS {identifier}{fieldSeperatorEnabledText}".Trim()));
        }

        public ICypherFluentQuery Unwind(string collectionName, string columnName)
        {
            return Mutate(w => w.AppendClause($"UNWIND {collectionName} AS {columnName}"));
        }

        public ICypherFluentQuery Unwind(IEnumerable collection, string identity)
        {
            return Mutate(w => w.AppendClause("UNWIND {0} AS " + identity, collection));
        }

        public ICypherFluentQuery Union()
        {
            return Mutate(w => w.AppendClause("UNION"));
        }

        public ICypherFluentQuery UnionAll()
        {
            return Mutate(w => w.AppendClause("UNION ALL"));
        }

        public ICypherFluentQuery Limit(int? limit)
        {
            return limit.HasValue
                ? Mutate(w => w.AppendClause("LIMIT {0}", limit))
                : this;
        }

        public ICypherFluentQuery Skip(int? skip)
        {
            return skip.HasValue
                ? Mutate(w => w.AppendClause("SKIP {0}", skip))
                : this;
        }

        public IOrderedCypherFluentQuery OrderBy(params string[] properties)
        {
            return MutateOrdered(w =>
                w.AppendClause($"ORDER BY {string.Join(", ", properties)}"));
        }

        public IOrderedCypherFluentQuery OrderByDescending(params string[] properties)
        {
            return MutateOrdered(w =>
                w.AppendClause($"ORDER BY {string.Join(" DESC, ", properties)} DESC"));
        }

        public IOrderedCypherFluentQuery ThenBy(params string[] properties)
        {
            return MutateOrdered(w =>
                w.AppendToClause($", {string.Join(", ", properties)}"));
        }

        public IOrderedCypherFluentQuery ThenByDescending(params string[] properties)
        {
            return MutateOrdered(w =>
                w.AppendToClause($", {string.Join(" DESC, ", properties)} DESC"));
        }

        public CypherQuery Query => QueryWriter.ToCypherQuery(Client.JsonContractResolver ?? GraphClient.DefaultJsonContractResolver, IsWrite, IncludeQueryStats);

        public Task ExecuteWithoutResultsAsync()
        {
            return Client.ExecuteCypherAsync(Query);
        }

        public ICypherFluentQueryAdvanced Advanced { get; }
        

        IGraphClient IAttachedReference.Client => Client;

        public ICypherFluentQuery ParserVersion(string version)
        {
            return Mutate(w => w.AppendClause($"CYPHER {version}"));
        }

        public ICypherFluentQuery ParserVersion(Version version)
        {
            return ParserVersion(version < minimumCypherParserVersion 
                ? "LEGACY" 
                : $"{version.Major}.{version.Minor}");
        }

        public ICypherFluentQuery ParserVersion(int major, int minor)
        {
            return ParserVersion(new Version(major, minor));
        }

        public ICypherFluentQuery Planner(string planner)
        {
            if(!Client.CypherCapabilities.SupportsPlanner)
                throw new InvalidOperationException("PLANNER not supported in Neo4j versions older than 2.2");

            return Mutate(w => w.AppendClause($"PLANNER {planner}"));
        }

        public ICypherFluentQuery Planner(CypherPlanner planner)
        {
            switch (planner)
            {
                case CypherPlanner.Rule:
                    return Planner("RULE");
                case CypherPlanner.CostIdp:
                    return Planner("IDP");
                case CypherPlanner.CostGreedy:
                    return Planner("COST");
                default:
                    throw new ArgumentOutOfRangeException(nameof(planner), planner, null);
            }
        }
        
        public ICypherFluentQuery MaxExecutionTime(int milliseconds)
        {
            QueryWriter.MaxExecutionTime = milliseconds;
            return this;
        }

        public ICypherFluentQuery CustomHeaders(NameValueCollection headers)
        {
            QueryWriter.CustomHeaders = headers;
            return this;
        }



        public static string ApplyCamelCase(bool isCamelCase, string propertyName)
        {
            return isCamelCase 
                ? $"{propertyName.Substring(0, 1).ToLowerInvariant()}{(propertyName.Length > 1 ? propertyName.Substring(1, propertyName.Length - 1) : string.Empty)}"
                : propertyName;
        }

        /// <inheritdoc />
        public ICypherFluentQuery WithIdentifier(string identifier)
        {
            QueryWriter.Identifier = string.IsNullOrWhiteSpace(identifier) ? null : identifier;
            return this;
        }
    }
}
