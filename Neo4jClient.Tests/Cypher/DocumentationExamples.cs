using Neo4jClient.Cypher;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedVariable

namespace Neo4jClient.Test.Cypher
{
    public class DocumentationExamples
    {
        static IGraphClient BuildClient()
        {
            return null;
        }

        public void NodeById()
        {
            // ##start Cypher
            // START n=node(1)
            // RETURN n
            // ##end Cypher

            var client = BuildClient();

            var someNodeReferenceAlreadyLoaded = (NodeReference)1;
            // ##start C#
            var results = client.Cypher
                .Start(new { n = someNodeReferenceAlreadyLoaded })
                .Return(n => n.Node<Person>())
                .Results;
            // ##end C#

            // ##start Note
            // You could use Start(new { n = (NodeReference)1 }), however we try to avoid passing around integer references like that. You should find the node via another query, or via an index, rather than remembering ids. To get your query started, you can use IGraphClient.RootNode.
            // ##end Note
        }

        public void RelationshipById()
        {
            // ##start Cypher
            // START n=relationship(1)
            // RETURN n
            // ##end Cypher

            var client = BuildClient();

            var someRelationshipReferenceAlreadyLoaded = (RelationshipReference)1;
            // ##start C#
            var results = client.Cypher
                .Start(new { r = someRelationshipReferenceAlreadyLoaded })
                .Return(r => r.As<RelationshipInstance>())
                .Results;
            // ##end C#

            // ##start Note
            // You could use Start(new { r = (RelationshipReference)1 }), however we try to avoid passing around integer references like that. You should find the relationship via another query, or via an index, rather than remembering ids.
            // ##end Note
        }

        public void MultipleNodesById()
        {
            // ##start Cypher
            // START n=node(1, 2, 3)
            // RETURN n
            // ##end Cypher

            var client = BuildClient();

            var n1 = (NodeReference)1;
            var n2 = (NodeReference)1;
            var n3 = (NodeReference)1;
            // ##start C#
            var results = client.Cypher
                .Start(new { n = new[] { n1, n2, n3 } })
                .Return(n => n.Node<Person>())
                .Results;
            // ##end C#
        }

        public void AllNodes()
        {
            // ##start Cypher
            // START n=node(*)
            // RETURN n
            // ##end Cypher

            var client = BuildClient();

            // ##start C#
            var results = client.Cypher
                .Start(new { n = All.Nodes })
                .Return(n => n.Node<Person>())
                .Results;
            // ##end C#
        }

        public void NodeByIndexLookup()
        {
            // ##start Cypher
            // START n=node:people(name = 'Bob')
            // RETURN n
            // ##end Cypher

            var client = BuildClient();

            // ##start C#
            var results = client.Cypher
                .Start(new { n = Node.ByIndexLookup("people", "name", "Bob") })
                .Return(n => n.Node<Person>())
                .Results;
            // ##end C#
        }

        public void Example1()
        {
            // ##start Cypher
            // START john=node:node_auto_index(name = 'John')
            // MATCH john-[:friend]->()-[:friend]->fof
            // RETURN john, fof
            // ##end Cypher

            var client = BuildClient();

            // ##start C#
            var results = client.Cypher
                .Start(new {john = Node.ByIndexLookup("node_auto_index", "name", "John")})
                .Match("john-[:friend]->()-[:friend]->fof")
                .Return((john, fof) => new
                {
                    John = john.As<Person>(),
                    FriendOfFriend = fof.As<Person>()
                })
                .Results;
            // ##end C#
        }

        public void Example2()
        {
            // ##start Cypher
            // MATCH n
            // WHERE n.Name = 'B'
            // RETURN n
            // ##end Cypher

            var client = BuildClient();

            // ##start C#
            var results = client.Cypher
                .Match("n")
                .Where<Person>(n => n.Name == "B")
                .Return(n => n.As<Person>())
                .Results;
            // ##end C#
        }

        public void Example3()
        {
            // ##start Cypher
            // MATCH n
            // WHERE n.Name = 'B'
            // RETURN n.Age
            // ##end Cypher

            var client = BuildClient();

            // ##start C#
            var results = client.Cypher
                .Match("n")
                .Where<Person>(n => n.Name == "B")
                .Return(n => n.As<Person>().Age)
                .Results;
            // ##end C#
        }

        public void Example4()
        {
            // ##start Cypher
            // MATCH a-->b
            // WHERE a.Name = 'A'
            // RETURN DISTINCT b
            // ##end Cypher

            var client = BuildClient();

            // ##start C#
            var results = client.Cypher
                .Match("a-->b")
                .Where<Person>(a => a.Name == "A")
                .ReturnDistinct(b => b.As<Person>())
                .Results;
            // ##end C#
        }

        public void Example5()
        {
            // ##start Cypher
            // MATCH n
            // RETURN n
            // SKIP 1
            // LIMIT 2
            // ##end Cypher

            var client = BuildClient();

            // ##start C#
            var results = client.Cypher
                .Match("n")
                .Return(n => n.As<Person>())
                .Skip(1)
                .Limit(2)
                .Results;
            // ##end C#
        }

        public void Example6()
        {
            // ##start Cypher
            // MATCH david--otherPerson-->()
            // WHERE david.name='David'
            // WITH otherPerson, count(*) AS foaf
            // WHERE foaf > 1
            // RETURN otherPerson
            // ##end Cypher

            var client = BuildClient();

            // ##start C#
            var results = client.Cypher
                .Match("david--otherPerson-->()")
                .Where<Person>(david => david.Name == "David")
                .With(otherPerson => new
                {
                    otherPerson,
                    foaf = "count(*)"
                })
                .Where<int>(foaf => foaf > 1)
                .Return(otherPerson => otherPerson.As<Person>())
                .Results;
            // ##end C#
        }

        public void Example7()
        {
            // ##start Cypher
            // MATCH n
            // WITH n
            // ORDER BY n.name DESC
            // LIMIT 3
            // RETURN collect(n)
            // ##end Cypher

            var client = BuildClient();

            // ##start C#
            var results = client.Cypher
                .Match("n")
                .With("n")
                .OrderByDescending("n.name")
                .Limit(3)
                .Return(n => n.CollectAs<Person>())
                .Results;
            // ##end C#
        }

        public void Example8()
        {
            // ##start Cypher
            // MATCH n:Actor
            // RETURN n.Name AS Name
            // UNION ALL
            // MATCH n:Movie
            // RETURN n.Title AS Name
            // ##end Cypher

            var client = BuildClient();

            // ##start C#
            var results = client.Cypher
                .Match("n:Actor")
                .Return(n => n.As<Person>().Name)
                .UnionAll()
                .Match("n:Movie")
                .Return(n => new {
                    Name = n.As<Movie>().Title
                })
                .Results;
            // ##end C#
        }

        class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        class Movie
        {
            public string Title { get; set; }
        }
    }
}
