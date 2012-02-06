using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neo4jClient.Cypher
{
    public class CypherQueryBuilder
    {
        readonly IList<CypherStartBit> startBits = new List<CypherStartBit>();
        public string[] ReturnIdentites { get; set; }
        public int? Limit { get; set; }

        public void AddStartBit(string identity, params NodeReference[] nodeReferences)
        {
            startBits.Add(new CypherStartBit(identity, "node", nodeReferences.Select(r => r.Id).ToArray()));
        }

        public void AddStartBit(string identity, params RelationshipReference[] relationshipReferences)
        {
            startBits.Add(new CypherStartBit(identity, "relationship", relationshipReferences.Select(r => r.Id).ToArray()));
        }

        public ICypherQuery ToQuery()
        {
            var queryTextBuilder = new StringBuilder();
            var queryParameters = new Dictionary<string, object>();

            WriteStartClause(queryTextBuilder, queryParameters);
            WriteReturnClause(queryTextBuilder);
            WriteLimitClause(queryTextBuilder, queryParameters);

            return new CypherQuery(queryTextBuilder.ToString(), queryParameters);
        }

        static string CreateParameter(IDictionary<string, object> parameters, object paramValue)
        {
            var paramName = string.Format("p{0}", parameters.Count);
            parameters.Add(paramName, paramValue);
            return "{" + paramName + "}";
        }

        void WriteStartClause(StringBuilder target, IDictionary<string, object> paramsDictionary)
        {
            target.Append("START ");

            var formattedStartBits = startBits.Select(bit =>
            {
                var lookupIdParameterNames = bit
                    .LookupIds
                    .Select(i => CreateParameter(paramsDictionary, i))
                    .ToArray();

                var lookupContent = string.Join(", ", lookupIdParameterNames);

                return string.Format("{0}={1}({2})", bit.Identity, bit.LookupType, lookupContent);
            });

            target.Append(string.Join(", ", formattedStartBits));
        }

        void WriteReturnClause(StringBuilder target)
        {
            if (ReturnIdentites == null) return;
            target.Append("\r\nRETURN ");
            target.Append(string.Join(", ", ReturnIdentites));
        }

        void WriteLimitClause(StringBuilder target, IDictionary<string, object> paramsDictionary)
        {
            if (Limit == null) return;
            target.AppendFormat("\r\nLIMIT {0}", CreateParameter(paramsDictionary, Limit));
        }
    }
}
