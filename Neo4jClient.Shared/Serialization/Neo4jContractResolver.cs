using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient.Serialization
{
    public class Neo4jContractResolver : DefaultContractResolver
    {
        private readonly Dictionary<Type, Dictionary<string, string>> _renames;

        public Neo4jContractResolver()
        {
            _renames = new Dictionary<Type, Dictionary<string, string>>();
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            PropertyInfo prop = property.DeclaringType.GetProperty(property.PropertyName);
            if (prop != null && prop.CustomAttributes.Any(a => a.AttributeType == typeof(Neo4jIgnoreAttribute)))
            {
                property.ShouldSerialize = i => false;
            }

            if (IsRenamed(property.DeclaringType, property.PropertyName, out var newJsonPropertyName))
                property.PropertyName = newJsonPropertyName;

            return property;
        }

        private bool IsRenamed(Type type, string jsonPropertyName, out string newJsonPropertyName)
        {
            Dictionary<string, string> renames;

            if (!_renames.TryGetValue(type, out renames) || !renames.TryGetValue(jsonPropertyName, out newJsonPropertyName))
            {
                newJsonPropertyName = null;
                return false;
            }

            return true;
        }
    }
}
