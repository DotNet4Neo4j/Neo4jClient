using System.Collections;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Extensions;
using System.Globalization;

namespace Neo4jClient.Deserializer
{
    public class CustomJsonDeserializer : IDeserializer
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
            return Deserialize<T>(new RestResponse
            {
                Content = content
            });
        }

        public T Deserialize<T>(RestResponse response) where T : new()
        {
            var target = new T();

            response.Content = CommonDeserializerMethods.ReplaceAllDateInstacesWithNeoDates(response.Content);

            if (target is IList)
            {
                var objType = target.GetType();

                if (RootElement.HasValue())
                {
                    var root = FindRoot(response.Content);
                    target = (T)CommonDeserializerMethods.BuildList(objType, root.Children(), Culture, new TypeMapping[0], 0);
                }
                else
                {
                    var json = JArray.Parse(response.Content);
                    target = (T)CommonDeserializerMethods.BuildList(objType, json.Root.Children(), Culture, new TypeMapping[0], 0);
                }
            }
            else if (target is IDictionary)
            {
                var root = FindRoot(response.Content);
                target = (T)CommonDeserializerMethods.BuildDictionary(target.GetType(), root.Children(), Culture, new TypeMapping[0], 0);
            }
            else
            {
                var root = FindRoot(response.Content);
                CommonDeserializerMethods.Map(target, root, Culture, new TypeMapping[0], 0);
            }

            return target;
        }

        JToken FindRoot(string content)
        {
            var json = JObject.Parse(content);
            var root = json.Root;

            if (RootElement.HasValue())
                root = json.SelectToken(RootElement);

            return root;
        }
    }
}