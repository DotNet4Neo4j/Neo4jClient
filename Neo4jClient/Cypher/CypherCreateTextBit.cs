using System.Collections.Generic;

namespace Neo4jClient.Cypher
{
    internal class CypherCreateTextBit
    {
        readonly string createText;

        public CypherCreateTextBit(string createText)
        {
            this.createText = createText;
        }

        public string CreateText
        {
            get { return createText; }
        }
    }
}