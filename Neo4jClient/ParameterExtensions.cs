using System.Collections.Generic;
using System.Linq;
using RestSharp;

namespace Neo4jClient
{
    internal static class ParameterExtensions
    {
        internal static string GetParameter(this IList<Parameter> parameters, string name)
        {
            return parameters
                .Where(p => p.Name == name)
                .Select(p => p.Value as string)
                .SingleOrDefault();
        }
    }
}