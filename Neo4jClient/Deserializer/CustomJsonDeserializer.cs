using System.Collections;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace Neo4jClient.Deserializer
{
    public class CustomJsonDeserializer
    {
        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string DateFormat { get; set; }
        public CultureInfo Culture { get; set; }

        public CustomJsonDeserializer()
        {
            Culture = CultureInfo.InvariantCulture;
        }

        public T Deserialize<T>(string content) where T : new()
        {
            var target = new T();

            content = CommonDeserializerMethods.ReplaceAllDateInstacesWithNeoDates(content);

            if (target is IList)
            {
                var objType = target.GetType();

                if (!string.IsNullOrEmpty(RootElement))
                {
                    var root = FindRoot(content);
                    target = (T)CommonDeserializerMethods.BuildList(objType, root.Children(), Culture, new TypeMapping[0], 0);
                }
                else
                {
                    var json = JArray.Parse(content);
                    target = (T)CommonDeserializerMethods.BuildList(objType, json.Root.Children(), Culture, new TypeMapping[0], 0);
                }
            }
            else if (target is IDictionary)
            {
                var root = FindRoot(content);
                target = (T)CommonDeserializerMethods.BuildDictionary(target.GetType(), root.Children(), Culture, new TypeMapping[0], 0);
            }
            else
            {
                var root = FindRoot(content);
                CommonDeserializerMethods.Map(target, root, Culture, new TypeMapping[0], 0);
            }

            return target;
        }

        JToken FindRoot(string content)
        {
            var json = JObject.Parse(content);
            var root = json.Root;

            if (!string.IsNullOrEmpty(RootElement))
                root = json.SelectToken(RootElement);

            return root;
        }
    }
}