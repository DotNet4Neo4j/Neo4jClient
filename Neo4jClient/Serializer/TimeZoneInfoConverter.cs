using System;
using Newtonsoft.Json;

namespace Neo4jClient.Serializer
{
    public class TimeZoneInfoConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var timeZone = (TimeZoneInfo) value;
            writer.WriteValue(timeZone.Id);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(reader.Value.ToString());
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(TimeZoneInfo) == objectType;
        }
    }
}