using System;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace Neo4jClient
{
    internal static class HttpResponseMessageExtensions
    {
        internal static void EnsureExpectedStatusCode(this HttpResponseMessage response, params HttpStatusCode[] expectedStatusCodes)
        {
            response.EnsureExpectedStatusCode(null, expectedStatusCodes);
        }

        internal static void EnsureExpectedStatusCode(this HttpResponseMessage response, string commandDescription, params HttpStatusCode[] expectedStatusCodes)
        {
            if (expectedStatusCodes.Contains(response.StatusCode))
                return;

            commandDescription = string.IsNullOrWhiteSpace(commandDescription)
                ? ""
                : commandDescription + "\r\n\r\n";

            var rawBody = string.Empty;
            if (response.Content != null)
            {
                var readTask = response.Content.ReadAsStringAsync();
                readTask.Wait();
                var rawContent = readTask.Result;
                rawBody = string.Format("\r\n\r\nThe raw response body was: {0}", rawContent);
            }

            throw new ApplicationException(string.Format(
                "Received an unexpected HTTP status when executing the request.\r\n\r\n{0}The response status was: {1} {2}{3}",
                commandDescription,
                (int)response.StatusCode,
                response.ReasonPhrase,
                rawBody));
        }
    }
}
