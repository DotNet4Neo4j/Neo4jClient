using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Neo4jClient.Execution;

namespace Neo4jClient
{
    public class HttpClientWrapper : IHttpClient
    {
        internal AuthenticationHeaderValue AuthenticationHeaderValue { get; private set; }
        internal readonly HttpClient Client;

        public HttpClientWrapper(string username = null, string password = null, HttpClient client = null) 
            : this(client ?? new HttpClient())
        {
            Username = username;
            Password = password;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return;

            var encoded = Encoding.ASCII.GetBytes(string.Format("{0}:{1}", username, password));
            AuthenticationHeaderValue = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(encoded));
        }

        public HttpClientWrapper(HttpClient client)
        {
            this.Client = client;
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            if (AuthenticationHeaderValue != null)
                request.Headers.Authorization = AuthenticationHeaderValue;

            return Client.SendAsync(request);
        }

        public string Username { get; private set; }
        public string Password { get; private set; }
    }
}