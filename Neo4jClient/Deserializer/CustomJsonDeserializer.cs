using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Extensions;
using System.Globalization;

namespace Neo4jClient.Deserializer
{
    public class CustomJsonDeserializer : IDeserializer
    {
        static readonly Regex DateRegex = new Regex(@"/Date\(\d+[+-]\d+\)/");
        static readonly Regex DateTypeNameRegex = new Regex(@"(?<=(?<quote>['""])/)Date(?=\(.*?\)/\k<quote>)");

        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string DateFormat { get; set; }
        public CultureInfo Culture { get; set; }

        public CustomJsonDeserializer()
        {
            Culture = CultureInfo.InvariantCulture;
        }

        public T Deserialize<T>(RestResponse response) where T : new()
        {
            var target = new T();

            // Replace all /Date(1234+0200)/ instances with /NeoDate(1234+0200)/
            response.Content = DateTypeNameRegex.Replace(response.Content, "NeoDate");

            if (target is IList)
            {
                var objType = target.GetType();

                if (RootElement.HasValue())
                {
                    var root = FindRoot(response.Content);
                    target = (T)BuildList(objType, root.Children());
                }
                else
                {
                    var json = JArray.Parse(response.Content);
                    target = (T)BuildList(objType, json.Root.Children());
                }
            }
            else if (target is IDictionary)
            {
                var root = FindRoot(response.Content);
                target = (T)BuildDictionary(target.GetType(), root.Children());
            }
            else
            {
                var root = FindRoot(response.Content);
                Map(target, root);
            }

            return target;
        }

        private JToken FindRoot(string content)
        {
            var json = JObject.Parse(content);
            var root = json.Root;

            if (RootElement.HasValue())
                root = json.SelectToken(RootElement);

            return root;
        }

        private void Map(object x, JToken json)
        {
            var objType = x.GetType();
            var props = objType.GetProperties().Where(p => p.CanWrite).ToList();

            foreach (var prop in props)
            {
                var type = prop.PropertyType;

                var name = prop.Name;
                var value = json[name];
                var actualName = name;

                if (value == null)
                {
                    // try camel cased name
                    actualName = name.ToCamelCase(Culture);
                    value = json[actualName];
                }

                if (value == null)
                {
                    // try lower cased name
                    actualName = name.ToLower();
                    value = json[actualName];
                }

                if (value == null)
                {
                    // try name with underscores
                    actualName = name.AddUnderscores();
                    value = json[actualName];
                }

                if (value == null)
                {
                    // try name with underscores with lower case
                    actualName = name.AddUnderscores().ToLower();
                    value = json[actualName];
                }

                if (value == null)
                {
                    // try name with dashes
                    actualName = name.AddDashes();
                    value = json[actualName];
                }

                if (value == null)
                {
                    // try name with dashes with lower case
                    actualName = name.AddDashes().ToLower();
                    value = json[actualName];
                }

                if (value == null || value.Type == JTokenType.Null)
                {
                    continue;
                }

                // check for nullable and extract underlying type
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    type = type.GetGenericArguments()[0];
                }

                if (type.IsPrimitive)
                {
                    // no primitives can contain quotes so we can safely remove them
                    // allows converting a json value like {"index": "1"} to an int
                    var tmpVal = value.AsString().Replace("\"", string.Empty);
                    prop.SetValue(x, tmpVal.ChangeType(type), null);
                }
                else if (type.IsEnum)
                {
                    var raw = value.AsString();
                    var converted = Enum.Parse(type, raw, false);
                    prop.SetValue(x, converted, null);
                }
                else if (type == typeof(Uri))
                {
                    var raw = value.AsString();
                    var uri = new Uri(raw, UriKind.RelativeOrAbsolute);
                    prop.SetValue(x, uri, null);
                }
                else if (type == typeof(string))
                {
                    var raw = value.AsString();
                    prop.SetValue(x, raw, null);
                }
                else if (type == typeof(DateTime))
                {
                    throw new NotSupportedException("DateTime values are not supported. Use DateTimeOffset instead.");
                }
                else if (type == typeof(DateTimeOffset))
                {
                    var dateTimeOffset = ParseDateTimeOffset(value);
                    if (dateTimeOffset.HasValue)
                        prop.SetValue(x, dateTimeOffset.Value, null);
                }
                else if (type == typeof(Decimal))
                {
                    var dec = Decimal.Parse(value.AsString(), Culture);
                    prop.SetValue(x, dec, null);
                }
                else if (type == typeof(Guid))
                {
                    var raw = value.AsString();
                    var guid = string.IsNullOrEmpty(raw) ? Guid.Empty : new Guid(raw);
                    prop.SetValue(x, guid, null);
                }
                else if (type.IsGenericType)
                {
                    var genericTypeDef = type.GetGenericTypeDefinition();
                    if (genericTypeDef == typeof(List<>))
                    {
                        var list = BuildList(type, value.Children());
                        prop.SetValue(x, list, null);
                    }
                    else if (genericTypeDef == typeof(Dictionary<,>))
                    {
                        var keyType = type.GetGenericArguments()[0];

                        // only supports Dict<string, T>()
                        if (keyType == typeof(string))
                        {
                            var dict = BuildDictionary(type, value.Children());
                            prop.SetValue(x, dict, null);
                        }
                    }
                    else
                    {
                        // nested property classes
                        var item = CreateAndMap(type, json[actualName]);
                        prop.SetValue(x, item, null);
                    }
                }
                else
                {
                    // nested property classes
                    var item = CreateAndMap(type, json[actualName]);
                    prop.SetValue(x, item, null);
                }
            }
        }

