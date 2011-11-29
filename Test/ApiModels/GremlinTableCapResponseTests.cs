using System.Collections.Generic;
using NUnit.Framework;
using Neo4jClient.ApiModels;
using System.Linq;

namespace Neo4jClient.Test.ApiModels
{
    [TestFixture]
    public class GremlinTableCapResponseTests
    {
        [Test]
        public void VerifyTransferTableCapResponseToResult()
        {
            var list = new List<List<GremlinTableCapResponse>>();
            var dataforfoo = "DataForFoo";
            var dataforbar = "DataForBar";
            list.Add(new List<GremlinTableCapResponse>
                {
                    new GremlinTableCapResponse
                        {
                            Columns = new List<string>
                                {
                                    "Foo",
                                    "Bar"
                                },
                            Data = new List<List<string>>
                                {
                                    new List<string>
                                        {
                                            dataforfoo,
                                            dataforbar,
                                        }
                                }
                        }
                });
            var response = GremlinTableCapResponse.TransferResponseToResult<SimpleClass>(list).ToArray();

            Assert.IsTrue(response.Any(r => r.Foo == dataforfoo));
            Assert.IsTrue(response.Any(r => r.Bar == dataforbar));
        }

    public class  SimpleClass
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
        }
    }
}
