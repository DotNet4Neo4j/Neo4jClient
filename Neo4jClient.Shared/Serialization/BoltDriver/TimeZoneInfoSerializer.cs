using System;
using System.Diagnostics;

namespace Neo4jClient.Serialization.Json
{
    public class TimeZoneInfoSerializer : ITypeSerializer
    {
        public bool CanConvert(Type objectType)
        {
            return typeof(TimeZoneInfo) == objectType;
        }

        public object Deserialize(Type objectType, object value)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(value.ToString());
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

        public object Serialize(object value)
        {
            var timeZone = (TimeZoneInfo)value;
            return timeZone.Id;
        }
    }
}