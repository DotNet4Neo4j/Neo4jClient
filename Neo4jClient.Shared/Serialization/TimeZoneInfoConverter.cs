using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Neo4jClient.Serialization
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
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(reader.Value.ToString());
            }
            catch
            {
                Debug.WriteLine("Could not deserialize TimeZoneInfo, defaulting to Utc. Ensure the TimeZoneId is valid. Valid TimeZone Ids are:");
                foreach (var timeZone in TimeZoneInfo.GetSystemTimeZones())
                {
                    Debug.WriteLine(timeZone.Id);
                }
                return TimeZoneInfo.Utc;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(TimeZoneInfo) == objectType;
        }
    }
}