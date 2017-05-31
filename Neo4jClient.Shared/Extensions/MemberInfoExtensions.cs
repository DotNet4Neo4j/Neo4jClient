using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Neo4jClient.Extensions
{
    public static class MemberInfoExtensions
    {
        /// <summary>Gets the name of the property, if a <see cref="JsonPropertyAttribute"/> is attached, it will attempt to use that first, then if a <see cref="JsonObjectAttribute"/> is attached it will use the <see cref="NamingStrategy"/> next.</summary>
        /// <param name="info">The <see cref="MemberInfo"/> to get the name from</param>
        /// <returns>The name of the property, if a <see cref="JsonPropertyAttribute"/> is attached, it will use that first, then if a <see cref="JsonObjectAttribute"/> is attached it will use the <see cref="NamingStrategy"/> next.</returns>
        internal static string GetNameUsingJsonProperty(this MemberInfo info)
        {
            var jsonPropertyAttribute = info.GetCustomAttributes(typeof (JsonPropertyAttribute)).FirstOrDefault() as JsonPropertyAttribute;
            var jsonObjectAttribute = info.DeclaringType.GetTypeInfo().GetCustomAttributes(typeof(JsonObjectAttribute)).FirstOrDefault() as JsonObjectAttribute;

            var hasSpecifiedName = jsonPropertyAttribute != null && !string.IsNullOrWhiteSpace(jsonPropertyAttribute.PropertyName);
            var hasNamingStrategy = jsonObjectAttribute != null && jsonObjectAttribute.NamingStrategyType != null;

            if (!hasSpecifiedName && !hasNamingStrategy)
                return info.Name;

            if (hasSpecifiedName)
            {
                return jsonPropertyAttribute.PropertyName;
            }

            var strategy = (NamingStrategy)Activator.CreateInstance(jsonObjectAttribute.NamingStrategyType);
            return strategy.GetPropertyName(info.Name, false);
        }
    }
}