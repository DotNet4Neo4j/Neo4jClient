using System;
using System.Collections.Generic;
using System.Linq;
using Neo4jClient;
using Neo4jClient.Cypher;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Neo4jTestConverters
{
    public class CustomTypeConverterBasedJsonConverter : DateTimeConverterBase
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Console.WriteLine("ReadJson From CustomTypeConverterBasedJsonConverter");

            if (objectType.IsAssignableFrom(typeof(DateTime)))
            {
                return new DateTime(long.Parse(reader.Value.ToString()));
            }

            return DateTime.MinValue;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value.GetType().IsAssignableFrom(typeof(DateTime)))
            {
                writer.WriteValue(((DateTime)value).Ticks);
            }

            Console.WriteLine("WriteJson From CustomTypeConverterBasedJsonConverter");
        }
    }


    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var client = new GraphClient(new Uri("http://localhost:7474/db/data"), "neo4j", "1234"))
            {
                client.JsonConverters.Add(new CustomTypeConverterBasedJsonConverter()); //Prepare new DateTime converter
                client.Connect();

                client.Cypher
                    .Match("n")
                    .DetachDelete("n")
                    .ExecuteWithoutResults();

                var experiments = new[]
                {
                    new TestEntity
                    {
                        Id = Guid.NewGuid(),
                        Created = DateTime.UtcNow,
                    },
                    new TestEntity
                    {
                        Id = Guid.NewGuid(),
                        Created = DateTime.UtcNow,
                    }
                };

                CreateNode(client.Cypher, experiments); //Use CustomTypeConverterBasedJsonConverter for write Json

                var results = client.Cypher // But in that place, CustomTypeConverterBasedJsonConverter not used. Please see CommonDeserializerMethods.CoerceValue method.
                    .Match("(m:TestEntity)")
                    .Return(m => m.As<TestEntity>())
                    .Results
                    .ToList();
            }
        }

        public static void CreateNode<T>(ICypherFluentQuery query, IEnumerable<T> obj)
        {
            query
                .Unwind(obj, "map")
                .Create("(n:" + typeof(T).Name + ")")
                .Set("n=map")
                .ExecuteWithoutResults();
        }

        public class TestEntity
        {
            public Guid Id { get; set; }
            public DateTime Created { get; set; }
        }
    }
}