using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Neo4jClient.ApiModels;
using Newtonsoft.Json;

namespace Neo4jClient.Execution
{
    internal static class HttpResponseMessageExtensions
    {
        internal static Task EnsureExpectedStatusCode(this HttpResponseMessage response, params HttpStatusCode[] expectedStatusCodes)
        {
            return response.EnsureExpectedStatusCode(null, expectedStatusCodes);
        }

        internal static async Task EnsureExpectedStatusCode(this HttpResponseMessage response, string commandDescription, params HttpStatusCode[] expectedStatusCodes)
        {
            if (expectedStatusCodes.Contains(response.StatusCode))
                return;

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var neoException = await TryBuildNeoException(response).ConfigureAwait(false);
                if (neoException != null) throw neoException;
            }

            commandDescription = string.IsNullOrWhiteSpace(commandDescription)
                ? ""
                : commandDescription + "\r\n\r\n";

            var rawBody = string.Empty;
            if (response.Content != null)
            {
                var readTask = response.Content.ReadAsStringAsync();
                readTask.Wait();
                var rawContent = readTask.Result;
                rawBody = string.Format("\r\n\r\nThe response from Neo4j (which might include useful detail!) was: {0}", rawContent);
            }

            throw new Exception(string.Format(
                "Received an unexpected HTTP status when executing the request.\r\n\r\n{0}The response status was: {1} {2}{3}",
                commandDescription,
                (int)response.StatusCode,
                response.ReasonPhrase,
                rawBody));
        }

        static async Task<NeoException> TryBuildNeoException(HttpResponseMessage response)
        {
            var isJson = response.Content.Headers.ContentType.MediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase);
            if (!isJson) return null;

            var exceptionResponse = await response.Content.ReadAsJsonAsync<ExceptionResponse>(new JsonConverter[0]).ConfigureAwait(false);

            if (string.IsNullOrEmpty(exceptionResponse.Message) ||
                string.IsNullOrEmpty(exceptionResponse.Exception))
                return null;

            return new NeoException(exceptionResponse);
        }
    }
}
