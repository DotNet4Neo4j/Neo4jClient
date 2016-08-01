using System.Collections.Generic;

namespace Neo4jClient
{
    public class NameValueCollection
    {
        private readonly IDictionary<string, string> contents = new Dictionary<string, string>();

        public string[] AllKeys { get; set; }
        public long Count { get; set; }

        public string Get(string customHeaderKey)
        {
            throw new System.NotImplementedException();
        }
    }
}