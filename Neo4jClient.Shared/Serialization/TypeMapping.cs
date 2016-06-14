using System;

namespace Neo4jClient.Serialization
{
    internal class TypeMapping
    {
        public Func<int, Type, bool> ShouldTriggerForPropertyType { get; set; }
        public Func<Type, Type> DetermineTypeToParseJsonIntoBasedOnPropertyType { get; set; }
        public Func<object, object> MutationCallback { get; set; }
    }
}
