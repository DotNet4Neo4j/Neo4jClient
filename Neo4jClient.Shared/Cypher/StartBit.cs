using System;
using CreateParameterCallback = System.Func<object, string>;

namespace Neo4jClient.Cypher
{
    public class StartBit
    {
        readonly Func<CreateParameterCallback, string> formatCallback;

        public StartBit(Func<CreateParameterCallback, string> formatCallback)
        {
            this.formatCallback = formatCallback;
        }

        public string ToCypherText(CreateParameterCallback createParameterCallback)
        {
            return formatCallback(createParameterCallback);
        }
    }
}
