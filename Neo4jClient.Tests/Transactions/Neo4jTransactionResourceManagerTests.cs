using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Neo4jClient.Execution;
using Neo4jClient.Transactions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Neo4jClient.Test.Transactions
{
    [TestFixture]
    public class Neo4jTransactionResourceManagerTests
    {
        #region Helper Classes
        private class TestJsonConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override bool CanConvert(Type objectType)
            {
                throw new NotImplementedException();
            }
        }

        private class TestHttpClient : IHttpClient
        {
            public string PropertyOne { get; set; }

            public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
            {
                throw new NotImplementedException();
            }
        }
        #endregion Helper Classes

        private static ExecutionConfiguration GetExecutionConfigurationFromNewAppDomain()
        {
            var newDomain = AppDomain.CreateDomain("NewDomain", null, new AppDomainSetup {ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase});
            var assemblyInDomain = newDomain.Load(typeof (Neo4jTransactionResourceManager).Assembly.FullName);
            var ntrm = assemblyInDomain.GetType(typeof (Neo4jTransactionResourceManager).FullName);
            var property = ntrm.GetProperty("ExecutionConfiguration", BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic);
            var ec = property.GetValue(null, null) as ExecutionConfiguration;
            return ec;
        }

        [Test]
        public void KeepsHttpClient_AcrossAppDomain()
        {
            const string propOne = "Property One";
            new GraphClient(new Uri("http://foo/bar"), new TestHttpClient {PropertyOne = propOne});

            var ec = GetExecutionConfigurationFromNewAppDomain();
            var tc = ec.HttpClient as TestHttpClient;
            Assert.IsNotNull(tc);
            Assert.AreEqual(propOne, tc.PropertyOne);
        }

        [Test]
        public void KeepsJsonDeserializers_AcrossAppDomain()
        {
            var client = new GraphClient(new Uri("http://foo/bar"));
            client.JsonConverters.Add(new TestJsonConverter());

            var ec = GetExecutionConfigurationFromNewAppDomain();
            var converters = ec.JsonConverters.Select(j => j.GetType().FullName).ToList();
            Assert.Contains(typeof (TestJsonConverter).FullName, converters);
        }

        [Test]
        public void KeepsUserAgent_AcrossAppDomain()
        {
            const string userAgent = "UserAgent_AppDomain";
            new GraphClient(new Uri("http://foo/bar")) {ExecutionConfiguration = {UserAgent = userAgent}};

            var ec = GetExecutionConfigurationFromNewAppDomain();
            Assert.IsNotNull(ec);
            Assert.AreEqual(userAgent, ec.UserAgent);
        }

        [Test]
        [TestCase(false)]
        [TestCase(true)]
        public void KeepsUseJsonStreaming_AcrossAppDomain(bool useStreaming)
        {
            new GraphClient(new Uri("http://foo/bar")) { ExecutionConfiguration = { UseJsonStreaming = useStreaming } };
            var ec = GetExecutionConfigurationFromNewAppDomain();

            Assert.AreEqual(useStreaming, ec.UseJsonStreaming);
        }
    }
}