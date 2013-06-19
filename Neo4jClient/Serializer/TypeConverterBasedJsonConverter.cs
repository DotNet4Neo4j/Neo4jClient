using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Neo4jClient.Serializer
{
    public class TypeConverterBasedJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteValue((string) null);
                return;
            }

            var typeConverter = TypeDescriptor.GetConverter(value.GetType());
            writer.WriteValue(typeConverter.ConvertToInvariantString(value));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null) return null;
            var typeConverter = TypeDescriptor.GetConverter(objectType);
            return typeConverter.ConvertFromString(reader.Value.ToString());
        }

        public override bool CanConvert(Type objectType)
        {
            var typeOfString = typeof (string);
            var typeConverter = TypeDescriptor.GetConverter(objectType);
            return
                objectType != typeOfString &&
                !objectType.IsPrimitive &&
                typeConverter.CanConvertTo(typeOfString)
                && typeConverter.CanConvertFrom(typeOfString);
        }
    }
}