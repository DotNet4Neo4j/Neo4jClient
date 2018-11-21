using System;
using System.Reflection;

namespace Neo4jClient.Serialization.BoltDriver
{
    public class EnumValueSerializer : ITypeSerializer
    {
        public bool CanConvert(Type objectType)
        {
            return objectType.GetTypeInfo().IsEnum;
        }

        public object Deserialize(Type objectType, object value)
        {
            return Enum.Parse(objectType, value.ToString());
        }

        public object Serialize(object value)
        {
            return value.ToString();
        }
    }
}