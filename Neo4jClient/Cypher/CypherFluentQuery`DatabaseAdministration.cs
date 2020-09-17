using System;

namespace Neo4jClient.Cypher
{
    public partial class CypherFluentQuery
    {
        private const string MultipleTenancyExceptionMessage = "DATABASE commands are not supported in Neo4j versions older than 4.0";

        private string DatabaseAdministrationChecks(string databaseName)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentException("You have to supply a name for the database.", nameof(databaseName));

            if (!Client.CypherCapabilities.SupportsMultipleTenancy)
                throw new InvalidOperationException(MultipleTenancyExceptionMessage);

            return databaseName.ToLowerInvariant();
        }

        public ICypherFluentQuery CreateDatabase(string databaseName, bool ifNotExists = false)
        {
            databaseName = DatabaseAdministrationChecks(databaseName);
            return Mutate(w => w.AppendClause($"CREATE DATABASE {databaseName}{(ifNotExists ? " IF NOT EXISTS" : "")}"));
        }

        public ICypherFluentQuery CreateOrReplaceDatabase(string databaseName)
        {
            databaseName = DatabaseAdministrationChecks(databaseName);
            return Mutate(w => w.AppendClause($"CREATE OR REPLACE DATABASE {databaseName}"));
        }

        public ICypherFluentQuery DropDatabase(string databaseName, bool dumpData = false)
        {
            databaseName = DatabaseAdministrationChecks(databaseName);
            return Mutate(w => w.AppendClause($"DROP DATABASE {databaseName}{(dumpData ? " DUMP DATA" : "")}"));
        }

        public ICypherFluentQuery DropDatabaseIfExists(string databaseName, bool dumpData = false)
        {
            databaseName = DatabaseAdministrationChecks(databaseName);
            return Mutate(w => w.AppendClause($"DROP DATABASE {databaseName} IF EXISTS{(dumpData ? " DUMP DATA" : "")}"));
        }

        public ICypherFluentQuery StartDatabase(string databaseName)
        {
            databaseName = DatabaseAdministrationChecks(databaseName);
            return Mutate(w => w.AppendClause($"START DATABASE {databaseName}"));
        }

        public ICypherFluentQuery StopDatabase(string databaseName)
        {
            databaseName = DatabaseAdministrationChecks(databaseName);
            return Mutate(w => w.AppendClause($"STOP DATABASE {databaseName}"));
        }

    }
}