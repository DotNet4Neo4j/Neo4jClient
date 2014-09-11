namespace Neo4jClient.Cypher.EntityExtension
{
    public abstract class BaseRelationship
    {
        protected BaseRelationship(string key):this(key,null,null){}
        protected BaseRelationship(string fromKey, string toKey):this(fromKey+toKey, fromKey,toKey){}
        protected BaseRelationship(string key, string fromKey, string toKey)
        {
            FromKey = fromKey;
            ToKey = toKey;
            Key = key;
        }

        public string FromKey { get; set; }
        public string Key { get; set; }
        public string ToKey { get; set; }
    }
}
