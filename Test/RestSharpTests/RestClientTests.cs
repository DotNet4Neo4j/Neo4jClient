using System;
using Neo4jClient.Serializer;
using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;

namespace Neo4jClient.Test.RestSharpTests
{
    [TestFixture]
    class RestClientTests
    {
        [Test]
        public void ExecuteShouldJSonSerializeAllProperties()
        {
            var uri = new Uri("http://foo/db/data");
            var testNode = new TestNode { Foo = "foo", Bar = "bar" };

            var request = new RestRequest(uri, Method.POST) { RequestFormat = DataFormat.Json };
            request.AddBody(testNode);

            const string expectedValue = "{\r\n  \"Foo\": \"foo\",\r\n  \"Bar\": \"bar\"\r\n}";
            Assert.AreEqual(expectedValue, request.Parameters[0].Value);
        }

        [Test]
        public void ExecuteShouldNotJSonSerializeNullProperties()
        {
            var uri = new Uri("http://foo/db/data");
            var testNode = new TestNode { Foo = "foo", Bar = null, Status = TestEnum.Value1 };

            var request = new RestRequest(uri, Method.POST)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomJsonSerializer {NullHandling =  NullValueHandling.Ignore}
            };
            request.AddBody(testNode);

            const string expectedValue = "{\r\n  \"Foo\": \"foo\",\r\n  \"Status\": \"Value1\"\r\n}";
            Assert.AreEqual(expectedValue, request.Parameters[0].Value);
        }

        [Test]
        public void ExecuteShouldSerializeEnumTypesToString()
        {
            var uri = new Uri("http://foo/db/data");
            var testNode = new TestNode { Status = TestEnum.Value1};

            var request = new RestRequest(uri, Method.POST)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomJsonSerializer { NullHandling = NullValueHandling.Ignore }
            };
            request.AddBody(testNode);

            const string expectedValue = "{\r\n  \"Status\": \"Value1\"\r\n}";
            Assert.AreEqual(expectedValue, request.Parameters[0].Value);
        }

        public class TestNode
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
            public TestEnum Status { get; set; }
        }

        public enum TestEnum
        {
            Value1,
            Value2
        }
    }
}
