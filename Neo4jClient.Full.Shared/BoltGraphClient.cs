﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Neo4jClient.Execution;
using Neo4jClient.Serialization;
using Neo4jClient.Transactions;
using Newtonsoft.Json;

//TODO: Logging
//TODO: Config Stuff
//TODO: Transaction Stuff

namespace Neo4jClient
{
    public partial class BoltGraphClient : IBoltGraphClient, IRawGraphClient, ITransactionalGraphClient
    {
        public BoltGraphClient(Uri uri, string username = null, string password = null, string realm = null)
        {
            if ((uri.Scheme == "http") || (uri.Scheme == "https"))
                throw new NotSupportedException($"To use the {nameof(BoltGraphClient)} you need to provide a 'bolt://' scheme, not '{uri.Scheme}'.");

            this.uri = uri;
            this.username = username;
            this.password = password;
            this.realm = realm;
            PolicyFactory = new ExecutionPolicyFactory(this);

            JsonConverters = new List<JsonConverter>();
            JsonConverters.AddRange(DefaultJsonConverters);
            TypeSerializers = new List<ITypeSerializer>();
            TypeSerializers.AddRange(DefaultSerializers);
            JsonContractResolver = DefaultJsonContractResolver;

            ExecutionConfiguration = new ExecutionConfiguration
            {
                UserAgent = $"Neo4jClient/{GetType().GetTypeInfo().Assembly.GetName().Version}",
                UseJsonStreaming = true,
                JsonConverters = JsonConverters,
                Username = username,
                Password = password,
                Realm = realm
            };

            transactionManager = new BoltTransactionManager(this);
        }
    }
}