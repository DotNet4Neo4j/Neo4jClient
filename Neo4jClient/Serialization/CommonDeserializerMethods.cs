using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient.Serialization
{
    class CommonDeserializerMethods
    {
        static readonly Regex DateRegex = new Regex(@"/Date\([-]?\d+([+-]\d+)?\)/");
        static readonly Regex DateTypeNameRegex = new Regex(@"(?<=(?<quote>['""])/)Date(?=\(.*?\)/\k<quote>)");

        public static string RemoveResultsFromJson(string content)
        {
            var root = JToken.Parse(content);
            var errors = root.Value<JToken>("errors");
            if (errors != null && errors.HasValues)
            {
                throw DeserializeNeo4jError(errors);
            }
            var output = root.SelectTokens("$.results[0]").Select(j => j.ToString(Formatting.None)).FirstOrDefault();
            return output;
        }

        public static NeoException DeserializeNeo4jError(JToken error) =>
            new NeoException(new ApiModels.ExceptionResponse
            {
                Exception = error.First?.Value<JToken>("code")?.ToString(),
                Message = error.First?.Value<JToken>("message")?.ToString(),
            });

        public static string ReplaceAllDateInstancesWithNeoDates(string content)
        {
            // Replace all /Date(1234+0200)/ instances with /NeoDate(1234+0200)/
            return DateTypeNameRegex.Replace(content, "NeoDate");
        }

        public static DateTimeOffset? ParseDateTimeOffset(JToken value)
        {
            if (value is JValue jValue)
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
                if (!DateTimeOffset.TryParse(rawValue, out _))
                    return null;
            }

            var text = $"{{\"a\":\"{rawValue}\"}}";
            var reader = new JsonTextReader(new StringReader(text)) {DateParseHandling = DateParseHandling.DateTimeOffset};
            reader.Read(); // JsonToken.StartObject
            reader.Read(); // JsonToken.PropertyName
            return reader.ReadAsDateTimeOffset();
        }

        public static DateTime? ParseDateTime(JToken value)
        {
            var rawValue = value.AsString();

            if (string.IsNullOrWhiteSpace(rawValue))
                return null;

            rawValue = rawValue.Replace("NeoDate", "Date");

            if (!DateRegex.IsMatch(rawValue))
            {
                if (!DateTime.TryParse(rawValue, out var parsed))
                    return null;

                return rawValue.EndsWith("Z", StringComparison.OrdinalIgnoreCase) ? parsed.ToUniversalTime() : parsed;
            }

            var text = $"{{\"a\":\"{rawValue}\"}}";
            var reader = new JsonTextReader(new StringReader(text));
            reader.Read(); // JsonToken.StartObject
            reader.Read(); // JsonToken.PropertyName
            return reader.ReadAsDateTime();
        }

        public static object CoerceValue(DeserializationContext context, PropertyInfo propertyInfo, JToken value, IEnumerable<TypeMapping> typeMappings, int nestingLevel)
        {
            if (value == null || value.Type == JTokenType.Null)
                return null;

            var propertyType = propertyInfo.PropertyType;
            var typeInfo = propertyType.GetTypeInfo();
            if (TryJsonConverters(context, propertyType, value, out var jsonConversionResult))
                return jsonConversionResult;
            
            Type genericTypeDef = null;

            if (typeInfo.IsGenericType)
            {
                genericTypeDef = propertyType.GetGenericTypeDefinition();

                if (genericTypeDef == typeof(Nullable<>))
                {
                    propertyType = propertyType.GetGenericArguments()[0];
                    genericTypeDef = null;
                }
            }

            typeMappings = typeMappings.ToArray();
            if (typeInfo.IsPrimitive)
            {
                // no primitives can contain quotes so we can safely remove them
                // allows converting a json value like {"index": "1"} to an int
                object tmpVal = value.AsString().Replace("\"", string.Empty);
                tmpVal = Convert.ChangeType(tmpVal, propertyType);
                return tmpVal;
            }

            if (typeInfo.IsEnum)
            {
                var raw = value.AsString();
                var converted = Enum.Parse(propertyType, raw, false);
                return converted;
            }

            if (propertyType == typeof(Uri))
            {
                var raw = value.AsString();
                var uri = new Uri(raw, UriKind.RelativeOrAbsolute);
                return uri;
            }

            if (propertyType == typeof(string))
            {
                var raw = value.AsString();
                return raw;
            }

            if (propertyType == typeof(DateTime))
            {
                return ParseDateTime(value);
            }

            if (propertyType == typeof(DateTimeOffset))
            {
                var dateTimeOffset = ParseDateTimeOffset(value);
                return dateTimeOffset;
            }

            if (propertyType == typeof(Decimal))
            {
                var dec = Convert.ToDecimal(((JValue) value).Value);
                return dec;
            }

            if (propertyType == typeof(TimeSpan))
            {
                var valueString = value.ToString();
                var timeSpan = TimeSpan.Parse(valueString);
                return timeSpan;
            }

            if (propertyType == typeof(Guid))
            {
                var raw = value.AsString();
                var guid = string.IsNullOrEmpty(raw) ? Guid.Empty : new Guid(raw);
                return guid;
            }

            if (propertyType == typeof(byte[]))
            {
                return Convert.FromBase64String(value.Value<string>());
            }

            if (genericTypeDef == typeof(List<>))
            {
                var list = BuildList(context, propertyType, value.Children(), typeMappings, nestingLevel + 1);
                return list;
            }

            if (genericTypeDef == typeof(Dictionary<,>))
            {
                var keyType = propertyType.GetGenericArguments()[0];

                // only supports Dict<string, T>()
                if (keyType != typeof (string))
                {
                    throw new NotSupportedException("Value coercion only supports dictionaries with a key of type System.String");
                }

                var dict = BuildDictionary(context, propertyType, value.Children(), typeMappings, nestingLevel + 1);
                return dict;
            }

            // nested objects
            var mapping = typeMappings.FirstOrDefault(m => m.ShouldTriggerForPropertyType(nestingLevel, propertyType));
            var item = mapping != null ? MutateObject(context, value, typeMappings, nestingLevel, mapping, propertyType) : CreateAndMap(context, propertyType, value, typeMappings, nestingLevel + 1);
            return item;
        }

        public static void SetPropertyValue(DeserializationContext context, object targetObject, PropertyInfo propertyInfo, JToken value, IEnumerable<TypeMapping> typeMappings, int nestingLevel)
        {
            if (value == null || value.Type == JTokenType.Null)
                return;

            var coercedValue = CoerceValue(context, propertyInfo, value, typeMappings, nestingLevel);
            propertyInfo.SetValue(targetObject, coercedValue, null);
        }

        public static object CreateAndMap(DeserializationContext context, Type type, JToken element, IEnumerable<TypeMapping> typeMappings, int nestingLevel)
        {
            if (element.Type == JTokenType.Null)
                return null;

            object instance;
            typeMappings = typeMappings.ToArray();

            Type genericTypeDefinition = null;
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericType)
            {
                genericTypeDefinition = type.GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof (Nullable<>))
                {
                    type = type.GetGenericArguments()[0];
                    genericTypeDefinition = null;
                }
            }

            if (genericTypeDefinition != null)
            {
                if (genericTypeDefinition == typeof(Dictionary<,>))
                {
                    instance = BuildDictionary(context, type, element.Children(), typeMappings, nestingLevel + 1);
                }
                else if (genericTypeDefinition == typeof(List<>))
                {
                    instance = BuildList(context, type, element.Children(), typeMappings, nestingLevel + 1);
                }
                else if (genericTypeDefinition == typeof(IEnumerable<>))
                {
                    instance = BuildIEnumerable(context, type, element.Children(), typeMappings, nestingLevel + 1);
                }
                else if (type == typeof (string))
                {
                    instance = (string) element;
                }
                else
                {
                    var mapping = typeMappings.FirstOrDefault(m => m.ShouldTriggerForPropertyType(nestingLevel, type));
                    if (mapping != null)
                        instance = MutateObject(context, element, typeMappings, nestingLevel, mapping, type);
                    else
                    {
                        instance = Activator.CreateInstance(type);
                        Map(context, instance, element, typeMappings, nestingLevel);
                    }
                }
            }
            else if (type == typeof(byte[]))
            {
                instance = Convert.FromBase64String(element.Value<string>());
            }
            else if (typeInfo.BaseType == typeof(Array)) //One Dimensional Only
            {
                var underlyingType = type.GetElementType();
                var arrayType = typeof(ArrayList);
                instance = BuildArray(context, arrayType, underlyingType, element.Children(), typeMappings, nestingLevel + 1);
            }
            else if (type == typeof(string))
            {
                instance = element.ToString();
            }
            else if (TryJsonConverters(context, type, element, out instance))
            {
            }
            else if (typeInfo.IsValueType)
            {
                if (type == typeof(Guid))
                {
                    instance = Guid.Parse(element.ToString());
                }
                else if (typeInfo.BaseType == typeof(Enum))
                {
                    instance = Enum.Parse(type, element.ToString(), false);
                }
                else
                {
                    instance = Convert.ChangeType(element.ToString(), type);
                }
            }
            else if (type == typeof(object) && element is JValue jValue)
            {
                instance = jValue.Value;
            }
            else
            {
                try
                {
                    instance = Activator.CreateInstance(type);
                }
                catch (MissingMethodException ex)
                {
                    throw new DeserializationException(
                        $"We expected a default public constructor on {type.Name} so that we could create instances of it to deserialize data into, however this constructor does not exist or is inaccessible.",
                        ex);
                }
                Map(context, instance, element, typeMappings, nestingLevel);
            }
            return instance;
        }

        static bool TryJsonConverters(DeserializationContext context, Type type, JToken element, out object instance)
        {
            instance = null;
            var converter = context.JsonConverters?.FirstOrDefault(c => c.CanConvert(type) && c.CanRead);
            if (converter == null) return false;
            using (var reader = element.CreateReader())
            {
                reader.Read();
                instance = converter.ReadJson(reader, type, null, null);
                return true;
            }
        }
        static object MutateObject(DeserializationContext context, JToken value, IEnumerable<TypeMapping> typeMappings, int nestingLevel,
                                   TypeMapping mapping, Type propertyType)
        {
            var newType = mapping.DetermineTypeToParseJsonIntoBasedOnPropertyType(propertyType);
            var rawItem = CreateAndMap(context, newType, value, typeMappings, nestingLevel + 1);
            var item = mapping.MutationCallback(rawItem);
            return item;
        }

        public static Dictionary<string, PropertyInfo> ApplyPropertyCasing(DeserializationContext context, Dictionary<string, PropertyInfo> properties)
        {
            if (context.JsonContractResolver is CamelCasePropertyNamesContractResolver)
            {
                var camel = new Func<string, string>(name => string.Format("{0}{1}", name.Substring(0,1).ToLowerInvariant(), name.Substring(1, name.Length-1)));
                return properties.Select(x => new { Key = camel(x.Key), x.Value }).ToDictionary(x => x.Key, x => x.Value);    
            }
            return properties;
        } 

        public static void Map(DeserializationContext context, object targetObject, JToken parentJsonToken, IEnumerable<TypeMapping> typeMappings, int nestingLevel)
        {
            typeMappings = typeMappings.ToArray();
            var objType = targetObject.GetType();
            var props = GetPropertiesForType(context, objType);
            IDictionary<string, JToken> dictionary = parentJsonToken as JObject;
            if (dictionary != null && props.Keys.All(dictionary.ContainsKey) == false && dictionary.ContainsKey("data")) {
               parentJsonToken = parentJsonToken["data"];
            }

            foreach (var propertyName in props.Keys)
            {
                var propertyInfo = props[propertyName];
                JToken jsonToken;
                try
                {
                    jsonToken = parentJsonToken[propertyName];
                }
                catch (InvalidOperationException ex)
                {
                    throw new InvalidOperationException(string.Format("While trying to map some JSON into an object of type {0}, we failed to find an expected property ({1}) in the JSON at path {2}.\r\n\r\nThe JSON block for this token was:\r\n\r\n{3}",
                        objType.FullName,
                        propertyName,
                        parentJsonToken.Path,
                        parentJsonToken),
                        ex);
                }
                SetPropertyValue(context, targetObject, propertyInfo, jsonToken, typeMappings, nestingLevel);
            }
        }

        public static IDictionary BuildDictionary(DeserializationContext context, Type type, JEnumerable<JToken> elements, IEnumerable<TypeMapping> typeMappings, int nestingLevel)
        {
            typeMappings = typeMappings.ToArray();
            var dict = (IDictionary)Activator.CreateInstance(type);
            var valueType = type.GetGenericArguments()[1];
            foreach (var jToken in elements)
            {
                var child = (JProperty) jToken;
                var key = child.Name;
                var item = CreateAndMap(context, valueType, child.Value, typeMappings, nestingLevel + 1);
                dict.Add(key, item);
            }

            return dict;
        }

        public static IList BuildList(DeserializationContext context, Type type, JEnumerable<JToken> elements, IEnumerable<TypeMapping> typeMappings, int nestingLevel)
        {
            typeMappings = typeMappings.ToArray();
            var list = (IList)Activator.CreateInstance(type);

            var itemType = type
                .GetInterfaces()
                .Where(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>))
                .Select(i => i.GetGenericArguments().First())
                .Single();

            foreach (var element in elements)
            {
                if (itemType.GetTypeInfo().IsPrimitive)
                {
                    var value = element as JValue;
                    if (value != null)
                    {
                        list.Add(Convert.ChangeType(value.Value, itemType));
                    }
                }
                else if (itemType == typeof(string))
                {
                    list.Add(element.AsString());
                }
                else
                {
                    var item = CreateAndMap(context, itemType, element, typeMappings, nestingLevel + 1);
                    list.Add(item);
                }
            }
            return list;
        }

        public static Array BuildArray(DeserializationContext context, Type type, Type itemType,  JEnumerable<JToken> elements, IEnumerable<TypeMapping> typeMappings, int nestingLevel)
        {
            typeMappings = typeMappings.ToArray();
            var list = (ArrayList)Activator.CreateInstance(type);

            foreach (var element in elements)
            {
                if (itemType.GetTypeInfo().IsPrimitive)
                {
                    var value = element as JValue;
                    if (value != null)
                    {
                        list.Add(Convert.ChangeType(value.Value, itemType));
                    }
                }
                else if (itemType == typeof(string))
                {
                    list.Add(element.AsString());
                }
                else
                {
                    var item = CreateAndMap(context, itemType, element, typeMappings, nestingLevel + 1);
                    list.Add(item);
                }
            }
            return list.ToArray(itemType);
        }

        public static IList BuildIEnumerable(DeserializationContext context, Type type, JEnumerable<JToken> elements, IEnumerable<TypeMapping> typeMappings, int nestingLevel)
        {
            typeMappings = typeMappings.ToArray();
            var itemType = type.GetGenericArguments().Single();
            var listType = typeof(List<>).MakeGenericType(itemType);
            var list = (IList)Activator.CreateInstance(listType);

            foreach (var element in elements)
            {
                if (itemType.GetTypeInfo().IsPrimitive)
                {
                    var value = element as JValue;
                    if (value != null)
                    {
                        list.Add(Convert.ChangeType(value.Value, itemType));
                    }
                }
                else if (itemType == typeof (string))
                {
                    list.Add(element.AsString());
                }
                else
                {
                    var item = CreateAndMap(context, itemType, element, typeMappings, nestingLevel + 1);
                    list.Add(item);
                }
            }

            return list;
        }

        static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> PropertyInfoCache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();
        static readonly object PropertyInfoCacheLock = new object();
        static Dictionary<string, PropertyInfo> GetPropertiesForType(DeserializationContext context, Type objType)
        {
            Dictionary<string, PropertyInfo> result;
            if (PropertyInfoCache.TryGetValue(objType, out result))
                return result;

            lock (PropertyInfoCacheLock)
            {
                if (PropertyInfoCache.TryGetValue(objType, out result))
                    return result;

                var camelCase = (context.JsonContractResolver is CamelCasePropertyNamesContractResolver);
                var camel = new Func<string, string>(name => string.Format("{0}{1}", name.Substring(0, 1).ToLowerInvariant(), name.Length > 1 ? name.Substring(1, name.Length - 1) : string.Empty));

                var properties = objType
                    .GetProperties()
                    .Where(p => p.CanWrite)
                    .Select(p =>
                    {
                        var attributes =
                            (JsonPropertyAttribute[])p.GetCustomAttributes(typeof(JsonPropertyAttribute), true);
                        return new
                        {
                            Name = attributes.Any() && attributes.Single().PropertyName != null ? attributes.Single().PropertyName : camelCase ? camel(p.Name) : p.Name, //only camelcase if json property doesn't exist
                            Property = p
                        };
                    });

                return properties.ToDictionary(p => p.Name, p => p.Property);
            }
        }
    }
}
