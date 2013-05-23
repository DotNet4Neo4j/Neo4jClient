using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Neo4jClient.Cypher
{
    public interface IFluentCypherResultItem
    {
        Node<T> CollectAs<T>();
    }
}
