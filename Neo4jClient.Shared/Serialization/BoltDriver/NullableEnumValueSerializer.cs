using System;
using System.Reflection;

namespace Neo4jClient.Serialization.BoltDriver
{
    public class NullableEnumValueSerializer : ITypeSerializer
    {
        public bool CanConvert(Type objectType)
        {
            return objectType.GetTypeInfo().IsGenericType &&
                   objectType.GetGenericTypeDefinition() == typeof (Nullable<>) &&
                   Nullable.GetUnderlyingType(objectType).GetTypeInfo().IsEnum;
        }

        public object Deserialize(Type objectType, object value)
        {
            if (string.IsNullOrEmpty(value.ToString()))
                return null;

            var enumType = Nullable.GetUnderlyingType(objectType);
            return Enum.Parse(enumType, value.ToString());
        }

        public object Serialize(object value)
        {
            return value.ToString();
        }
    }
}