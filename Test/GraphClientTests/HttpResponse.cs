using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using RestSharp;

namespace Neo4jClient.Test.GraphClientTests
{
    public class HttpResponse : IHttpResponse
    {
        // Fields
        private string _content;
        private ResponseStatus _responseStatus;

        // Methods
        public HttpResponse()
        {
            this.Headers = new List<HttpHeader>();
            this.Cookies = new List<HttpCookie>();
        }

        // Properties
        public string Content
        {
            get 
            {
                return this._content;
            }
            set
            {
                this._content = value;
            }
        }

        public string ContentEncoding { get; set; }

        public long ContentLength { get; set; }

        public string ContentType { get; set; }

        public IList<HttpCookie> Cookies { get; private set; }

        public Exception ErrorException { get; set; }

        public string ErrorMessage { get; set; }

        public IList<HttpHeader> Headers { get; private set; }

        public byte[] RawBytes { get; set; }

        public ResponseStatus ResponseStatus
        {
            get
            {
                return this._responseStatus;
            }
            set
            {
                this._responseStatus = value;
            }
        }

        public Uri ResponseUri { get; set; }

        public string Server { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public string StatusDescription { get; set; }
    }

 

}
