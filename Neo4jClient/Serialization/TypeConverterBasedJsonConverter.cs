using System;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;

namespace Neo4jClient.Serialization
{
    public class TypeConverterBasedJsonConverter : JsonConverter
    {
        readonly Type[] builtinSupportedTypes = new[]
            {
                typeof(string),
                typeof(Uri),
                typeof(DateTime),
                typeof(DateTime?),
                typeof(DateTimeOffset),
                typeof(DateTimeOffset?),
                typeof(decimal),
                typeof(decimal?),
                typeof(TimeSpan),
                typeof(TimeSpan?),
                typeof(Guid),
                typeof(Guid?)
            };

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
                !objectType.IsPrimitive &&
                builtinSupportedTypes.All(builtinType => objectType != builtinType) &&
                typeConverter.CanConvertTo(typeOfString)
                && typeConverter.CanConvertFrom(typeOfString);
        }
    }
}