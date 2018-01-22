## What is Neo4jClient?

A .NET client for neo4j. Supports basic CRUD operations, Cypher and Gremlin queries via fluent interfaces, and some indexing operations.

Grab the latest drop straight from the `Neo4jClient` package on [NuGet](http://nuget.org/List/Packages/Neo4jClient).

Read [our wiki doco](https://github.com/Readify/Neo4jClient/wiki).

## Current Builds
The official neo4jclient build and nuget package is automated via [AppVeyor](http://www.appveyor.com). 

### Stable (3.x)

[![Build status](https://ci.appveyor.com/api/projects/status/q96upd53uq0hyepe?svg=true)](https://ci.appveyor.com/project/ChrisSkardon/neo4jclient)

#### Changes in 3.x

* Bolt!
* Transactions now use `AsyncLocal<>` instead of `ThreadStatic`
  * Transactions still don't work in the .NET Core version for the same reason as listed below (in `Breaking Changes in 2.0`)
  * `TransactionScope` _does_ exist in `NetStandard 2.0` - but some of the other bits surrounding the Transaction management doesn't. 
* JSON.NET updated to 10.0.3
* `PathResults` doesn't work with Bolt, you need to use `PathResultsBolt` instead.

#### Dependency Changes in 2.0

* JSON.NET updated to 9.0.1 

#### Breaking Changes in 2.0

* If using the *DotNet Core* version of `Neo4jClient` - transactions will **not** work. This will be returned when DotNet Core gets the `TransactionScope` (See [this comment](https://github.com/Readify/Neo4jClient/issues/135#issuecomment-231981065) for more details).

## License Information

Licensed under MS-PL. See `LICENSE` in the root of this repository for full license text.
