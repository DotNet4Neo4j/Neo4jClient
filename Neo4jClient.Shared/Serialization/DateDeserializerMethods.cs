using System;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Neo4jClient.Serialization
{
    class DateDeserializerMethods
    {
        static readonly Regex DateRegex = new Regex(@"/Date\([-]?\d+([+-]\d+)?\)/");
        static readonly Regex DateTypeNameRegex = new Regex(@"(?<=(?<quote>['""])/)Date(?=\(.*?\)/\k<quote>)");

        public static string ReplaceAllDateInstacesWithNeoDates(string content)
        {
            // Replace all /Date(1234+0200)/ instances with /NeoDate(1234+0200)/
            return DateTypeNameRegex.Replace(content, "NeoDate");
        }

        public static DateTimeOffset? ParseDateTimeOffset(JToken value)
        {
            var jValue = value as JValue;
            if (jValue != null)
            {
                if (jValue.Value == null)
                    return null;

                if (jValue.Value is DateTimeOffset)
                    return jValue.Value<DateTimeOffset>();
            }

            var rawValue = value.AsString();

            if (string.IsNullOrWhiteSpace(rawValue))
                return null;

            rawValue = rawValue.Replace("NeoDate", "Date");

            if (!DateRegex.IsMatch(rawValue))
            {
                DateTimeOffset parsed;
                if (!DateTimeOffset.TryParse(rawValue, out parsed))
                    return null;
            }

            var text = string.Format("{{\"a\":\"{0}\"}}", rawValue);
            var reader = new JsonTextReader(new StringReader(text)) {DateParseHandling = DateParseHandling.DateTimeOffset};
            reader.Read(); // JsonToken.StartObject
            reader.Read(); // JsonToken.PropertyName
            return reader.ReadAsDateTimeOffset();
        }

        public static DateTime? ParseDateTime(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return null;

            rawValue = rawValue.Replace("NeoDate", "Date");

            if (!DateRegex.IsMatch(rawValue))
            {
                if (!DateTime.TryParse(rawValue, out DateTime parsed))
                    return null;

                return rawValue.EndsWith("Z", StringComparison.OrdinalIgnoreCase) ? parsed.ToUniversalTime() : parsed;
            }

            var text = $"{{\"a\":\"{rawValue}\"}}";
            var reader = new JsonTextReader(new StringReader(text));
            reader.Read(); // JsonToken.StartObject
            reader.Read(); // JsonToken.PropertyName
            return reader.ReadAsDateTime();
        }

        public static DateTime? ParseDateTime(JToken value)
        {
            var rawValue = value.AsString();
            return ParseDateTime(rawValue);
        }
    }
}
