using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Neo4jClient.Extensions
{
    public static class MemberInfoExtensions
    {
        /// <summary>Gets the name of the property, if a <see cref="JsonPropertyAttribute"/> is attached, it will attempt to use that first.</summary>
        /// <param name="info">The <see cref="MemberInfo"/> to get the name from</param>
        /// <returns>The name of the property, if a <see cref="JsonPropertyAttribute"/> is attached, it will use that first.</returns>
        internal static string GetNameUsingJsonProperty(this MemberInfo info)
        {
            var jsonProperty = info.GetCustomAttributes(typeof (JsonPropertyAttribute)).FirstOrDefault() as JsonPropertyAttribute;
            if (jsonProperty == null || string.IsNullOrWhiteSpace(jsonProperty.PropertyName))
                return info.Name;

            return jsonProperty.PropertyName;
        }
    }
}