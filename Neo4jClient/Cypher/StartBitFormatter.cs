using System;
using System.Collections.Generic;
using System.Linq;
using CreateParameterCallback = System.Func<object, string>;

namespace Neo4jClient.Cypher
{
    internal static class StartBitFormatter
    {
        internal static string FormatAsCypherText(
            object startBits,
            CreateParameterCallback createParameterCallback)
        {
            var startBitsAsDictionary = startBits
                .GetType()
                .GetProperties()
                .Select(property =>
                {
                    var getMethod = property.GetGetMethod();
                    var value = getMethod.Invoke(startBits, new object[0]);
                    return new {Identity = property.Name, Value = value};
                })
                .ToDictionary(k => k.Identity, k=> k.Value);

            return FormatAsCypherText(startBitsAsDictionary, createParameterCallback);
        }

        internal static string FormatAsCypherText(
            IDictionary<string, object> startBits,
            CreateParameterCallback createParameterCallback)
        {
            var cypherTextBits = startBits
                .Keys
                .Select(identity =>
                {
                    var value = startBits[identity];
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
                typeof(string),
                (value, callback) => (string)value
            },
            {
                typeof(NodeReference),
                (value, callback) => FormatValue((NodeReference)value, callback)
            },
            {
                typeof(NodeReference[]),
                (value, callback) => FormatValue((NodeReference[])value, callback)
            },
            {
                typeof(RelationshipReference),
                (value, callback) => FormatValue((RelationshipReference)value, callback)
            },
            {
                typeof(RelationshipReference[]),
                (value, callback) => FormatValue((RelationshipReference[])value, callback)
            }
        };

        static string FormatValue(NodeReference value, CreateParameterCallback createParameterCallback)
        {
            return string.Format("node({0})", createParameterCallback(value.Id));
        }

        static string FormatValue(IEnumerable<NodeReference> value, CreateParameterCallback createParameterCallback)
        {
            var paramNames = value
                .Select(v => createParameterCallback(v.Id))
                .ToArray();

            return string.Format("node({0})", string.Join(", ", paramNames));
        }

        static string FormatValue(RelationshipReference value, CreateParameterCallback createParameterCallback)
        {
            return string.Format("relationship({0})", createParameterCallback(value.Id));
        }

        static string FormatValue(IEnumerable<RelationshipReference> value, CreateParameterCallback createParameterCallback)
        {
            var paramNames = value
                .Select(v => createParameterCallback(v.Id))
                .ToArray();

            return string.Format("relationship({0})", string.Join(", ", paramNames));
        }
    }
}
