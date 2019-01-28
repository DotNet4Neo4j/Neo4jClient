using System;

namespace Neo4jClient
{
    internal static class UriCreator
    {
        /// <summary>
        /// Creates a <see cref="Uri"/> from the given <see cref="string"/> <paramref name="uri"/> instance.
        /// </summary>
        /// <param name="uri">A <see cref="string"/> representing a URI, can contain the scheme, or not.</param>
        /// <returns>A <see cref="Uri"/> from the given <see cref="string"/> <paramref name="uri"/> instance.</returns>
        public static Uri From(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
                return null;

            if (uri.IndexOf("://", StringComparison.Ordinal) == -1)
                uri = $"scheme://{uri}";

            var output = new UriBuilder(uri);
            if (output.Uri.IsDefaultPort)
                output.Port = 7687;

            return output.Uri;
        }
    }
}