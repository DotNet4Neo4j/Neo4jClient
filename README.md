## What is Neo4jClient?

A .NET client for neo4j. Supports basic CRUD operations, Cypher and Gremlin queries via fluent interfaces, and some indexing operations.

Grab the latest drop straight from the `Neo4jClient` package on [NuGet](http://nuget.org/List/Packages/Neo4jClient).

Read [our wiki doco](https://github.com/Readify/Neo4jClient/wiki).

## Current Builds

### Stable [![neo4jclient-tx MyGet Build Status](https://www.myget.org/BuildSource/Badge/neo4jclient-tx?identifier=57c22856-7609-4211-a432-a1ecdf6f1497)](https://www.myget.org/)

The current stable release doesn't contain Transaction support, and is due to be superceded soon.

### Pre-Release [![neo4jclient-tx MyGet Build Status](https://www.myget.org/BuildSource/Badge/neo4jclient-tx?identifier=d0ddcfa5-4a79-4e0b-84ac-9cf11135c7b1)](https://www.myget.org/)

The current pre-release contains Transaction support and Authentication via the Client support.

#### Breaking Changes in Pre-Release

* _(Tx0009 onwards)_ `CollectAs` now returns `IEnumerable<T>` and not `IEnumerable<Node<T>>`
* _(Tx0010 onwards)_ `IHttpClient` now contains a `Username` and `Password` getter, this should have an effect if you're using a custom ``HttpClient`` for Authentication. 
With the Pre-Release you no longer need to use a custom HttpClient, the `GraphClient` supports authentication now.


## License Information

Licensed under MS-PL. See `LICENSE` in the root of this repository for full license text.
