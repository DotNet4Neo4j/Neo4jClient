## Neo4jClient 
---

A .NET client for [Neo4j](https://neo4j.com). Supports Cypher queries via fluent interfaces, and some indexing operations.

Grab the latest drop straight from the `Neo4jClient` package on [NuGet](http://nuget.org/List/Packages/Neo4jClient).

Read [our wiki docs](https://github.com/DotNet4Neo4j/Neo4jClient/wiki) - **Currently OUT OF DATE**

---
## Current Builds
The official Neo4jClient build and nuget package is automated via [AppVeyor](http://www.appveyor.com). 

---
## Stable 4.x [![Build status](https://ci.appveyor.com/api/projects/status/q96upd53uq0hyepe?svg=true)](https://ci.appveyor.com/project/ChrisSkardon/neo4jclient)

Version 4.0.0 of Neo4jClient is _now_ the stable version. There have been a lot of changes, additions, removals, so it's likely there will be breaking changes.

---

## Changing from 3.x to 4.x

This isn't an exhaustive list of things you need to do, but I'll try to add things if I've forgotten them.

### Uris

You will need to use the correct URI for the server version you are connecting to:

#### GraphClient
  * 3.x server: `http://localhost:7474/db/data`
  * 4.x server: `http://localhost:7474/`

#### BoltGraphClient
 * 3.x or 4.x server: `neo4j://localhost:7687`
 * Worth reviewing the [Neo4j Documentation](https://neo4j.com/docs/driver-manual/current/client-applications/#driver-configuration-examples) to see what you need to use.

 ### Async

 As this release is 100% `async` you will need to update any calls to `Results` or `ExecuteWithoutResults` to their `Async` equivalents.

---

## Breaking Changes

* Async endpoints only
  * To get this release out, `Neo4jClient` is `Async` only now. 
* Transactions will no longer use the `TransactionScope` class which means that [MSDTC](https://en.wikipedia.org/wiki/Microsoft_Distributed_Transaction_Coordinator) will no longer work.
  * This has been an issue since the dawn of Core/NetStandard - `TransactionScope` may be in NetStandard now - but the other classes the Transaction code was relying on wasn't. 
* The `GraphClient` and `BoltGraphClient` will no longer support Neo4j 3.4 or lower.
  * Largely this is because the `Neo4j.Driver` that does the `Bolt` side of things only targets 3.5+, and keeping all the backwards compatibility means a lot of work, for little gain.

### Dependency Changes

* [Json.NET](https://www.nuget.org/packages/Newtonsoft.Json/) - `10.0.3` -> `12.0.3`
* [Neo4j.Driver.Signed](https://www.nuget.org/packages/Neo4j.Driver.Signed/4.1.1) - `1.7.2` -> `4.1.1`

---
## Historical Notes

If you're changing from `2.x` to `3.x`, you'll want the below information, but you should really be on `4.x` unless you have to target an older DB instance.

### Changes in 3.x

* Bolt!
* Transactions now use `AsyncLocal<>` instead of `ThreadStatic`
  * Transactions still don't work in the .NET Core version for the same reason as listed below (in `Breaking Changes in 2.0`)
  * `TransactionScope` _does_ exist in `NetStandard 2.0` - but some of the other bits surrounding the Transaction management doesn't. 
* JSON.NET updated to 10.0.3
* `PathResults` doesn't work with Bolt, you need to use `PathResultsBolt` instead.

### Dependency Changes in 2.0

* JSON.NET updated to 9.0.1 

### Breaking Changes in 2.0

* If using the *DotNet Core* version of `Neo4jClient` - transactions will **not** work. This will be returned when DotNet Core gets the `TransactionScope` (See [this comment](https://github.com/Readify/Neo4jClient/issues/135#issuecomment-231981065) for more details).

---

## License Information

Licensed under MS-PL. See `LICENSE` in the root of this repository for full license text.

---

## Updates to the 3.x releases

I will not be updating the 3.x version of the client, the focus is on 4.x and the features that gives us. Neo4j no longer actively support Neo4j 3.4 so you should consider updating if you can. Largely - anyone using the `3.x` version of the client is coping with it's deficiencies, and as 4.x addresses most of them. ¯\_(ツ)_/¯
