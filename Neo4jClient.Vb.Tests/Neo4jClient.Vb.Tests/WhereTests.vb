Imports FluentAssertions
Imports Moq
Imports Neo4jClient.Cypher
Imports NUnit.Framework

<TestFixture>
Public Class WhereTests
    <Test>
    Public Sub CreatesCorrectCypherWhenStringComparisonPassedIn()
        Dim mock As New Mock(Of IRawGraphClient)()

        Dim c = New CypherFluentQuery(mock.[Object]).Where(Function(n As Foo) n.Name = "NameIn").Query
        c.DebugQueryText.Should().Be("WHERE (n.Name = ""NameIn"")")
    End Sub


        <Test>
    Public Sub CreatesCorrectCypherWhenUsingClassInstance()
        Dim mock As New Mock(Of IRawGraphClient)()

        Dim c = New CypherFluentQuery(mock.[Object]).Where(Function(n As Foo) n.Name = "NameIn").Query
        c.DebugQueryText.Should().Be("WHERE (n.Name = ""NameIn"")")
    End Sub

End Class


Public Class Foo
    Public Property Name As String
End Class