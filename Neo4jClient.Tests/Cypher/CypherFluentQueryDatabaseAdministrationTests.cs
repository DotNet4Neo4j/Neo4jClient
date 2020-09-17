using System;
using FluentAssertions;
using Neo4jClient.Cypher;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Tests.Cypher
{
    public class CypherFluentQueryDatabaseAdministrationTests
    {
        private static IRawGraphClient MockClient
        {
            get
            {
                var client = Substitute.For<IRawGraphClient>();
                client.CypherCapabilities.Returns(new CypherCapabilities { SupportsMultipleTenancy = true, SupportsShow = true});
                return client;
            }
        }

        //https://neo4j.com/docs/cypher-manual/current/administration/databases/#administration-databases-create-database
        public class CreateDatabaseMethod : IClassFixture<CultureInfoSetupFixture>
        {
            [Fact]
            public void GeneratesTheCorrectCypher()
            {
                var query = new CypherFluentQuery(MockClient)
                    .CreateDatabase("foo")
                    .Query;

                query.QueryText.Should().Be($"CREATE DATABASE foo");
            }

            [Fact]
            public void GeneratesTheCorrectCypher_IfNotExists()
            {
                var query = new CypherFluentQuery(MockClient)
                    .CreateDatabase("foo", true)
                    .Query;

                query.QueryText.Should().Be($"CREATE DATABASE foo IF NOT EXISTS");
            }

            [Fact]
            public void ThrowsInvalidOperationException_IfNotOnASupportedVersion()
            {
                var client = Substitute.For<IRawGraphClient>();
                client.CypherCapabilities.Returns(new CypherCapabilities {SupportsMultipleTenancy = false});

                var ex = Assert.Throws<InvalidOperationException>(() => new CypherFluentQuery(client).CreateDatabase("foo", true));
                ex.Should().NotBeNull();
                ex.Message.Should().Be("DATABASE commands are not supported in Neo4j versions older than 4.0");
            }

            [Theory]
            [InlineData("FOO")]
            [InlineData("foo")]
            [InlineData("Foo")]
            [InlineData("fOO")]
            [InlineData("FoO")]
            public void UsesLowerCaseForDatabaseName(string name)
            {
                var query = new CypherFluentQuery(MockClient)
                    .CreateDatabase(name)
                    .Query;

                query.QueryText.Should().Be($"CREATE DATABASE foo");
            }

            [Theory]
            [InlineData("")]
            [InlineData(" ")]
            [InlineData("  ")]
            [InlineData(null)]
            public void ThrowsArgumentException_IfDatabaseNameIsInvalid(string databaseName)
            {
                var ex = Assert.Throws<ArgumentException>(() => new CypherFluentQuery(MockClient).CreateDatabase(databaseName));
                ex.Should().NotBeNull();
            }
        }

        //https://neo4j.com/docs/cypher-manual/current/administration/databases/#administration-databases-create-database
        public class CreateOrReplaceDatabaseMethod : IClassFixture<CultureInfoSetupFixture>
        {
            [Fact]
            public void GeneratesTheCorrectCypher()
            {
                var query = new CypherFluentQuery(MockClient)
                    .CreateOrReplaceDatabase("foo")
                    .Query;

                query.QueryText.Should().Be($"CREATE OR REPLACE DATABASE foo");
            }

            [Fact]
            public void ThrowsInvalidOperationException_IfNotOnASupportedVersion()
            {
                var client = Substitute.For<IRawGraphClient>();
                client.CypherCapabilities.Returns(new CypherCapabilities { SupportsMultipleTenancy = false });

                var ex = Assert.Throws<InvalidOperationException>(() => new CypherFluentQuery(client).CreateOrReplaceDatabase("foo"));
                ex.Should().NotBeNull();
                ex.Message.Should().Be("DATABASE commands are not supported in Neo4j versions older than 4.0");
            }

            [Theory]
            [InlineData("FOO")]
            [InlineData("foo")]
            [InlineData("Foo")]
            [InlineData("fOO")]
            [InlineData("FoO")]
            public void UsesLowerCaseForDatabaseName(string name)
            {
                var query = new CypherFluentQuery(MockClient)
                    .CreateOrReplaceDatabase(name)
                    .Query;

                query.QueryText.Should().Be($"CREATE OR REPLACE DATABASE foo");
            }

            [Theory]
            [InlineData("")]
            [InlineData(" ")]
            [InlineData("  ")]
            [InlineData(null)]
            public void ThrowsArgumentException_IfDatabaseNameIsInvalid(string databaseName)
            {
                var ex = Assert.Throws<ArgumentException>(() => new CypherFluentQuery(MockClient).CreateOrReplaceDatabase(databaseName));
                ex.Should().NotBeNull();
            }

        }

        //https://neo4j.com/docs/cypher-manual/current/administration/databases/#administration-databases-drop-database
        public class DropDatabaseMethod : IClassFixture<CultureInfoSetupFixture>
        {
            [Fact]
            public void GeneratesTheCorrectCypher()
            {
                var query = new CypherFluentQuery(MockClient)
                    .DropDatabase("foo")
                    .Query;

                query.QueryText.Should().Be($"DROP DATABASE foo");
            }

            [Fact]
            public void GeneratesTheCorrectCypher_IfNotExists()
            {
                var query = new CypherFluentQuery(MockClient)
                    .DropDatabase("foo", true)
                    .Query;

                query.QueryText.Should().Be($"DROP DATABASE foo DUMP DATA");
            }

            [Fact]
            public void ThrowsInvalidOperationException_IfNotOnASupportedVersion()
            {
                var client = Substitute.For<IRawGraphClient>();
                client.CypherCapabilities.Returns(new CypherCapabilities {SupportsMultipleTenancy = false});

                var ex = Assert.Throws<InvalidOperationException>(() => new CypherFluentQuery(client).DropDatabase("foo"));
                ex.Should().NotBeNull();
                ex.Message.Should().Be("DATABASE commands are not supported in Neo4j versions older than 4.0");
            }

            [Theory]
            [InlineData("FOO")]
            [InlineData("foo")]
            [InlineData("Foo")]
            [InlineData("fOO")]
            [InlineData("FoO")]
            public void UsesLowerCaseForDatabaseName(string name)
            {
                var query = new CypherFluentQuery(MockClient)
                    .DropDatabase(name)
                    .Query;

                query.QueryText.Should().Be($"DROP DATABASE foo");
            }

            [Theory]
            [InlineData("")]
            [InlineData(" ")]
            [InlineData("  ")]
            [InlineData(null)]
            public void ThrowsArgumentException_IfDatabaseNameIsInvalid(string databaseName)
            {
                var ex = Assert.Throws<ArgumentException>(() => new CypherFluentQuery(MockClient).DropDatabase(databaseName));
                ex.Should().NotBeNull();
            }
        }

        //https://neo4j.com/docs/cypher-manual/current/administration/databases/#administration-databases-drop-database
        public class DropDatabaseIfExistsMethod : IClassFixture<CultureInfoSetupFixture>
        {
            [Fact]
            public void GeneratesTheCorrectCypher()
            {
                var query = new CypherFluentQuery(MockClient)
                    .DropDatabaseIfExists("foo")
                    .Query;

                query.QueryText.Should().Be($"DROP DATABASE foo IF EXISTS");
            }

            [Fact]
            public void GeneratesTheCorrectCypher_IfNotExists()
            {
                var query = new CypherFluentQuery(MockClient)
                    .DropDatabaseIfExists("foo", true)
                    .Query;

                query.QueryText.Should().Be($"DROP DATABASE foo IF EXISTS DUMP DATA");
            }

            [Fact]
            public void ThrowsInvalidOperationException_IfNotOnASupportedVersion()
            {
                var client = Substitute.For<IRawGraphClient>();
                client.CypherCapabilities.Returns(new CypherCapabilities { SupportsMultipleTenancy = false });

                var ex = Assert.Throws<InvalidOperationException>(() => new CypherFluentQuery(client).DropDatabaseIfExists("foo"));
                ex.Should().NotBeNull();
                ex.Message.Should().Be("DATABASE commands are not supported in Neo4j versions older than 4.0");
            }

            [Theory]
            [InlineData("FOO")]
            [InlineData("foo")]
            [InlineData("Foo")]
            [InlineData("fOO")]
            [InlineData("FoO")]
            public void UsesLowerCaseForDatabaseName(string name)
            {
                var query = new CypherFluentQuery(MockClient)
                    .DropDatabaseIfExists(name)
                    .Query;

                query.QueryText.Should().Be($"DROP DATABASE foo IF EXISTS");
            }

            [Theory]
            [InlineData("")]
            [InlineData(" ")]
            [InlineData("  ")]
            [InlineData(null)]
            public void ThrowsArgumentException_IfDatabaseNameIsInvalid(string databaseName)
            {
                var ex = Assert.Throws<ArgumentException>(() => new CypherFluentQuery(MockClient).DropDatabaseIfExists(databaseName));
                ex.Should().NotBeNull();
            }
        }

        //https://neo4j.com/docs/cypher-manual/current/administration/databases/#administration-databases-start-database
        public class StartDatabaseMethod : IClassFixture<CultureInfoSetupFixture>
        {
            [Fact]
            public void GeneratesTheCorrectCypher()
            {
                var query = new CypherFluentQuery(MockClient)
                    .StartDatabase("foo")
                    .Query;

                query.QueryText.Should().Be($"START DATABASE foo");
            }

            [Fact]
            public void ThrowsInvalidOperationException_IfNotOnASupportedVersion()
            {
                var client = Substitute.For<IRawGraphClient>();
                client.CypherCapabilities.Returns(new CypherCapabilities { SupportsMultipleTenancy = false });

                var ex = Assert.Throws<InvalidOperationException>(() => new CypherFluentQuery(client).StartDatabase("foo"));
                ex.Should().NotBeNull();
                ex.Message.Should().Be("DATABASE commands are not supported in Neo4j versions older than 4.0");
            }

            [Theory]
            [InlineData("FOO")]
            [InlineData("foo")]
            [InlineData("Foo")]
            [InlineData("fOO")]
            [InlineData("FoO")]
            public void UsesLowerCaseForDatabaseName(string name)
            {
                var query = new CypherFluentQuery(MockClient)
                    .StartDatabase(name)
                    .Query;

                query.QueryText.Should().Be($"START DATABASE foo");
            }

            [Theory]
            [InlineData("")]
            [InlineData(" ")]
            [InlineData("  ")]
            [InlineData(null)]
            public void ThrowsArgumentException_IfDatabaseNameIsInvalid(string databaseName)
            {
                var ex = Assert.Throws<ArgumentException>(() => new CypherFluentQuery(MockClient).StartDatabase(databaseName));
                ex.Should().NotBeNull();
            }
        }

        //https://neo4j.com/docs/cypher-manual/current/administration/databases/#administration-databases-stop-database
        public class StopDatabaseMethod : IClassFixture<CultureInfoSetupFixture>
        {
            [Fact]
            public void GeneratesTheCorrectCypher()
            {
                var query = new CypherFluentQuery(MockClient)
                    .StopDatabase("foo")
                    .Query;

                query.QueryText.Should().Be($"STOP DATABASE foo");
            }

            [Fact]
            public void ThrowsInvalidOperationException_IfNotOnASupportedVersion()
            {
                var client = Substitute.For<IRawGraphClient>();
                client.CypherCapabilities.Returns(new CypherCapabilities { SupportsMultipleTenancy = false });

                var ex = Assert.Throws<InvalidOperationException>(() => new CypherFluentQuery(client).StopDatabase("foo"));
                ex.Should().NotBeNull();
                ex.Message.Should().Be("DATABASE commands are not supported in Neo4j versions older than 4.0");
            }

            [Theory]
            [InlineData("FOO")]
            [InlineData("foo")]
            [InlineData("Foo")]
            [InlineData("fOO")]
            [InlineData("FoO")]
            public void UsesLowerCaseForDatabaseName(string name)
            {
                var query = new CypherFluentQuery(MockClient)
                    .StopDatabase(name)
                    .Query;

                query.QueryText.Should().Be($"STOP DATABASE foo");
            }

            [Theory]
            [InlineData("")]
            [InlineData(" ")]
            [InlineData("  ")]
            [InlineData(null)]
            public void ThrowsArgumentException_IfDatabaseNameIsInvalid(string databaseName)
            {
                var ex = Assert.Throws<ArgumentException>(() => new CypherFluentQuery(MockClient).StopDatabase(databaseName));
                ex.Should().NotBeNull();
            }
        }

        public class ShowMethod : IClassFixture<CultureInfoSetupFixture>
        {
            [Fact]
            public void GeneratesTheCorrectCypher()
            {
                var query = new CypherFluentQuery(MockClient)
                    .Show("DATABASES")
                    .Query;

                query.QueryText.Should().Be($"SHOW DATABASES");
            }

            [Fact]
            public void ThrowsInvalidOperationException_IfNotOnASupportedVersion()
            {
                var client = Substitute.For<IRawGraphClient>();
                client.CypherCapabilities.Returns(new CypherCapabilities { SupportsMultipleTenancy = false });

                var ex = Assert.Throws<InvalidOperationException>(() => new CypherFluentQuery(client).Show("DATABASES"));
                ex.Should().NotBeNull();
                ex.Message.Should().Be("SHOW commands are not supported in Neo4j versions older than 4.0");
            }

            [Theory]
            [InlineData("")]
            [InlineData(" ")]
            [InlineData("  ")]
            [InlineData(null)]
            public void ThrowsArgumentException_IfDatabaseNameIsInvalid(string command)
            {
                var ex = Assert.Throws<ArgumentException>(() => new CypherFluentQuery(MockClient).Show(command));
                ex.Should().NotBeNull();
            }

        }
    }
}