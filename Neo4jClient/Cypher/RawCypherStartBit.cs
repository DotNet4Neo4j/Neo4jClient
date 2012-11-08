namespace Neo4jClient.Cypher
{
    internal class RawCypherStartBit
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
    }
}
