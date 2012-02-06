using System;

namespace Neo4jClient.Deserializer
{
    internal class TypeMapping
    {
        public Type PropertyTypeToTriggerMapping { get; set; }
        public Func<Type, Type> DetermineTypeToParseJsonIntoBasedOnPropertyType { get; set; }
        public Func<object, object> MutationCallback { get; set; }
    }
}
