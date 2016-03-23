using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Neo4jClient.Serialization
{
    public class TypeConverterBasedJsonConverter : JsonConverter
    {
        internal static readonly Type[] BuiltinTypes =
        {
            typeof(string),
            typeof(bool),
            typeof(bool?),
            typeof(byte),
            typeof(byte?),
            typeof(char),
            typeof(char?),
            typeof(double),
            typeof(double?),
            typeof(short),
            typeof(short?),
            typeof(ushort),
            typeof(ushort?),
            typeof(int),
            typeof(int?),
            typeof(uint),
            typeof(uint?),
            typeof(long),
            typeof(long?),
            typeof(ulong),
            typeof(ulong?),
            typeof(SByte),
            typeof(SByte?),
            typeof(Single),
            typeof(Single?),
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
            var result =
                !objectType.GetTypeInfo().IsPrimitive &&
                !BuiltinTypes.Contains(objectType) &&
                typeConverter.GetType() != typeof(TypeConverter) && // Ignore the default one
                typeConverter.CanConvertTo(typeOfString) &&
                typeConverter.CanConvertFrom(typeOfString);
            return result;
        }
    }
}