using System;
using System.ComponentModel;
using System.Diagnostics;
using Neo4j.Driver.V1;
using Newtonsoft.Json;

namespace Neo4jClient.Serialization
{
    public class ZonedDateTimeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var zdt = (ZonedDateTime)value;
            zdt.As<DateTimeOffset>();

            var typeConverter = TypeDescriptor.GetConverter(zdt.GetType());
            writer.WriteValue(typeConverter.ConvertToInvariantString(value));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null) return null;
            var typeConverter = TypeDescriptor.GetConverter(typeof(DateTimeOffset));
            var dto = (DateTimeOffset) typeConverter.ConvertFromString(reader.Value.ToString());
            return new ZonedDateTime(dto);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(ZonedDateTime) == objectType;
        }
    }


    public class LocalDateTimeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var ldt = (LocalDateTime)value;
            ldt.As<DateTime>();

            var typeConverter = TypeDescriptor.GetConverter(ldt.GetType());
            writer.WriteValue(typeConverter.ConvertToInvariantString(value));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null) return null;
            var typeConverter = TypeDescriptor.GetConverter(typeof(DateTime));
            var dt = typeConverter.ConvertFromString(reader.Value.ToString()) as DateTime?;
            return new LocalDateTime(dt ?? DateTime.MinValue);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(LocalDateTime) == objectType;
        }
    }
}