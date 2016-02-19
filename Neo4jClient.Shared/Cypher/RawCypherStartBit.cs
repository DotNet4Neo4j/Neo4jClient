using System;

namespace Neo4jClient.Cypher
{
    [Obsolete("Use IGraphClient.Cypher.Start(new { foo = \"bar\" }) instead. See https://bitbucket.org/Readify/neo4jclient/issue/74/support-nicer-cypher-start-notation for more details about this change.")]
    public class RawCypherStartBit : ICypherStartBit
    {
        readonly string identifier;
        readonly string startText;

        public RawCypherStartBit(string identifier, string startText)
        {
            this.identifier = identifier;
            this.startText = startText;
        }

        public string Identifier
        {
            get { return identifier; }
        }

        public string StartText
        {
            get { return startText; }
        }

        public string ToCypherText(Func<object, string> createParameterCallback)
        {
            return identifier + "=" + startText;
        }
    }
}
