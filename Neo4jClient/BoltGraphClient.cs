using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Neo4j.Driver.V1;
using Neo4jClient.Execution;
using Neo4jClient.Transactions;
using Newtonsoft.Json;

//TODO: Logging
//TODO: Config Stuff
//TODO: Transaction Stuff

namespace Neo4jClient
{
    public partial class BoltGraphClient : IBoltGraphClient, IRawGraphClient, ITransactionalGraphClient
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="BoltGraphClient" />.
        /// </summary>
        /// <param name="uri">
        ///     If the <paramref name="uris" /> parameter is provided, this will be treated as a <em>virtual URI</em>
        ///     , else it will be the URI connected to.
        /// </param>
        /// <param name="uris">
        ///     A collection of <see cref="Uri" /> instances to connect to using an
        ///     <see cref="IServerAddressResolver" />. Leave <c>null</c> (or empty) if you don't want to use it.
        /// </param>
        /// <param name="username">The username to connect to Neo4j with.</param>
        /// <param name="password">The password to connect to Neo4j with.</param>
        /// <param name="realm">The realm to connect to Neo4j with.</param>
        public BoltGraphClient(Uri uri, IEnumerable<Uri> uris, string username = null, string password = null, string realm = null)
        {
            var localUris = uris?.ToList();
            if (localUris != null && localUris.Any())
            {
                if(uri.Scheme.ToLowerInvariant() != "bolt+routing")
                    throw new NotSupportedException($"To use the {nameof(BoltGraphClient)} you need to provide a 'bolt://' scheme, not '{uri.Scheme}'.");

                addressResolver = new AddressResolver(uri, localUris);
            }
            else if (uri.Scheme.ToLowerInvariant() != "bolt" && uri.Scheme.ToLowerInvariant() != "bolt+routing")
                throw new NotSupportedException($"To use the {nameof(BoltGraphClient)} you need to provide a 'bolt://' or 'bolt+routing://' scheme, not '{uri.Scheme}'.");

            
            this.uri = uri;
            this.username = username;
            this.password = password;
            this.realm = realm;
            PolicyFactory = new ExecutionPolicyFactory(this);

            JsonConverters = new List<JsonConverter>();
            JsonConverters.AddRange(DefaultJsonConverters);
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