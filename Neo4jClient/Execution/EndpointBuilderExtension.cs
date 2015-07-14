using System;

namespace Neo4jClient.Execution
{
    /// <summary>
    /// Restricts the end point URI builder to adding paths
    /// </summary>
    internal static class EndpointBuilderExtension
    {
        public static Uri AddPath(this Uri startUri, string path)
        {
            var uriBuilder = new UriBuilder(startUri);
            if (path.StartsWith("/") || uriBuilder.Path.EndsWith("/"))
            {
                uriBuilder.Path += path;
            }
            else
            {
                uriBuilder.Path += "/" + path;
            }
            return uriBuilder.Uri;
        }

        public static Uri AddQuery(this Uri startUri, string query)
        {
            var uriBuilder = new UriBuilder(startUri);
            if (uriBuilder.Query != null && uriBuilder.Query.Length > 1)
                uriBuilder.Query = uriBuilder.Query.Substring(1) + "&" + query;
            else
                uriBuilder.Query = query; 
            return uriBuilder.Uri;
        }

        public static Uri AddPath(this Uri startUri, object startReference, IExecutionPolicy policy)
        {
            return policy.AddPath(startUri, startReference);
        }

    }
}
