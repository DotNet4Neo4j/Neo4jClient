using System;
using System.Collections.Generic;
using System.Linq;
using Neo4j.Driver.V1;

namespace Neo4jClient
{
    internal class AddressResolver : IServerAddressResolver
    {
        private readonly Dictionary<ServerAddress, List<ServerAddress>> servers = new Dictionary<ServerAddress, List<ServerAddress>>();

        public AddressResolver()
            :this(null, new string[0])
        { }

        public AddressResolver(string virtualUri, IEnumerable<string> uris)
        :this(UriCreator.From(virtualUri), uris?.Select(UriCreator.From))
        {}

        public AddressResolver(Uri virtualUri, IEnumerable<Uri> uris)
        {
            servers.Add(ServerAddress.From(virtualUri), uris == null
                ? new List<ServerAddress>()
                : new List<ServerAddress>(uris.Select(ServerAddress.From)));
        }

        public ISet<ServerAddress> Resolve(ServerAddress address)
        {
            if(address == null && servers.Keys.Count > 1)
                throw new InvalidOperationException("Unable to resolve addresses to use as no virtual uri passed in.");
            
            var localServers = address == null || !servers.ContainsKey(address) ? servers.First().Value : servers[address];
            return new HashSet<ServerAddress>(localServers);
        }
    }
}