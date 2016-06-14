using System;
using System.Runtime.Serialization;
using Neo4jClient.ApiModels;

namespace Neo4jClient
{
    [DataContract]
    public class NeoException : Exception
    {
        readonly string neoMessage;
        readonly string neoException;
        readonly string neoFullName;
        readonly string[] neoStackTrace;

        internal NeoException(ExceptionResponse response)
            : base(response.Exception + ": " + response.Message)
        {
            neoMessage = response.Message;
            neoException = response.Exception;
            neoFullName = response.FullName;
            neoStackTrace = response.StackTrace;
        }

        public string NeoMessage { get { return neoMessage; } }
        public string NeoExceptionName { get { return neoException; } }
        public string NeoFullName { get { return neoFullName; } }
        public string[] NeoStackTrace { get { return neoStackTrace; } }
    }
}
