module Neo4jClient.Tests.Cypher.Where

  open Neo4jClient
  open Neo4jClient.Cypher
  open Xunit
  open Moq

  [<CLIMutable>]
  type ObjWithCount = { Name:string; Count:int }
  
  [<Fact>] 
  let where_should_inject_correct_parameter () = 
    let mockGc = new Mock<IRawGraphClient>()
    let cfq = new CypherFluentQuery(mockGc.Object);
    let query =
        cfq
            .Where(fun u -> u.Count = 1000).Query
    Assert.NotNull query
    Assert.Equal(query.QueryText, "WHERE (u.Count = $p0)")
    Assert.Equal(query.QueryParameters.["p0"], 1000)

  [<Fact>]
  let generates_the_correct_return_statement () = 
    let mockGc = new Mock<IRawGraphClient>()
    let cfq = new CypherFluentQuery(mockGc.Object);
    let query = 
        cfq
          .Return(fun (u : Cypher.ICypherResultItem) (t : Cypher.ICypherResultItem) -> u.As<ObjWithCount>(), t.Count()).Query
    Assert.NotNull query
    Assert.Equal(query.QueryText, "RETURN u AS Item1, count(t) AS Item2")
