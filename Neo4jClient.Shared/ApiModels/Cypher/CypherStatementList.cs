using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Neo4jClient.Cypher;
using Newtonsoft.Json;

namespace Neo4jClient.ApiModels.Cypher
{
    /// <summary>
    /// Represents the collection of Cypher statements that are going to be sent through a transaction.
    /// </summary>
    [JsonObject]
    class CypherStatementList : IList<CypherTransactionStatement>
    {
        private readonly IList<CypherTransactionStatement> _statements;

        public CypherStatementList()
        {
            _statements = new List<CypherTransactionStatement>();
        }

        public CypherStatementList(IEnumerable<CypherQuery> queries)
        {
            _statements = queries
                .Select(query => new CypherTransactionStatement(query, query.ResultFormat == CypherResultFormat.Rest))
                .ToList();
        }

        [JsonProperty("statements")]
        public IList<CypherTransactionStatement> Statements
        {
            get { return _statements; }
        }

        public IEnumerator<CypherTransactionStatement> GetEnumerator()
        {
            return _statements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _statements).GetEnumerator();
        }

        public void Add(CypherTransactionStatement item)
        {
            _statements.Add(item);
        }

        public void Clear()
        {
            _statements.Clear();
        }

        public bool Contains(CypherTransactionStatement item)
        {
            return _statements.Contains(item);
        }

        public void CopyTo(CypherTransactionStatement[] array, int arrayIndex)
        {
            _statements.CopyTo(array, arrayIndex);
        }

        public bool Remove(CypherTransactionStatement item)
        {
            return _statements.Remove(item);
        }

        [JsonIgnore]
        public int Count
        {
            get { return _statements.Count; }
        }

        [JsonIgnore]
        public bool IsReadOnly
        {
            get { return _statements.IsReadOnly; }
        }

        public int IndexOf(CypherTransactionStatement item)
        {
            return _statements.IndexOf(item);
        }

        public void Insert(int index, CypherTransactionStatement item)
        {
            _statements.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _statements.RemoveAt(index);
        }

        [JsonIgnore]
        public CypherTransactionStatement this[int index]
        {
            get { return _statements[index]; }
            set { _statements[index] = value; }
        }
    }
}
