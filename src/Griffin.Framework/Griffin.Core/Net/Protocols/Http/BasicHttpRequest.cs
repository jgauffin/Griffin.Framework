using System;
using System.Net;
using Griffin.Net.Protocols.Http.Messages;

namespace Griffin.Net.Protocols.Http
{
    public class BasicHttpRequest : HttpMessage, IHttpRequest
    {
        private readonly HttpFileCollection _files = new HttpFileCollection();
        private readonly ParameterCollection _form = new ParameterCollection();
        private string _httpMethod;
        private string _pathAndQuery;
        private Uri _uri;

        /// <summary>
        /// </summary>
        /// <param name="httpMethod">Method like <c>POST</c>.</param>
        /// <param name="pathAndQuery">Absolute path and query string (if one exist)</param>
        /// <param name="httpVersion">HTTP version like <c>HTTP/1.1</c></param>
        public BasicHttpRequest(string httpMethod, string pathAndQuery, string httpVersion)
            : base(httpVersion)
        {
            HttpMethod = httpMethod;
            Uri = new Uri("http://notspecified" + pathAndQuery);
        }

        /// <summary>
        ///     Method which was invoked.
        /// </summary>
        /// <remarks>
        ///     <para>Typically <c>GET</c>, <c>POST</c>, <c>PUT</c>, <c>DELETE</c> or <c>HEAD</c>.</para>
        /// </remarks>
        public string HttpMethod
        {
            get { return _httpMethod; }
            internal set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");

                _httpMethod = value;
            }
        }

        /// <summary>
        ///     Request UrI
        /// </summary>
        /// <remarks>
        ///     <para>Is built using the <c>server</c> header and the path + query which is included in the request line</para>
        ///     <para>If no <c>server</c> header is included "localhost" will be used as server.</para>
        /// </remarks>
        public Uri Uri
        {
            get { return _uri; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                _uri = value;
                _pathAndQuery = value.PathAndQuery;
            }
        }

        /// <inheritdoc />
        public EndPoint RemoteEndPoint { get; set; }

        /// <inheritdoc />
        public IParameterCollection Form
        {
            get { return _form; }
        }

        /// <inheritdoc />
        public IHttpFileCollection Files
        {
            get { return _files; }
        }

        IHttpResponse IHttpRequest.CreateResponse()
        {
            return new BasicHttpResponse(200, "", "HTTP/1.1");
        }

        /// <summary>
        ///     Status line for requests is "HttpMethod PathAndQuery HttpVersion"
        /// </summary>
        public override string StatusLine
        {
            get { return HttpMethod + " " + _pathAndQuery + " " + HttpVersion; }
        }

        /// <inheritdoc />
        protected override void OnHeaderSet(string name, string value)
        {
            if (name.Equals("host", StringComparison.OrdinalIgnoreCase))
                Uri = new Uri("http://" + value + _pathAndQuery);


            base.OnHeaderSet(name, value);
        }
    }
}