using System;
using System.Reflection;
using Newtonsoft.Json;

namespace Neo4jClient.Serialization
{
    public class NullableEnumValueConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (string.IsNullOrEmpty(reader.Value.ToString()))
                return null;

            var enumType = Nullable.GetUnderlyingType(objectType);
            return Enum.Parse(enumType, reader.Value.ToString());
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.GetTypeInfo().IsGenericType &&
                   objectType.GetGenericTypeDefinition() == typeof (Nullable<>) &&
                   Nullable.GetUnderlyingType(objectType).GetTypeInfo().IsEnum;
        }
    }
}