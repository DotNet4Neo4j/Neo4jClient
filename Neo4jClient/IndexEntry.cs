using System;
using System.Collections;
using System.Collections.Generic;

namespace Neo4jClient
{
    public class IndexEntry : IEnumerable<KeyValuePair<string, object>>
    {
        readonly IList<KeyValuePair<string, object>> originalKeyValuesList;

        public IndexEntry()
        {
            originalKeyValuesList = null;
            KeyValues = originalKeyValuesList = new List<KeyValuePair<string, object>>();
        }

        public IndexEntry(string name) : this()
        {
            Name = name;
        }

        public string Name { get; set; }
        public IEnumerable<KeyValuePair<string, object>> KeyValues { get; set; }

        public void Add(string key, object value)
        {
            if (originalKeyValuesList == null ||
                !ReferenceEquals(originalKeyValuesList, KeyValues))
                throw new InvalidOperationException(@"You can only call the Add method if you haven't directly set the KeyValues property. Write your code like this:

new IndexEntry(""index-name"")
{
    { ""Foo"", 1234 },
    { ""Bar"", ""Baz"" }
}");

            originalKeyValuesList.Add(new KeyValuePair<string, object>(key, value));
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return KeyValues.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}