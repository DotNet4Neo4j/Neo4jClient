using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace Neo4jClient
{
    internal class DriverWrapper : IDriver
    {
        private readonly IDriver driver;
        public string Username { get;  }
        public string Password { get; }
        public string Realm { get; }
        public EncryptionLevel? EncryptionLevel { get; }

        public DriverWrapper(IDriver driver)
        {
            this.driver = driver;
        }

        public DriverWrapper(string uri, IServerAddressResolver addressResolver, string username, string pass, string realm, EncryptionLevel? encryptionLevel)
            :this(new Uri(uri), addressResolver, username, pass, realm, encryptionLevel)
        {
        }

        public DriverWrapper(Uri uri, IServerAddressResolver addressResolver, string username, string pass, string realm, EncryptionLevel? encryptionLevel)
        {
            Uri = uri;
            Username = username;
            Password = pass;
            Realm = realm;
            EncryptionLevel = encryptionLevel;

            var authToken = GetAuthToken(username, pass, realm);
            if (addressResolver != null)
            {
                driver = encryptionLevel == null 
                    ? GraphDatabase.Driver(uri, authToken, builder => builder.WithResolver(addressResolver)) 
                    : GraphDatabase.Driver(uri, authToken, builder => builder.WithResolver(addressResolver).WithEncryptionLevel(encryptionLevel.Value));
            }
            else
            {
                driver = GraphDatabase.Driver(uri, authToken);
            }
        }

        public IAsyncSession Session()
        {
            return driver.AsyncSession();
        }

        public IAsyncSession Session(AccessMode defaultMode)
        {
            return driver.AsyncSession(x => x.WithDefaultAccessMode(defaultMode));
        }

        public IAsyncSession Session(string bookmark)
        {
            return driver.AsyncSession(x => x.WithBookmarks(Bookmark.From(bookmark)));
        }

        public IAsyncSession Session(AccessMode defaultMode, string bookmark)
        {
            return driver.AsyncSession(x => x.WithDefaultAccessMode(defaultMode).WithBookmarks(Bookmark.From(bookmark)));
        }

        public IAsyncSession Session(AccessMode defaultMode, IEnumerable<string> bookmarks)
        {
            return driver.AsyncSession(x =>
            {
                x.WithDefaultAccessMode(defaultMode);
                if (bookmarks != null) x.WithBookmarks(Bookmark.From(bookmarks.ToArray()));
            });
        }

        public IAsyncSession Session(IEnumerable<string> bookmarks)
        {
            return driver.AsyncSession(x =>
            {
                if (bookmarks != null) x.WithBookmarks(Bookmark.From(bookmarks.ToArray()));
            });
        }
        
        public IAsyncSession AsyncSession()
        {
            return driver.AsyncSession();
        }

        public IAsyncSession AsyncSession(Action<SessionConfigBuilder> action)
        {
            return driver.AsyncSession(action);
        }

        public Task CloseAsync()
        {
            return driver.CloseAsync();
        }

        public Task VerifyConnectivityAsync()
        {
            return driver.VerifyConnectivityAsync();
        }

        public Task<bool> SupportsMultiDbAsync()
        {
            return driver.SupportsMultiDbAsync();
        }

        public Config Config => driver.Config;

        public Uri Uri { get; private set; }

        public IServerAddressResolver AddressResolver => Config.Resolver;

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