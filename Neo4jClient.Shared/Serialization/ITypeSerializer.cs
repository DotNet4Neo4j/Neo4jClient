using System;

namespace Neo4jClient.Serialization
{
    /// <summary>
    /// Serializer for a particular type.
    /// </summary>
    public interface ITypeSerializer
    {
        /// <summary>
        /// Determines if the serializer can handle this type.
        /// </summary>
        /// <param name="objectType">The type to deserialize into or serialize from.</param>
        /// <returns>A boolean value indicating if it can handle the given type.</returns>
        bool CanConvert(Type objectType);
        
        /// <summary>
        /// Deserialize an object.
        /// </summary>
        /// <param name="objectType">The expected type.</param>
        /// <param name="value">The serialized value.</param>
        /// <returns>An instance of the expected type after being deserialized.</returns>
        object Deserialize(Type objectType, object value);

        /// <summary>
        /// Serializes the given object.
        /// </summary>
        /// <param name="value">The instance to serialize.</param>
        /// <returns>A valid serialization of the given object.</returns>
        object Serialize(object value);
    }
}
