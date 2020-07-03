## What is Neo4jClient?

A .NET client for [Neo4j](https://neo4j.com). Supports basic CRUD operations, Cypher and Gremlin queries via fluent interfaces, and some indexing operations.

Grab the latest drop straight from the `Neo4jClient` package on [NuGet](http://nuget.org/List/Packages/Neo4jClient).

Read [our wiki docs](https://github.com/Readify/Neo4jClient/wiki).

## Current Builds
The official Neo4jClient build and nuget package is automated via [AppVeyor](http://www.appveyor.com). 

## PreRelease (4.x)

[![Build status](https://ci.appveyor.com/api/projects/status/gu4ra8yufideqrjh/branch/40-development?svg=true)](https://ci.appveyor.com/project/ChrisSkardon/neo4jclient-40/branch/40-development)

It's worth noting - due to a lot of the changes that are taking place - at the moment this will be super unstable - bugs will be introduced, features added/removed/changed etc.

### Plans

* Finally transactions in Core
* Fully Async
* Support Bolt 2.0 (for Neo4j 4.x)

### Things being considered

* Removal of Gremlin support
  * No-one uses it anymore, and it's not supported by Neo4j anymore.
* Removal of `Root` property 
  * This has been out of favour since Neo4j `2.0` and been marked as `Obsolete` since then.
* Removal of _all_ the `Obsolete` code.
  * It's been obsolete for a loooong time now :)
* Having a _Signed_ version of the Client.
  * This largely comes down to how _easy_ it is do this - any advice would be super - I'm using AppVeyor to CI and deploy to nuget, so let me know what I need to do!

### Breaking Changes

* Transactions will no longer use the `TransactionScope` class which means that [MSDTC](https://en.wikipedia.org/wiki/Microsoft_Distributed_Transaction_Coordinator) will no longer work.
  * This has been an issue since the dawn of Core/NetStandard - `TransactionScope` may be in NetStandard now - but the other classes the Transaction code was relying on wasn't. 
* The `GraphClient` and `BoltGraphClient` will no longer support Neo4j 3.4 or lower.
  * Largely this is because the `Neo4j.Driver` that does the `Bolt` side of things only targets 3.5+, and keeping all the backwards compatibility means a lot of work, for little gain.

### Dependency Changes

* [Json.NET](https://www.nuget.org/packages/Newtonsoft.Json/) - `10.0.3` -> `12.0.3`
* [Neo4j.Driver.Signed](https://www.nuget.org/packages/Neo4j.Driver.Signed/4.0.0-beta01) - `1.7.2` -> `4.0.1`

---

## Stable (3.x)

[![Build status](https://ci.appveyor.com/api/projects/status/q96upd53uq0hyepe?svg=true)](https://ci.appveyor.com/project/ChrisSkardon/neo4jclient)

### Changes in 3.x

* Bolt!
* Transactions now use `AsyncLocal<>` instead of `ThreadStatic`
  * Transactions still don't work in the .NET Core version for the same reason as listed below (in `Breaking Changes in 2.0`)
  * `TransactionScope` _does_ exist in `NetStandard 2.0` - but some of the other bits surrounding the Transaction management doesn't. 
* JSON.NET updated to 10.0.3
* `PathResults` doesn't work with Bolt, you need to use `PathResultsBolt` instead.

---

### Dependency Changes in 2.0

* JSON.NET updated to 9.0.1 

### Breaking Changes in 2.0

* If using the *DotNet Core* version of `Neo4jClient` - transactions will **not** work. This will be returned when DotNet Core gets the `TransactionScope` (See [this comment](https://github.com/Readify/Neo4jClient/issues/135#issuecomment-231981065) for more details).

## License Information

Licensed under MS-PL. See `LICENSE` in the root of this repository for full license text.
