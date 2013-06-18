using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Neo4jClient.ApiModels;
using System.Linq;
using Neo4jClient.ApiModels.Gremlin;
using Newtonsoft.Json;

namespace Neo4jClient.Test.ApiModels
{
    [TestFixture]
    public class GremlinTableCapResponseTests
    {
        [Test]
        public void VerifyTransferTableCapResponseToResult()
        {
            var list = new List<List<GremlinTableCapResponse>>();
            const string dataforfoo = "DataForFoo";
            const string dataforbar = "DataForBar";
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
            var response = GremlinTableCapResponse.TransferResponseToResult<SimpleClass>(list, new JsonConverter[0]).ToArray();

            Assert.IsTrue(response.Any(r => r.Foo == dataforfoo));
            Assert.IsTrue(response.Any(r => r.Bar == dataforbar));
        }

        [Test]
        public void VerifyTransferTableCapResponseToResultFromStringWithLongToLong()
        {
            var list = new List<List<GremlinTableCapResponse>>();

            list.Add(new List<GremlinTableCapResponse>
                {
                    new GremlinTableCapResponse
                        {
                            Columns = new List<string>
                                {
                                    "Long"
                                },
                            Data = new List<List<string>>
                                {
                                    new List<string>
                                        {
                                            "123",
                                        }
                                }
                        }
                });
            var response = GremlinTableCapResponse.TransferResponseToResult<SimpleClass>(list, new JsonConverter[0]).ToArray();
            Assert.IsTrue(response.Any(r => r.Long == 123));
        }

        [Test]
        public void VerifyTransferTableCapResponseToResultFromStringWithEnumValueToEnum()
        {
            var list = new List<List<GremlinTableCapResponse>>();

            list.Add(new List<GremlinTableCapResponse>
                {
                    new GremlinTableCapResponse
                        {
                            Columns = new List<string>
                                {
                                    "EnumValue"
                                },
                            Data = new List<List<string>>
                                {
                                    new List<string>
                                        {
                                            MyEnum.Foo.ToString(),
                                        }
                                }
                        }
                });
            var response = GremlinTableCapResponse.TransferResponseToResult<SimpleClass>(list, new JsonConverter[0]).ToArray();
            Assert.IsTrue(response.Any(r => r.EnumValue == MyEnum.Foo));
        }

        [Test]
        public void VerifyTransferTableCapResponseToResultFromStringWithNullableEnumValueToEnum()
        {
            var list = new List<List<GremlinTableCapResponse>>();

            list.Add(new List<GremlinTableCapResponse>
                {
                    new GremlinTableCapResponse
                        {
                            Columns = new List<string>
                                {
                                    "EnumValueNullable"
                                },
                            Data = new List<List<string>>
                                {
                                    new List<string>
                                        {
                                            MyEnum.Foo.ToString(),
                                        }
                                }
                        }
                });
            var response = GremlinTableCapResponse.TransferResponseToResult<SimpleClass>(list, new JsonConverter[0]).ToArray();
            Assert.IsTrue(response.Any(r => r.EnumValueNullable == MyEnum.Foo));
        }

        [Test]
        public void VerifyTransferTableCapResponseToResultFromStringWithLongToNullableLong()
        {
            var list = new List<List<GremlinTableCapResponse>>();

            list.Add(new List<GremlinTableCapResponse>
                {
                    new GremlinTableCapResponse
                        {
                            Columns = new List<string>
                                {
                                    "NullableLong"
                                },
                            Data = new List<List<string>>
                                {
                                    new List<string>
                                        {
                                            "123",
                                        }
                                }
                        }
                });
            var response = GremlinTableCapResponse.TransferResponseToResult<SimpleClass>(list, new JsonConverter[0]).ToArray();
            Assert.IsTrue(response.Any(r => r.NullableLong.Value == 123));
        }

        [Test]
        public void VerifyTransferTableCapResponseToResultFromStringWithLongNullToNullableLong()
        {
            var list = new List<List<GremlinTableCapResponse>>();

            list.Add(new List<GremlinTableCapResponse>
                {
                    new GremlinTableCapResponse
                        {
                            Columns = new List<string>
                                {
                                    "NullableLong"
                                },
                            Data = new List<List<string>>
                                {
                                    new List<string>
                                        {
                                            "",
                                        }
                                }
                        }
                });
            var response = GremlinTableCapResponse.TransferResponseToResult<SimpleClass>(list, new JsonConverter[0]).ToArray();
            Assert.IsTrue(response.Any(r => !r.NullableLong.HasValue));
        }

        [Test]
        public void VerifyTransferTableCapResponseToResultFromStringWithIntNullToNullableInt()
        {
            var list = new List<List<GremlinTableCapResponse>>();

            list.Add(new List<GremlinTableCapResponse>
                {
                    new GremlinTableCapResponse
                        {
                            Columns = new List<string>
                                {
                                    "NullableInt"
                                },
                            Data = new List<List<string>>
                                {
                                    new List<string>
                                        {
                                            "",
                                        }
                                }
                        }
                });
            var response = GremlinTableCapResponse.TransferResponseToResult<SimpleClass>(list, new JsonConverter[0]).ToArray();
            Assert.IsTrue(response.Any(r => !r.NullableLong.HasValue));
        }

