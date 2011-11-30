using System;
using System.ComponentModel;
using System.Reflection;

namespace Neo4jClient.Converters
{
    public static class ConverterHelper
    {
        public static void ConvertAndSetValue<TResult>(this TResult result, string value, PropertyInfo prop) where TResult : new()
        {
            try
            {
                var validType = prop.PropertyType;
                if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                {
                    var nullableConverter = new NullableConverter(prop.PropertyType);
                    validType = nullableConverter.UnderlyingType;
                }

                if (value == null || string.IsNullOrEmpty(value))
                {
                    prop.SetValue(result, null, null);
                    return;
                }

                object convertedData;
                if (validType.IsEnum)
                {
                    convertedData = Enum.Parse(validType, value, false);
                }
                else if (validType == typeof(DateTimeOffset))
                {
                    convertedData = DateTimeOffset.Parse(value);
                }
                else
                {
                    convertedData = Convert.ChangeType(value, validType);
                }
                prop.SetValue(result, convertedData, null);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Could not set property {0} to value {1} for type {2}\n {3}", prop.Name,
                                                  value, result.GetType().FullName, ex));
            }
        }
    }
}
