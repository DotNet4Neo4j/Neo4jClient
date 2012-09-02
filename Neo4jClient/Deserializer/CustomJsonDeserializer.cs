using System.Collections;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace Neo4jClient.Deserializer
{
    public class CustomJsonDeserializer
    {
        readonly CultureInfo culture = CultureInfo.InvariantCulture;

        public T Deserialize<T>(string content) where T : new()
        {
            var target = new T();

            content = CommonDeserializerMethods.ReplaceAllDateInstacesWithNeoDates(content);

            if (target is IList)
            {
                var objType = target.GetType();

                var json = JArray.Parse(content);
                target = (T)CommonDeserializerMethods.BuildList(objType, json.Root.Children(), culture, new TypeMapping[0], 0);
            }
            else if (target is IDictionary)
            {
                var root = FindRoot(content);
                target = (T)CommonDeserializerMethods.BuildDictionary(target.GetType(), root.Children(), culture, new TypeMapping[0], 0);
            }

            else
            {
                var root = FindRoot(content);
                CommonDeserializerMethods.Map(target, root, culture, new TypeMapping[0], 0);
            }

            return target;
        }

        JToken FindRoot(string content)
        {
            var json = JObjectCustom.Parse(content);
            var root = json.Root;

            return root;
        }
    }
}