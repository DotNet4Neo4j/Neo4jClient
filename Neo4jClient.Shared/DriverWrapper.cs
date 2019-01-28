using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver.V1;

namespace Neo4jClient
{
    /*
     *
     private IDriver CreateDriverWithCustomResolver(string virtualUri, IAuthToken token,
    params ServerAddress[] addresses)
{
    return GraphDatabase.Driver(virtualUri, token,
        new Config {Resolver = new ListAddressResolver(addresses), EncryptionLevel = EncryptionLevel.None});
}

public void AddPerson(string name)
{
    using (var driver = CreateDriverWithCustomResolver("bolt+routing://x.acme.com",
        AuthTokens.Basic(Username, Password),
        ServerAddress.From("a.acme.com", 7687), ServerAddress.From("b.acme.com", 7877),
        ServerAddress.From("c.acme.com", 9092)))
    {
        using (var session = driver.Session())
        {
            session.Run("CREATE (a:Person {name: $name})", new {name});
        }
    }
}

private class ListAddressResolver : IServerAddressResolver
{
    private readonly ServerAddress[] servers;

    public ListAddressResolver(params ServerAddress[] servers)
    {
        this.servers = servers;
    }

    public ISet<ServerAddress> Resolve(ServerAddress address)
    {
        return new HashSet<ServerAddress>(servers);
    }
}
     *
     */

    internal class DriverWrapper : IDriver
    {
        private readonly IDriver driver;
        public string Username { get;  }
        public string Password { get; }
        public string Realm { get; }

        public DriverWrapper(IDriver driver)
        {
            this.driver = driver;
        }

        public DriverWrapper(string uri, IServerAddressResolver addressResolver, string username, string pass, string realm)
            :this(new Uri(uri), addressResolver, username, pass, realm)
        {
        }

        public DriverWrapper(Uri uri, IServerAddressResolver addressResolver, string username, string pass, string realm)
        {
            Uri = uri;
            Username = username;
            Password = pass;
            Realm = realm;

            var authToken = GetAuthToken(username, pass, realm);
            this.driver = addressResolver == null
                ? GraphDatabase.Driver(uri, authToken) 
                : GraphDatabase.Driver(uri, authToken, new Config { Resolver = addressResolver });
        }
        
        public ISession Session()
        {
            return driver.Session();
        }

        public ISession Session(AccessMode defaultMode)
        {
            return driver.Session(defaultMode);
        }

        public ISession Session(string bookmark)
        {
            return driver.Session(bookmark);
        }

        public ISession Session(AccessMode defaultMode, string bookmark)
        {
            return driver.Session(defaultMode, bookmark);
        }

        public ISession Session(AccessMode defaultMode, IEnumerable<string> bookmarks)
        {
            return driver.Session(defaultMode, bookmarks);
        }

        public ISession Session(IEnumerable<string> bookmarks)
        {
            return driver.Session(bookmarks);
        }

        public void Close()
        {
            driver.Close();
        }

        public Task CloseAsync()
        {
            return driver.CloseAsync();
        }

        public Uri Uri { get; }

        public IServerAddressResolver AddressResolver { get; }

        private static IAuthToken GetAuthToken(string username, string password, string realm)
        {
            return string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)
                ? AuthTokens.None
                : AuthTokens.Basic(username, password, realm);
        }
        public void Dispose()
        {
            driver?.Dispose();
        }
    }
}