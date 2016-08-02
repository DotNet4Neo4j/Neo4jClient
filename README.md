## What is Neo4jClient?

A .NET client for neo4j. Supports basic CRUD operations, Cypher and Gremlin queries via fluent interfaces, and some indexing operations.

Grab the latest drop straight from the `Neo4jClient` package on [NuGet](http://nuget.org/List/Packages/Neo4jClient).

Read [our wiki doco](https://github.com/Readify/Neo4jClient/wiki).

## Current Builds
The official neo4jclient build and nuget package is automated via MyGet [build services](http://docs.myget.org/docs/reference/build-services). Contributors can test private builds using MyGet build services under their own account.

### Stable [![neo4jclient-tx MyGet Build Status](https://www.myget.org/BuildSource/Badge/neo4jclient-tx?identifier=57c22856-7609-4211-a432-a1ecdf6f1497)](https://www.myget.org/)

#### Dependency Changes in 2.0

* JSON.NET updated to 9.0.1 
* Microsoft.Net.Http updated to 2.2.29

#### Breaking Changes in 2.0

* If using the *DotNet Core* version of `Neo4jClient` - transactions will **not** work. This will be returned when DotNet Core gets the `TransactionScope` (See [this comment](https://github.com/Readify/Neo4jClient/issues/135#issuecomment-231981065) for more details).

## License Information

Licensed under MS-PL. See `LICENSE` in the root of this repository for full license text.
