using System;
using System.Net.Http;
using System.Text;
using NSubstitute;
using NUnit.Framework;

namespace Neo4jClient.Test
{
    [TestFixture]
    public class HttpClientWrapperTests
    {
        private static string Base64Header(string username, string password)
        {
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", username, password)));
        }

        [Test]
        public void SetsAuthenticationHeaderValueCorrectly()
        {
            const string username = "user";
            const string password = "pass";
            var base64 = Base64Header(username, password);

            var wrapper = new HttpClientWrapper(username, password, Substitute.For<HttpClient>());
            Assert.AreEqual("Basic", wrapper.AuthenticationHeaderValue.Scheme);
            Assert.AreEqual(base64, wrapper.AuthenticationHeaderValue.Parameter);
        }

        [Test]
        public void CreatesNewHttpClientWhenNotSupplied()
        {
            var wrapper = new HttpClientWrapper();
            Assert.IsNotNull(wrapper.Client);
        }

        [Test]
        public void DoesNotCreateAuthenticationHeaderWhenUsernameIsNullOrEmpty(
            [Values("", " ", null)]string username
            )
        {
            var wrapper = new HttpClientWrapper(username);
            Assert.IsNull(wrapper.AuthenticationHeaderValue);
        }

        [Test]
        public void DoesNotCreateAuthenticationHeaderWhenPasswordIsNullOrEmpty(
            [Values("", " ", null)]string password
            )
        {
            var wrapper = new HttpClientWrapper(password:password);
            Assert.IsNull(wrapper.AuthenticationHeaderValue);
        }

        [Test]
        public void UsesGivenHttpClient()
        {
            var client = Substitute.For<HttpClient>();
            var wrapper = new HttpClientWrapper(client);

            Assert.AreSame(wrapper.Client, client);
        }

        [Test]
        public void SetsTheUsernameAndPasswordProperties()
        {
            const string username = "username";
            const string password = "password";
            var client = Substitute.For<HttpClient>();
            var wrapper = new HttpClientWrapper(username, password, client);
            Assert.AreEqual(username, wrapper.Username);
            Assert.AreEqual(password, wrapper.Password);
        }
        

        [Test]
        public void AddsAuthorizationHeaderToRequestWhenSet()
        {
            const string username = "username";
            const string password = "password";
            var client = Substitute.For<HttpClient>();
            var wrapper = new HttpClientWrapper(username, password, client);

            var requestWithAuth = new HttpRequestMessage(HttpMethod.Get, "http://foo/");
            requestWithAuth.Headers.Authorization = wrapper.AuthenticationHeaderValue;

            wrapper.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://foo/"));
            client.Received(1).SendAsync(requestWithAuth);
        }

        [Test]
        public void DoesNotAddAuthorizationHeaderToRequestWhenNotSet()
        {
            var client = Substitute.For<HttpClient>();
            var wrapper = new HttpClientWrapper(client: client);

            var request = new HttpRequestMessage(HttpMethod.Get, "http://foo/");

            wrapper.SendAsync(request);
            client.Received(1).SendAsync(request);
        }
    }
}