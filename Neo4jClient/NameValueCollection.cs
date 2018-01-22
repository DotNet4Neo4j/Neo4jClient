using System.Collections.Generic;
using System.Linq;

namespace Neo4jClient
{
    public class NameValueCollection
    {
        private readonly IDictionary<string, string> contents = new Dictionary<string, string>();

        public string[] AllKeys => contents.Keys.ToArray();
        public long Count => contents.Count;

        public string Get(string customHeaderKey)
        {
            return contents[customHeaderKey];
        }

        public void Add(string headerName, string headerValue)
        {
            contents.Add(headerName, headerValue);
        }
    }
}