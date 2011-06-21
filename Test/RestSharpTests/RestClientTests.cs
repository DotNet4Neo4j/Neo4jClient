using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Neo4jClient.Serializer;
using Neo4jClient.Test.GraphClientTests;
using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using RestSharp.Serializers;

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
            var testNode = new TestNode { Foo = "foo", Bar = null };

            var request = new RestRequest(uri, Method.POST)
            {
                RequestFormat = DataFormat.Json,
                JsonSerializer = new CustomJsonSerializer {NullHandling =  NullValueHandling.Ignore}
            };
            request.AddBody(testNode);

            const string expectedValue = "{\r\n  \"Foo\": \"foo\"\r\n}";
            Assert.AreEqual(expectedValue, request.Parameters[0].Value);
        }

        public class TestNode
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
        }
    }
}
