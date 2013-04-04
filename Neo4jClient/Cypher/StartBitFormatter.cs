using System;
using System.Collections.Generic;
using System.Linq;
using CreateParameterCallback = System.Func<object, string>;

namespace Neo4jClient.Cypher
{
    internal static class StartBitFormatter
    {
        internal static string FormatAsCypherText(object startBits, CreateParameterCallback createParameterCallback)
        {
            var cypherTextBits = startBits
                .GetType()
                .GetProperties()
                .Select(property =>
                {
                    var getMethod = property.GetGetMethod();
                    var value = getMethod.Invoke(startBits, new object[0]);

                    var identity = property.Name;
                    var cypherText = FormatBitAsCypherText(identity, value, createParameterCallback);

                    return identity + "=" + cypherText;
                })
                .ToArray();

            return string.Join(", ", cypherTextBits);
        }

        static string FormatBitAsCypherText(string identity, object value, CreateParameterCallback createParameterCallback)
        {
            var valueType = value.GetType();
            if (!Formatters.ContainsKey(valueType))
                throw new NotSupportedException(string.Format(
                    "The start expression for {0} is not a supported type. We were expecting one of: {1}. It was an instance of {2}.",
                    identity,
                    string.Join(", ", Formatters.Keys.Select(k => k.Name).ToArray()),
                    valueType.FullName
                ));

            return Formatters[valueType](value, createParameterCallback);
        }

        static readonly IDictionary<Type, Func<object, CreateParameterCallback, string>> Formatters = new Dictionary<Type, Func<object, CreateParameterCallback, string>>
        {
            {
                typeof(NodeReference),
                (value, callback) => FormatValue((NodeReference)value, callback)
            }
        };

        static string FormatValue(NodeReference value, CreateParameterCallback createParameterCallback)
        {
            return string.Format("node({0})", createParameterCallback(value.Id));
        }
    }
}
