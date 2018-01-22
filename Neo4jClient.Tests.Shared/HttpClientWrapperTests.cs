using System;
using System.Net.Http;
using System.Text;
using Neo4jClient.Test.Fixtures;
using NSubstitute;
using Xunit;

namespace Neo4jClient.Test
{
    
    public class HttpClientWrapperTests : IClassFixture<CultureInfoSetupFixture>
    {
        private static string Base64Header(string username, string password)
        {
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", username, password)));
        }

        [Fact]
        public void CreatesNewHttpClientWhenNotSupplied()
        {
            var wrapper = new HttpClientWrapper();
            Assert.NotNull(wrapper.Client);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void DoesNotCreateAuthenticationHeaderWhenPasswordIsNullOrEmpty(string password)
        {
            var wrapper = new HttpClientWrapper(password: password);
            Assert.Null(wrapper.AuthenticationHeaderValue);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void DoesNotCreateAuthenticationHeaderWhenUsernameIsNullOrEmpty(string username)
        {
            var wrapper = new HttpClientWrapper(username);
            Assert.Null(wrapper.AuthenticationHeaderValue);
        }

        [Fact]
        public void SetsAuthenticationHeaderValueCorrectly()
        {
            const string username = "user";
            const string password = "pass";
            var base64 = Base64Header(username, password);

            var wrapper = new HttpClientWrapper(username, password, Substitute.For<HttpClient>());
            Assert.Equal("Basic", wrapper.AuthenticationHeaderValue.Scheme);
            Assert.Equal(base64, wrapper.AuthenticationHeaderValue.Parameter);
        }

        [Fact]
        public void SetsTheUsernameAndPasswordProperties()
        {
            const string username = "username";
            const string password = "password";
            var client = Substitute.For<HttpClient>();
            var wrapper = new HttpClientWrapper(username, password, client);
            Assert.Equal(username, wrapper.Username);
            Assert.Equal(password, wrapper.Password);
        }

        [Fact]
        public void UsesGivenHttpClient()
        {
            var client = Substitute.For<HttpClient>();
            var wrapper = new HttpClientWrapper(client);

            Assert.Same(wrapper.Client, client);
        }
    }
}