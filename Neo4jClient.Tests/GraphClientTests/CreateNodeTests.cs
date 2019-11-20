﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Neo4jClient.ApiModels;
using Xunit;

namespace Neo4jClient.Tests.GraphClientTests
{
    
    public class CreateNodeTests : IClassFixture<CultureInfoSetupFixture>
    {
        [Fact]
        public async Task ShouldThrowArgumentNullExceptionForNullNode()
        {
            var client = new GraphClient(new Uri("http://foo"));
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await client.CreateAsync<object>(null));
        }

        [Fact]
        public async Task ShouldThrowInvalidOperationExceptionIfNotConnected()
        {
            var client = new GraphClient(new Uri("http://foo"));
            await Assert.ThrowsAsync<InvalidOperationException>(() => client.CreateAsync(new object()));
        }

        [Fact]
        public async Task ShouldThrowValidationExceptionForInvalidNodes()
        {
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), null);

            var testNode = new TestNode {Foo = "text is too long", Bar = null, Baz = "123"};
            await Assert.ThrowsAsync<ValidationException>(() => graphClient.CreateAsync(testNode));
        }

        [Fact]
        public async Task ShouldThrowArgumentExceptionForPreemptivelyWrappedNode()
        {
            var graphClient = new GraphClient(new Uri("http://foo/db/data"), null);
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => graphClient.CreateAsync((Node<TestNode>)null));
            ex.Message.Should().Be($"You're trying to pass in a Node<TestNode> instance. Just pass the TestNode instance instead.{Environment.NewLine}Parameter name: node");
        }

        [Fact]
        public async Task ShouldThrowNeoExceptionWhenBatchCreationStepJobFails()
        {
            var testHarness = new RestTestHarness
            {
                {
                    MockRequest.PostJson("/batch",
                        @"[{
                          'method': 'POST', 'to' : '/node',
                          'body': {
                            'Foo': 'foo',
                            'TestNode2': { 'Foo': 'foo', 'Bar': 'bar' }
                          },
                          'id': 0
                        }]"
                    ),
                    MockResponse.Json(HttpStatusCode.OK,
                       @"[ {
                        'id':0,'location':null,
                        'body': {
                            'message': 'Could not set property ""TestNode2"", unsupported type: {Foo=foo, Bar=bar}',
                            'exception': 'PropertyValueException',
                            'fullname': 'org.neo4j.server.rest.web.PropertyValueException',
                            'stacktrace': [
                               'org.neo4j.server.rest.domain.PropertySettingStrategy.setProperty(PropertySettingStrategy.java:141)',
                               'java.lang.Thread.run(Unknown Source)'
                            ]
                        },
                        'status': 400}]"
                    )
                }
            };

            var graphClient = await testHarness.CreateAndConnectGraphClient();
            var ex = await Assert.ThrowsAsync<NeoException>(async () => await graphClient.CreateAsync(new NestedTestNode()
                                   {
                                       Foo = "foo",
                                       TestNode2 = new TestNode2() {Bar = "bar", Foo = "foo"}
                                   }));
            ex.Message.Should().Be("PropertyValueException: Could not set property \"TestNode2\", unsupported type: {Foo=foo, Bar=bar}");
        }

        [Fact]
        public async Task ShouldNotThrowANotSupportedExceptionForPre15M02DatabaseWhenThereAreNoIndexEntries()
        {
            var testNode = new TestNode { Foo = "foo", Bar = "bar", Baz = "baz" };
            var batch = new List<BatchStep>();
            batch.Add(HttpMethod.Post, "/node", testNode);

            var testHarness = new RestTestHarness
            {
                {
                    MockRequest.Get(""),
                    MockResponse.NeoRootPre15M02()
                },
                {
                    MockRequest.PostObjectAsJson("/batch", batch),
                    MockResponse.Json(HttpStatusCode.OK,
                        @"[{'id':0,'location':'http://foo/db/data/node/760','body':{
                          'outgoing_relationships' : 'http://foo/db/data/node/760/relationships/out',
                          'data' : {
                            'Foo' : 'foo',
                            'Bar' : 'bar',
                            'Baz' : 'baz'
                          },
                          'traverse' : 'http://foo/db/data/node/760/traverse/{returnType}',
                          'all_typed_relationships' : 'http://foo/db/data/node/760/relationships/all/{-list|&|types}',
                          'self' : 'http://foo/db/data/node/760',
                          'property' : 'http://foo/db/data/node/760/properties/{key}',
                          'outgoing_typed_relationships' : 'http://foo/db/data/node/760/relationships/out/{-list|&|types}',
                          'properties' : 'http://foo/db/data/node/760/properties',
                          'incoming_relationships' : 'http://foo/db/data/node/760/relationships/in',
                          'extensions' : {
                          },
                          'create_relationship' : 'http://foo/db/data/node/760/relationships',
                          'paged_traverse' : 'http://foo/db/data/node/760/paged/traverse/{returnType}{?pageSize,leaseTime}',
                          'all_relationships' : 'http://foo/db/data/node/760/relationships/all',
                          'incoming_typed_relationships' : 'http://foo/db/data/node/760/relationships/in/{-list|&|types}'
                        },'from':'/node'}]"
                    )
                }
            };

            var graphClient = await testHarness.CreateAndConnectGraphClient();

            await graphClient.CreateAsync(testNode, null, null);

            testHarness.AssertRequestConstraintsAreMet();
        }

        [Fact]
        public async Task ShouldSerializeAllProperties()
        {
            var testHarness = new RestTestHarness
            {
                {
                    MockRequest.PostJson("/batch",
                        @"[{
                          'method': 'POST', 'to' : '/node',
                          'body': {
                            'Foo': 'foo',
                            'Bar': 'bar',
                            'Baz': 'baz'
                          },
                          'id': 0
                        }]"
                    ),
                    MockResponse.Json(HttpStatusCode.OK,
                        @"[{'id':0,'location':'http://foo/db/data/node/760','body':{
                          'outgoing_relationships' : 'http://foo/db/data/node/760/relationships/out',
                          'data' : {
                            'Foo' : 'foo',
                            'Bar' : 'bar',
                            'Baz' : 'baz'
                          },
                          'traverse' : 'http://foo/db/data/node/760/traverse/{returnType}',
                          'all_typed_relationships' : 'http://foo/db/data/node/760/relationships/all/{-list|&|types}',
                          'self' : 'http://foo/db/data/node/760',
                          'property' : 'http://foo/db/data/node/760/properties/{key}',
                          'outgoing_typed_relationships' : 'http://foo/db/data/node/760/relationships/out/{-list|&|types}',
                          'properties' : 'http://foo/db/data/node/760/properties',
                          'incoming_relationships' : 'http://foo/db/data/node/760/relationships/in',
                          'extensions' : {
                          },
                          'create_relationship' : 'http://foo/db/data/node/760/relationships',
                          'paged_traverse' : 'http://foo/db/data/node/760/paged/traverse/{returnType}{?pageSize,leaseTime}',
                          'all_relationships' : 'http://foo/db/data/node/760/relationships/all',
                          'incoming_typed_relationships' : 'http://foo/db/data/node/760/relationships/in/{-list|&|types}'
                        },'from':'/node'}]"
                    )
                }
            };

            var graphClient = await testHarness.CreateAndConnectGraphClient();

            await graphClient.CreateAsync(new TestNode { Foo = "foo", Bar = "bar", Baz = "baz" });

            testHarness.AssertRequestConstraintsAreMet();
        }

        [Fact]
        public async Task ShouldPreserveUnicodeCharactersInStringProperties()
        {
            var testHarness = new RestTestHarness
            {
                {
                    MockRequest.PostJson("/batch",
                        @"[{
                            'method': 'POST', 'to' : '/node',
                            'body': { 'Foo': 'foo東京', 'Bar': 'bar', 'Baz': 'baz' },
                            'id': 0
                        }]"
                    ),
                    MockResponse.Json(HttpStatusCode.OK,
                        @"[{'id':0,'location':'http://foo/db/data/node/760','body':{
                          'outgoing_relationships' : 'http://foo/db/data/node/760/relationships/out',
                          'data' : {
                            'Foo' : 'foo',
                            'Bar' : 'bar',
                            'Baz' : 'baz'
                          },
                          'traverse' : 'http://foo/db/data/node/760/traverse/{returnType}',
                          'all_typed_relationships' : 'http://foo/db/data/node/760/relationships/all/{-list|&|types}',
                          'self' : 'http://foo/db/data/node/760',
                          'property' : 'http://foo/db/data/node/760/properties/{key}',
                          'outgoing_typed_relationships' : 'http://foo/db/data/node/760/relationships/out/{-list|&|types}',
                          'properties' : 'http://foo/db/data/node/760/properties',
                          'incoming_relationships' : 'http://foo/db/data/node/760/relationships/in',
                          'extensions' : {
                          },
                          'create_relationship' : 'http://foo/db/data/node/760/relationships',
                          'paged_traverse' : 'http://foo/db/data/node/760/paged/traverse/{returnType}{?pageSize,leaseTime}',
                          'all_relationships' : 'http://foo/db/data/node/760/relationships/all',
                          'incoming_typed_relationships' : 'http://foo/db/data/node/760/relationships/in/{-list|&|types}'
                        },'from':'/node'}]"
                    )
                }
            };

            var graphClient = await testHarness.CreateAndConnectGraphClient();

            await graphClient.CreateAsync(new TestNode { Foo = "foo東京", Bar = "bar", Baz = "baz" });

            testHarness.AssertRequestConstraintsAreMet();
        }

        [Fact]
        public async Task ShouldReturnReferenceToCreatedNode()
        {
            var testNode = new TestNode { Foo = "foo", Bar = "bar", Baz = "baz" };
            var batch = new List<BatchStep>();
            batch.Add(HttpMethod.Post, "/node", testNode);

            var testHarness = new RestTestHarness
            {
                {
                    MockRequest.PostObjectAsJson("/batch", batch),
                    MockResponse.Json(HttpStatusCode.OK,
                        @"[{'id':0,'location':'http://foo/db/data/node/760','body':{
                          'outgoing_relationships' : 'http://foo/db/data/node/760/relationships/out',
                          'data' : {
                            'Foo' : 'foo',
                            'Bar' : 'bar',
                            'Baz' : 'baz'
                          },
                          'traverse' : 'http://foo/db/data/node/760/traverse/{returnType}',
                          'all_typed_relationships' : 'http://foo/db/data/node/760/relationships/all/{-list|&|types}',
                          'self' : 'http://foo/db/data/node/760',
                          'property' : 'http://foo/db/data/node/760/properties/{key}',
                          'outgoing_typed_relationships' : 'http://foo/db/data/node/760/relationships/out/{-list|&|types}',
                          'properties' : 'http://foo/db/data/node/760/properties',
                          'incoming_relationships' : 'http://foo/db/data/node/760/relationships/in',
                          'extensions' : {
                          },
                          'create_relationship' : 'http://foo/db/data/node/760/relationships',
                          'paged_traverse' : 'http://foo/db/data/node/760/paged/traverse/{returnType}{?pageSize,leaseTime}',
                          'all_relationships' : 'http://foo/db/data/node/760/relationships/all',
                          'incoming_typed_relationships' : 'http://foo/db/data/node/760/relationships/in/{-list|&|types}'
                        },'from':'/node'}]"
                    )
                }
            };

            var graphClient = await testHarness.CreateAndConnectGraphClient();

            var node = await graphClient.CreateAsync(testNode);

            Assert.Equal(760, node.Id);
            testHarness.AssertRequestConstraintsAreMet();
        }

        [Fact]
        public async Task ShouldReturnReferenceToCreatedNodeWithLongId()
        {
            var testNode = new TestNode { Foo = "foo", Bar = "bar", Baz = "baz" };
            var batch = new List<BatchStep>();
            batch.Add(HttpMethod.Post, "/node", testNode);

            var testHarness = new RestTestHarness
            {
                {
                    MockRequest.PostObjectAsJson("/batch", batch),
                    MockResponse.Json(HttpStatusCode.OK,
                        @"[{'id':0,'location':'http://foo/db/data/node/2157483647','body':{
                          'outgoing_relationships' : 'http://foo/db/data/node/2157483647/relationships/out',
                          'data' : {
                            'Foo' : 'foo',
                            'Bar' : 'bar',
                            'Baz' : 'baz'
                          },
                          'traverse' : 'http://foo/db/data/node/2157483647/traverse/{returnType}',
                          'all_typed_relationships' : 'http://foo/db/data/node/2157483647/relationships/all/{-list|&|types}',
                          'self' : 'http://foo/db/data/node/2157483647',
                          'property' : 'http://foo/db/data/node/2157483647/properties/{key}',
                          'outgoing_typed_relationships' : 'http://foo/db/data/node/2157483647/relationships/out/{-list|&|types}',
                          'properties' : 'http://foo/db/data/node/2157483647/properties',
                          'incoming_relationships' : 'http://foo/db/data/node/2157483647/relationships/in',
                          'extensions' : {
                          },
                          'create_relationship' : 'http://foo/db/data/node/2157483647/relationships',
                          'paged_traverse' : 'http://foo/db/data/node/2157483647/paged/traverse/{returnType}{?pageSize,leaseTime}',
                          'all_relationships' : 'http://foo/db/data/node/2157483647/relationships/all',
                          'incoming_typed_relationships' : 'http://foo/db/data/node/2157483647/relationships/in/{-list|&|types}'
                        },'from':'/node'}]"
                    )
                }
            };

            var graphClient = await testHarness.CreateAndConnectGraphClient();

            var node = await graphClient.CreateAsync(testNode);

            Assert.Equal(2157483647, node.Id);
            testHarness.AssertRequestConstraintsAreMet();
        }

        [Fact]
        public async Task ShouldReturnAttachedNodeReference()
        {
            var testNode = new TestNode { Foo = "foo", Bar = "bar", Baz = "baz" };
            var batch = new List<BatchStep>();
            batch.Add(HttpMethod.Post, "/node", testNode);

            var testHarness = new RestTestHarness
            {
                {
                    MockRequest.PostObjectAsJson("/batch", batch),
                    MockResponse.Json(HttpStatusCode.OK,
                        @"[{'id':0,'location':'http://foo/db/data/node/760','body':{
                          'outgoing_relationships' : 'http://foo/db/data/node/760/relationships/out',
                          'data' : {
                            'Foo' : 'foo',
                            'Bar' : 'bar',
                            'Baz' : 'baz'
                          },
                          'traverse' : 'http://foo/db/data/node/760/traverse/{returnType}',
                          'all_typed_relationships' : 'http://foo/db/data/node/760/relationships/all/{-list|&|types}',
                          'self' : 'http://foo/db/data/node/760',
                          'property' : 'http://foo/db/data/node/760/properties/{key}',
                          'outgoing_typed_relationships' : 'http://foo/db/data/node/760/relationships/out/{-list|&|types}',
                          'properties' : 'http://foo/db/data/node/760/properties',
                          'incoming_relationships' : 'http://foo/db/data/node/760/relationships/in',
                          'extensions' : {
                          },
                          'create_relationship' : 'http://foo/db/data/node/760/relationships',
                          'paged_traverse' : 'http://foo/db/data/node/760/paged/traverse/{returnType}{?pageSize,leaseTime}',
                          'all_relationships' : 'http://foo/db/data/node/760/relationships/all',
                          'incoming_typed_relationships' : 'http://foo/db/data/node/760/relationships/in/{-list|&|types}'
                        },'from':'/node'}]"
                    )
                }
            };

            var graphClient = await testHarness.CreateAndConnectGraphClient();

            var node = await graphClient.CreateAsync(testNode);

            Assert.NotNull(((IAttachedReference)node).Client);
        }

        [Fact]
        public async Task ShouldCreateOutgoingRelationship()
        {
            var testNode = new TestNode { Foo = "foo", Bar = "bar", Baz = "baz" };
            var testRelationshipPayload = new TestPayload { Foo = "123", Bar = "456", Baz = "789" };
            var batch = new List<BatchStep>();
            batch.Add(HttpMethod.Post, "/node", testNode);
            batch.Add(HttpMethod.Post, "{0}/relationships",
                new RelationshipTemplate { To = "/node/789", Data = testRelationshipPayload, Type = "TEST_RELATIONSHIP" });

            var testHarness = new RestTestHarness
            {
                {
                    MockRequest.PostObjectAsJson("/batch", batch),
                    MockResponse.Json(HttpStatusCode.OK,
                        @"[{'id':0,'location':'http://foo/db/data/node/760','body':{
                          'outgoing_relationships' : 'http://foo/db/data/node/760/relationships/out',
                          'data' : {
                            'Foo' : 'foo',
                            'Bar' : 'bar',
                            'Baz' : 'baz'
                          },
                          'traverse' : 'http://foo/db/data/node/760/traverse/{returnType}',
                          'all_typed_relationships' : 'http://foo/db/data/node/760/relationships/all/{-list|&|types}',
                          'self' : 'http://foo/db/data/node/760',
                          'property' : 'http://foo/db/data/node/760/properties/{key}',
                          'outgoing_typed_relationships' : 'http://foo/db/data/node/760/relationships/out/{-list|&|types}',
                          'properties' : 'http://foo/db/data/node/760/properties',
                          'incoming_relationships' : 'http://foo/db/data/node/760/relationships/in',
                          'extensions' : {
                          },
                          'create_relationship' : 'http://foo/db/data/node/760/relationships',
                          'paged_traverse' : 'http://foo/db/data/node/760/paged/traverse/{returnType}{?pageSize,leaseTime}',
                          'all_relationships' : 'http://foo/db/data/node/760/relationships/all',
                          'incoming_typed_relationships' : 'http://foo/db/data/node/760/relationships/in/{-list|&|types}'
                        },'from':'/node'},{'id':1,'location':'http://foo/db/data/relationship/756','body':{
                          'start' : 'http://foo/db/data/node/760',
                          'data' : {
                            'Foo' : 123,
                            'Bar' : 456,
                            'Baz' : 789
                          },
                          'property' : 'http://foo/db/data/relationship/756/properties/{key}',
                          'self' : 'http://foo/db/data/relationship/756',
                          'properties' : 'http://foo/db/data/relationship/756/properties',
                          'type' : 'TEST_RELATIONSHIP',
                          'extensions' : {
                          },
                          'end' : 'http://foo/db/data/node/789'
                        },'from':'http://foo/db/data/node/761/relationships'}]"
                    )
                }
            };

            var graphClient = await testHarness.CreateAndConnectGraphClient();

            await graphClient.CreateAsync(
                testNode,
                new TestRelationship(789, testRelationshipPayload));

            testHarness.AssertRequestConstraintsAreMet();
        }

        [Fact]
        public async Task ShouldCreateIndexEntries()
        {
            var testNode = new TestNode { Foo = "foo", Bar = "bar", Baz = "baz" };
            var batch = new List<BatchStep>();
            batch.Add(HttpMethod.Post, "/node", testNode);
            batch.Add(HttpMethod.Post, "/index/node/my_index", new { key = "key", value = "value", uri = "{0}" });
            batch.Add(HttpMethod.Post, "/index/node/my_index", new { key = "key3", value = "value3", uri = "{0}" });

            var testHarness = new RestTestHarness
            {
                {
                    MockRequest.PostObjectAsJson("/batch", batch),
                    MockResponse.Json(HttpStatusCode.OK,
                        @"[{'id':0,'location':'http://localhost:20001/db/data/node/763','body':{
                          'outgoing_relationships' : 'http://localhost:20001/db/data/node/763/relationships/out',
                          'data' : {
                            'Baz' : 'baz',
                            'Foo' : 'foo',
                            'Bar' : 'bar'
                          },
                          'traverse' : 'http://localhost:20001/db/data/node/763/traverse/{returnType}',
                          'all_typed_relationships' : 'http://localhost:20001/db/data/node/763/relationships/all/{-list|&|types}',
                          'self' : 'http://localhost:20001/db/data/node/763',
                          'property' : 'http://localhost:20001/db/data/node/763/properties/{key}',
                          'outgoing_typed_relationships' : 'http://localhost:20001/db/data/node/763/relationships/out/{-list|&|types}',
                          'properties' : 'http://localhost:20001/db/data/node/763/properties',
                          'incoming_relationships' : 'http://localhost:20001/db/data/node/763/relationships/in',
                          'extensions' : {
                          },
                          'create_relationship' : 'http://localhost:20001/db/data/node/763/relationships',
                          'paged_traverse' : 'http://localhost:20001/db/data/node/763/paged/traverse/{returnType}{?pageSize,leaseTime}',
                          'all_relationships' : 'http://localhost:20001/db/data/node/763/relationships/all',
                          'incoming_typed_relationships' : 'http://localhost:20001/db/data/node/763/relationships/in/{-list|&|types}'
                        },'from':'/node'},{'id':1,'location':'http://localhost:20001/db/data/index/node/my_index/key/value/763','body':{
                          'indexed' : 'http://localhost:20001/db/data/index/node/my_index/key/value/763',
                          'outgoing_relationships' : 'http://localhost:20001/db/data/node/763/relationships/out',
                          'data' : {
                            'Baz' : 'baz',
                            'Foo' : 'foo',
                            'Bar' : 'bar'
                          },
                          'traverse' : 'http://localhost:20001/db/data/node/763/traverse/{returnType}',
                          'all_typed_relationships' : 'http://localhost:20001/db/data/node/763/relationships/all/{-list|&|types}',
                          'self' : 'http://localhost:20001/db/data/node/763',
                          'property' : 'http://localhost:20001/db/data/node/763/properties/{key}',
                          'outgoing_typed_relationships' : 'http://localhost:20001/db/data/node/763/relationships/out/{-list|&|types}',
                          'properties' : 'http://localhost:20001/db/data/node/763/properties',
                          'incoming_relationships' : 'http://localhost:20001/db/data/node/763/relationships/in',
                          'extensions' : {
                          },
                          'create_relationship' : 'http://localhost:20001/db/data/node/763/relationships',
                          'paged_traverse' : 'http://localhost:20001/db/data/node/763/paged/traverse/{returnType}{?pageSize,leaseTime}',
                          'all_relationships' : 'http://localhost:20001/db/data/node/763/relationships/all',
                          'incoming_typed_relationships' : 'http://localhost:20001/db/data/node/763/relationships/in/{-list|&|types}'
                        },'from':'/index/node/my_index/key/value'}]"
                    )
                }
            };

            var graphClient = await testHarness.CreateAndConnectGraphClient();

            await graphClient.CreateAsync(
                testNode,
                null,
                new[]
                {
                    new IndexEntry
                    {
                        Name = "my_index",
                        KeyValues = new[]
                        {
                            new KeyValuePair<string, object>("key", "value"),
                            new KeyValuePair<string, object>("key2", ""),
                            new KeyValuePair<string, object>("key3", "value3")
                        }
                    }
                });
        }

        [Fact]
        public async Task ShouldCreateIncomingRelationship()
        {
            var testNode = new TestNode2 { Foo = "foo", Bar = "bar" };
            var testRelationshipPayload = new TestPayload { Foo = "123", Bar = "456", Baz = "789" };
            var batch = new List<BatchStep>();
            batch.Add(HttpMethod.Post, "/node", testNode);
            batch.Add(HttpMethod.Post, "/node/789/relationships",
                new RelationshipTemplate { To = "{0}", Data = testRelationshipPayload, Type = "TEST_RELATIONSHIP" });

            var testHarness = new RestTestHarness
            {
                {
                    MockRequest.PostObjectAsJson("/batch", batch),
                    MockResponse.Json(HttpStatusCode.OK,
                        @"[{'id':0,'location':'http://foo/db/data/node/760','body':{
                          'outgoing_relationships' : 'http://foo/db/data/node/760/relationships/out',
                          'data' : {
                            'Foo' : 'foo',
                            'Bar' : 'bar',
                            'Baz' : 'baz'
                          },
                          'traverse' : 'http://foo/db/data/node/760/traverse/{returnType}',
                          'all_typed_relationships' : 'http://foo/db/data/node/760/relationships/all/{-list|&|types}',
                          'self' : 'http://foo/db/data/node/760',
                          'property' : 'http://foo/db/data/node/760/properties/{key}',
                          'outgoing_typed_relationships' : 'http://foo/db/data/node/760/relationships/out/{-list|&|types}',
                          'properties' : 'http://foo/db/data/node/760/properties',
                          'incoming_relationships' : 'http://foo/db/data/node/760/relationships/in',
                          'extensions' : {
                          },
                          'create_relationship' : 'http://foo/db/data/node/760/relationships',
                          'paged_traverse' : 'http://foo/db/data/node/760/paged/traverse/{returnType}{?pageSize,leaseTime}',
                          'all_relationships' : 'http://foo/db/data/node/760/relationships/all',
                          'incoming_typed_relationships' : 'http://foo/db/data/node/760/relationships/in/{-list|&|types}'
                        },'from':'/node'},{'id':1,'location':'http://foo/db/data/relationship/756','body':{
                          'start' : 'http://foo/db/data/node/760',
                          'data' : {
                            'Foo' : 123,
                            'Bar' : 456,
                            'Baz' : 789
                          },
                          'property' : 'http://foo/db/data/relationship/756/properties/{key}',
                          'self' : 'http://foo/db/data/relationship/756',
                          'properties' : 'http://foo/db/data/relationship/756/properties',
                          'type' : 'TEST_RELATIONSHIP',
                          'extensions' : {
                          },
                          'end' : 'http://foo/db/data/node/789'
                        },'from':'http://foo/db/data/node/761/relationships'}]"
                    )
                }
            };

            var graphClient = await testHarness.CreateAndConnectGraphClient();

            await graphClient.CreateAsync(
                testNode,
                new TestRelationship(789, testRelationshipPayload));

            testHarness.AssertRequestConstraintsAreMet();
        }

        public class TestNode
        {
            [StringLength(4)]
            public string Foo { get; set; }

            [Required]
            public string Bar { get; set; }

            [RegularExpression(@"\w*")]
            public string Baz { get; set; }

        }

        public class TestNode2
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
        }

        public class NestedTestNode
        {
            public string Foo { get; set; }
            public TestNode2 TestNode2 { get; set; }
        }

        public class TestPayload
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
            public string Baz { get; set; }
        }

        public class TestRelationship : Relationship<TestPayload>,
            IRelationshipAllowingSourceNode<TestNode>,
            IRelationshipAllowingTargetNode<TestNode2>
        {
            public TestRelationship(NodeReference targetNode, TestPayload data)
                : base(targetNode, data)
            {
            }

            public override string RelationshipTypeKey
            {
                get { return "TEST_RELATIONSHIP"; }
            }
        }
    }
}