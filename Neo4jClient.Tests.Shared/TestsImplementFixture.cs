using System;
using System.Linq;
using System.Reflection;
using Neo4jClient.Test.Fixtures;
using Xunit;

namespace Neo4jClient.Test
{
    public class TestsImplementFixture : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public void AllClassesImplementIClassFixture()
        {
            var typesWithFactsNotImplementingIClassFixture =
                //Get this assembly and it's types.
                Assembly.GetExecutingAssembly().GetTypes()
                //Get the types with their interfaces and methods
                .Select(type => new {type, interfaces = type.GetInterfaces(), methods = type.GetMethods()}) 
                //First we only want types where they have a method which is a 'Fact'
                .Where(t => t.methods.Select(Attribute.GetCustomAttributes).Any(attributes => attributes.Any(a => a.GetType() == typeof(FactAttribute))))
                //Then check if that type implements the type 
                .Where(t => t.interfaces.All(i => i != typeof(IClassFixture<CultureInfoSetupFixture>)))
                //Select the name
                .Select(t => t.type.FullName)
                //ToList it to avoid multiple enumeration
                .ToList();

            if (typesWithFactsNotImplementingIClassFixture.Any())
                throw new InvalidOperationException($"All test classes must implement {nameof(IClassFixture<CultureInfoSetupFixture>)}{Environment.NewLine}These don't:{Environment.NewLine} * {string.Join($"{Environment.NewLine} * ", typesWithFactsNotImplementingIClassFixture)}");
        }
    }
}