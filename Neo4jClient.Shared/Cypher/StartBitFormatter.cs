using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

            if (!startBitsAsDictionary.Keys.Any())
                throw new ArgumentException("The start object you supplied didn't have any properties on it, resulting in an empty START clause. Consult the documentation at https://bitbucket.org/Readify/neo4jclient/wiki/cypher if you're unsure about how to use this overload.", "startBits");

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
                    if (value == null)
                        throw new ArgumentException(string.Format("The value of {0} was null.", identity), "startBits");
                    var cypherText = FormatBitAsCypherText(identity, value, createParameterCallback);
                    return identity + "=" + cypherText;
                })
                .ToArray();

            return string.Join(", ", cypherTextBits);
        }

        static string FormatBitAsCypherText(string identity, object value, CreateParameterCallback createParameterCallback)
        {
            var valueType = value.GetType();
            var formatterKey = Formatters
                .Keys
                .SingleOrDefault(k =>
                    k == valueType ||
                    k.IsAssignableFrom(valueType) ||
                    IsCovariantlyEquivalentEnumerable(k, valueType));

            if (formatterKey == null)
                throw new NotSupportedException(string.Format(
                    "The start expression for {0} is not a supported type. We were expecting one of: {1}. It was an instance of {2}.",
                    identity,
                    string.Join(", ", Formatters.Keys.Select(k => k.Name).ToArray()),
                    valueType.FullName
                ));

            return Formatters[formatterKey](value, createParameterCallback);
        }

        static bool IsCovariantlyEquivalentEnumerable(Type type1, Type type2)
        {
            return
                type1.GetTypeInfo().IsGenericType &&
                type1.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                type2.GetTypeInfo().IsGenericType &&
                type2.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                type1.GetGenericArguments()[0].IsAssignableFrom(type2.GetGenericArguments()[0]);
        }

        static readonly IDictionary<Type, Func<object, CreateParameterCallback, string>> Formatters = new Dictionary<Type, Func<object, CreateParameterCallback, string>>
        {
            {
                typeof(string),
                (value, callback) => (string)value
            },
            {
                typeof(StartBit),
                (value, callback) => FormatValue((StartBit)value, callback)
            },
            {
                typeof(NodeReference),
                (value, callback) => FormatValue((NodeReference)value, callback)
            },
            {
                typeof(IEnumerable<NodeReference>),
                (value, callback) => FormatValue(((IEnumerable<NodeReference>)value).ToArray(), callback)
            },
            {
                typeof(IHasNodeReference),
                (value, callback) => FormatValue(((IHasNodeReference)value).Reference, callback)
            },
            {
                typeof(IEnumerable<IHasNodeReference>),
                (value, callback) =>
                {
                    var references = ((IEnumerable<IHasNodeReference>) value).Select(n => n.Reference);
                    return FormatValue(references, callback);
                }
            },
            {
                typeof(RelationshipReference),
                (value, callback) => FormatValue((RelationshipReference)value, callback)
            },
            {
                typeof(IEnumerable<RelationshipReference>),
                (value, callback) => FormatValue(((IEnumerable<RelationshipReference>)value).ToArray(), callback)
            }
        };

        static string FormatValue(StartBit value, CreateParameterCallback createParameterCallback)
        {
            return value.ToCypherText(createParameterCallback);
        }

        static string FormatValue(NodeReference value, CreateParameterCallback createParameterCallback)
        {
            return FormatValue(new[] {value}, createParameterCallback);
        }

        static string FormatValue(IEnumerable<NodeReference> value, CreateParameterCallback createParameterCallback)
        {
            var ids = value.Select(v => v.Id).ToArray();
            var idsParam = ids.Count() == 1 ? createParameterCallback(ids.Single()) : createParameterCallback(ids);
            return string.Format("node({0})", idsParam);
        }

        static string FormatValue(RelationshipReference value, CreateParameterCallback createParameterCallback)
        {
            return FormatValue(new[] { value }, createParameterCallback);
        }

        static string FormatValue(IEnumerable<RelationshipReference> value, CreateParameterCallback createParameterCallback)
        {
            var ids = value.Select(v => v.Id).ToArray();
            var idsParam = ids.Count() == 1 ? createParameterCallback(ids.Single()) : createParameterCallback(ids);
            return string.Format("relationship({0})", idsParam);
        }
    }
}
