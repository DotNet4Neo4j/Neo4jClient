namespace Neo4jClient.Serialization
{
    public interface ISerializer
    {
        string Serialize(object toSerialize);
    }
}