        static DateTimeOffset? ParseDateTimeOffset(JToken value)
        {
            var rawValue = value.AsString();

            if (string.IsNullOrWhiteSpace(rawValue))
                return null;

            rawValue = rawValue.Replace("NeoDate", "Date");
            if (!DateRegex.IsMatch(rawValue))
                return null;

            var text = string.Format("{{\"a\":\"{0}\"}}", rawValue);
            var reader = new JsonTextReader(new StringReader(text));
            reader.Read(); // JsonToken.StartObject
            reader.Read(); // JsonToken.PropertyName
            return reader.ReadAsDateTimeOffset();
        }

        private object CreateAndMap(Type type, JToken element)
        {
            object instance;
            if (type.IsGenericType)
            {
                var genericTypeDef = type.GetGenericTypeDefinition();
                if (genericTypeDef == typeof(Dictionary<,>))
                {
                    instance = BuildDictionary(type, element.Children());
                }
                else if (genericTypeDef == typeof(List<>))
                {
                    instance = BuildList(type, element.Children());
                }
                else if (type == typeof(string))
                {
                    instance = (string)element;
                }
                else
                {
                    instance = Activator.CreateInstance(type);
                    Map(instance, element);
                }
            }
            else if (type == typeof(string))
            {
                instance = (string)element;
            }
            else
            {
                instance = Activator.CreateInstance(type);
                Map(instance, element);
            }
            return instance;
        }

        private IDictionary BuildDictionary(Type type, JEnumerable<JToken> elements)
        {
            var dict = (IDictionary)Activator.CreateInstance(type);
            var valueType = type.GetGenericArguments()[1];
            foreach (JProperty child in elements)
            {
                var key = child.Name;
                var item = CreateAndMap(valueType, child.Value);
                dict.Add(key, item);
            }

            return dict;
        }

        private IList BuildList(Type type, JEnumerable<JToken> elements)
        {
            var list = (IList)Activator.CreateInstance(type);
            var itemType = type.GetGenericArguments()[0];

            foreach (var element in elements)
            {
                if (itemType.IsPrimitive)
                {
                    var value = element as JValue;
                    if (value != null)
                    {
                        list.Add(value.Value.ChangeType(itemType));
                    }
                }
                else if (itemType == typeof(string))
                {
                    list.Add(element.AsString());
                }
                else
                {
                    var item = CreateAndMap(itemType, element);
                    list.Add(item);
                }
            }
            return list;
        }
    }
}