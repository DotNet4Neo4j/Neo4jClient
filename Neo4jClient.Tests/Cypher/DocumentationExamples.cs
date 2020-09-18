using System.Threading.Tasks;
using Neo4jClient.Cypher;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedVariable

namespace Neo4jClient.Tests.Cypher
{
    public class DocumentationExamples
    {
        static IGraphClient BuildClient()
        {
            return null;
        }

        public async Task Example2()
        {
            // ##start Cypher
            // MATCH n
            // WHERE n.Name = 'B'
            // RETURN n
            // ##end Cypher

            var client = BuildClient();

            // ##start C#
            var results = await client.Cypher
                .Match("n")
                .Where<Person>(n => n.Name == "B")
                .Return(n => n.As<Person>())
                .ResultsAsync;
            // ##end C#
        }

        public async Task Example3()
        {
            // ##start Cypher
            // MATCH n
            // WHERE n.Name = 'B'
            // RETURN n.Age
            // ##end Cypher

            var client = BuildClient();

            // ##start C#
            var results = await client.Cypher
                .Match("n")
                .Where<Person>(n => n.Name == "B")
                .Return(n => n.As<Person>().Age)
                .ResultsAsync;
            // ##end C#
        }

        public async Task Example4()
        {
            // ##start Cypher
            // MATCH a-->b
            // WHERE a.Name = 'A'
            // RETURN DISTINCT b
            // ##end Cypher

            var client = BuildClient();

            // ##start C#
            var results = await client.Cypher
                .Match("a-->b")
                .Where<Person>(a => a.Name == "A")
                .ReturnDistinct(b => b.As<Person>())
                .ResultsAsync;
            // ##end C#
        }

        public async Task Example5()
        {
            // ##start Cypher
            // MATCH n
            // RETURN n
            // SKIP 1
            // LIMIT 2
            // ##end Cypher

            var client = BuildClient();

            // ##start C#
            var results = await client.Cypher
                .Match("n")
                .Return(n => n.As<Person>())
                .Skip(1)
                .Limit(2)
                .ResultsAsync;
            // ##end C#
        }

        public async Task Example6()
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
            var results = await client.Cypher
                .Match("david--otherPerson-->()")
                .Where<Person>(david => david.Name == "David")
                .With(otherPerson => new
                {
                    otherPerson,
                    foaf = "count(*)"
                })
                .Where<int>(foaf => foaf > 1)
                .Return(otherPerson => otherPerson.As<Person>())
                .ResultsAsync;
            // ##end C#
        }

        public async Task Example7()
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
            var results = await client.Cypher
                .Match("n")
                .With("n")
                .OrderByDescending("n.name")
                .Limit(3)
                .Return(n => n.CollectAs<Person>())
                .ResultsAsync;
            // ##end C#
        }

        public async Task Example8()
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
            var results = await client.Cypher
                .Match("n:Actor")
                .Return(n => n.As<Person>().Name)
                .UnionAll()
                .Match("n:Movie")
                .Return(n => new {
                    Name = n.As<Movie>().Title
                })
                .ResultsAsync;
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
