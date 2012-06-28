using System.Collections.Generic;

namespace Neo4jClient.Cypher
{
    public interface ICypherResultItem
    {
        T As<T>();
        T CollectAs<T>();
        Node<T> Node<T>();
    }
}
