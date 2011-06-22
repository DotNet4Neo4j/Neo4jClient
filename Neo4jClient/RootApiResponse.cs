namespace Neo4jClient
{
    class RootApiResponse
    {
        public string Node { get; set; }
        public string NodeIndex { get; set; }
        public string RelationshipIndex { get; set; }
        public string ReferenceNode { get; set; }
        public string ExtensionsInfo { get; set; }
        public Extensions Extensions { get; set; }
    }
}