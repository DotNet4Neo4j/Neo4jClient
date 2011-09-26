namespace Neo4jClient.ApiModels
{
    class RootApiResponse
    {
        public string Batch { get; set; }
        public string Node { get; set; }
        public string NodeIndex { get; set; }
        public string RelationshipIndex { get; set; }
        public string ReferenceNode { get; set; }
        public string ExtensionsInfo { get; set; }
        public ExtensionsApiResponse Extensions { get; set; }
    }
}
