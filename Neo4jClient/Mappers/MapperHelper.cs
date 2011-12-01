using System;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using Neo4jClient.Deserializer;

namespace Neo4jClient.Mappers
{
    public static class MapperHelper
    {
        static readonly Regex DateRegex = new Regex(@"/Date\([-]?\d+([+-]\d+)?\)/");

        public static void ConvertAndSetValue<TResult>(this TResult result, string value, PropertyInfo prop)
            where TResult : new()
        {
            var deserializer = new CustomJsonDeserializer();
            try
            {
                var validType = prop.PropertyType;
                if (prop.PropertyType.IsGenericType &&
                    prop.PropertyType.GetGenericTypeDefinition().Equals(typeof (Nullable<>)))
                {
                    var nullableConverter = new NullableConverter(prop.PropertyType);
                    validType = nullableConverter.UnderlyingType;
                }

                if (value == null || value == "null" || string.IsNullOrEmpty(value))
                {
                    prop.SetValue(result, null, null);
                    return;
                }

                object convertedData;
                if (validType.IsEnum)
                {
                    convertedData = Enum.Parse(validType, value, false);
                }
                else if (validType == typeof (DateTimeOffset))
                {
                    var rawValue = value.Replace("NeoDate", "Date");
                    convertedData = DateRegex.IsMatch(rawValue)
                        ? deserializer.Deserialize<DateHolder>(string.Format(@"{{ DateTimeOffset: '{0}'}}", rawValue)).DateTimeOffset
                        : DateTimeOffset.Parse(rawValue);
                }
                else
                {
                    convertedData = Convert.ChangeType(value, validType);
                }
                prop.SetValue(result, convertedData, null);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Could not set property {0} to value {1} for type {2}\n {3}",
                                                  prop.Name,
                                                  value, result.GetType().FullName, ex));
            }
        }
    }

    public class DateHolder
    {
        public DateTimeOffset DateTimeOffset { get; set; }
    }
}
