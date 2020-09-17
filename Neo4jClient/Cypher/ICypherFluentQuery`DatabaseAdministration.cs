using Neo4j.Driver;

namespace Neo4jClient.Cypher
{
    public partial interface ICypherFluentQuery
    {
        /// <summary>
        ///     Creates a new Database in Neo4j
        /// </summary>
        /// <remarks>4.x Server only.</remarks>
        /// <param name="databaseName">The name of the database instance to create.</param>
        /// <param name="ifNotExists">
        ///     If <c>true</c> no exception will be thrown if the database already exists, <c>false</c>
        ///     (default) will throw an exception if the database already exists..
        /// </param>
        /// <returns>An <see cref="ICypherFluentQuery" /> instance to continue the query with.</returns>
        ICypherFluentQuery CreateDatabase(string databaseName, bool ifNotExists = false);

        /// <summary>
        ///     This will create a new Database in Neo4j, regardless of one already existing.
        /// </summary>
        /// <remarks>It will DELETE the existing database if it exists.</remarks>
        /// <param name="databaseName">The name of the database instance to create.</param>
        /// <returns>An <see cref="ICypherFluentQuery" /> instance to continue the query with.</returns>
        ICypherFluentQuery CreateOrReplaceDatabase(string databaseName);

        /// <summary>
        ///     Deletes the given database from Neo4j.
        /// </summary>
        /// <param name="databaseName">The name of the database to delete.</param>
        /// <param name="dumpData">
        ///     If <c>true</c> the data will be dumped to the <c>dbms.directories.dumps.root</c> setting of your
        ///     server. <c>false</c> (default) will mean the data is destroyed.
        /// </param>
        /// <returns>An <see cref="ICypherFluentQuery" /> instance to continue the query with.</returns>
        /// <exception cref="ClientException">
        ///     Thrown if the database doesn't exist. Use <see cref="DropDatabaseIfExists" /> to
        ///     avoid the exception.
        /// </exception>
        ICypherFluentQuery DropDatabase(string databaseName, bool dumpData = false);

        /// <summary>
        ///     Deletes the given database from Neo4j if it exists.
        /// </summary>
        /// <param name="databaseName">The name of the database to delete.</param>
        /// <param name="dumpData">
        ///     If <c>true</c> the data will be dumped to the <c>dbms.directories.dumps.root</c> setting of your
        ///     server. <c>false</c> (default) will mean the data is destroyed.
        /// </param>
        /// <returns>An <see cref="ICypherFluentQuery" /> instance to continue the query with.</returns>
        ICypherFluentQuery DropDatabaseIfExists(string databaseName, bool dumpData = false);

        /// <summary>
        ///     Starts the given database instance on the server.
        /// </summary>
        /// <param name="databaseName">The name of the database to start.</param>
        /// <returns>An <see cref="ICypherFluentQuery" /> instance to continue the query with.</returns>
        ICypherFluentQuery StartDatabase(string databaseName);

        /// <summary>
        ///     Stops the given database instance on the server.
        /// </summary>
        /// <param name="databaseName">The name of the database to stop.</param>
        /// <returns>An <see cref="ICypherFluentQuery" /> instance to continue the query with.</returns>
        ICypherFluentQuery StopDatabase(string databaseName);
    }
}