        [Test]
        public void VerifyTransferTableCapResponseToResultFromStringWithLongNullAsStringToNullableLong()
        {
            var list = new List<List<GremlinTableCapResponse>>();

            list.Add(new List<GremlinTableCapResponse>
                {
                    new GremlinTableCapResponse
                        {
                            Columns = new List<string>
                                {
                                    "NullableLong"
                                },
                            Data = new List<List<string>>
                                {
                                    new List<string>
                                        {
                                            "null",
                                        }
                                }
                        }
                });
            var response = GremlinTableCapResponse.TransferResponseToResult<SimpleClass>(list, new JsonConverter[0]).ToArray();
            Assert.IsTrue(response.Any(r => !r.NullableLong.HasValue));
        }

        [Test]
        public void VerifyTransferTableCapResponseToResultFromStringWithDateTimeOffsetToNullableDateTimeOffset()
        {
            var list = new List<List<GremlinTableCapResponse>>();

            const string expectedDate = "01 Jul 2009";
            list.Add(new List<GremlinTableCapResponse>
                {
                    new GremlinTableCapResponse
                        {
                            Columns = new List<string>
                                {
                                    "DateTimeOffsetNullable"
                                },
                            Data = new List<List<string>>
                                {
                                    new List<string>
                                        {
                                            expectedDate,
                                        }
                                }
                        }
                });
            var response = GremlinTableCapResponse.TransferResponseToResult<SimpleClass>(list, new JsonConverter[0]).ToArray();
            Assert.IsTrue(response.Any(r => r.DateTimeOffsetNullable.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) == expectedDate));
        }

        [Test]
        public void VerifyTransferTableCapResponseToResultFromStringWithDateTimeOffsetToDateTimeOffsetUsingNeoDate()
        {
            var list = new List<List<GremlinTableCapResponse>>();

            const string date = "/NeoDate(1322007153048+1100)/";
            list.Add(new List<GremlinTableCapResponse>
                {
                    new GremlinTableCapResponse
                        {
                            Columns = new List<string>
                                {
                                    "DateTimeOffset"
                                },
                            Data = new List<List<string>>
                                {
                                    new List<string>
                                        {
                                            date,
                                        }
                                }
                        }
                });
            var response = GremlinTableCapResponse.TransferResponseToResult<SimpleClass>(list, new JsonConverter[0]).ToArray();
            Assert.IsTrue(response.Any(r => r.DateTimeOffset.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) == "23 Nov 2011"));
        }

        [Test]
        public void VerifyTransferTableCapResponseToResultFromStringWithDateTimeOffsetToDateTimeOffset()
        {
            var list = new List<List<GremlinTableCapResponse>>();

            const string expectedDate = "01 Jul 2009";
            list.Add(new List<GremlinTableCapResponse>
                {
                    new GremlinTableCapResponse
                        {
                            Columns = new List<string>
                                {
                                    "DateTimeOffset"
                                },
                            Data = new List<List<string>>
                                {
                                    new List<string>
                                        {
                                            expectedDate,
                                        }
                                }
                        }
                });
            var response = GremlinTableCapResponse.TransferResponseToResult<SimpleClass>(list, new JsonConverter[0]).ToArray();
            Assert.IsTrue(response.Any(r => r.DateTimeOffset.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) == expectedDate));
        }

        [Test]
        public void VerifyTransferTableCapResponseToResultFromStringWithDateTimeToDateTime()
        {
            var list = new List<List<GremlinTableCapResponse>>();

            const string expectedDate = "01 Jul 2009";
            list.Add(new List<GremlinTableCapResponse>
                {
                    new GremlinTableCapResponse
                        {
                            Columns = new List<string>
                                {
                                    "DateTime"
                                },
                            Data = new List<List<string>>
                                {
                                    new List<string>
                                        {
                                            expectedDate,
                                        }
                                }
                        }
                });
            var response = GremlinTableCapResponse.TransferResponseToResult<SimpleClass>(list, new JsonConverter[0]).ToArray();
            Assert.IsTrue(response.Any(r => r.DateTime.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) == expectedDate));
        }

    internal enum MyEnum {Foo, Bar, Baz}
    internal class  SimpleClass
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
            public long Long { get; set; }
            public long? NullableLong { get; set; }
            public int? NullableInt { get; set; }
            public MyEnum EnumValue { get; set; }
            public MyEnum? EnumValueNullable { get; set; }
            public DateTimeOffset DateTimeOffset { get; set; }
            public DateTime DateTime { get; set; }
            public DateTimeOffset DateTimeOffsetNullable { get; set; }
        }
    }
}
