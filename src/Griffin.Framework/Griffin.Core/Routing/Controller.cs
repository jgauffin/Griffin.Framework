using Griffin.Net.Protocols.Http;
using System.IO;
using System.Net;

namespace Griffin.Routing
{
    /// <summary>
    /// Request Controller base class.
    /// </summary>
    /// <remarks>
    /// All controllers must implement this class.
    /// </remarks>
    public abstract class Controller
    {
        /// <summary>
        /// Gets the request.
        /// </summary>
        /// <value>The request.</value>
        public HttpRequest Request { get; internal set; }
        
        /// <summary>
        /// Gets the headers.
        /// </summary>
        /// <value>The headers.</value>
        public IHeaderCollection Headers { get { return Request.Headers; } }
        /// <summary>
        /// Gets the files.
        /// </summary>
        /// <value>The files.</value>
        public IHttpFileCollection Files { get { return Request.Files; } }
        /// <summary>
        /// Gets the post.
        /// </summary>
        /// <value>The post.</value>
        public IParameterCollection Post { get { return Request.Form; } }
        /// <summary>
        /// Gets the form.
        /// </summary>
        /// <value>The form.</value>
        public IParameterCollection Form { get { return Request.Form; } }
        /// <summary>
        /// Gets the cookies.
        /// </summary>
        /// <value>The cookies.</value>
        public IHttpCookieCollection<IHttpCookie> Cookies { get { return Request.Cookies; } }
        /// <summary>
        /// Gets the http method.
        /// </summary>
        /// <value>The http method.</value>
        public string HttpMethod { get { return Request.HttpMethod; } }
        /// <summary>
        /// Gets the remote end point.
        /// </summary>
        /// <value>The remote end point.</value>
        public EndPoint RemoteEndPoint { get { return Request.RemoteEndPoint; } }
        /// <summary>
        /// Gets the content charset.
        /// </summary>
        /// <value>The content charset.</value>
        public System.Text.Encoding ContentCharset { get { return Request.ContentCharset; } }
        /// <summary>
        /// Gets or sets the type of the content.
        /// </summary>
        /// <value>The type of the content.</value>
        public string ContentType
        {
            get { return Request.ContentType; }
            set { Request.ContentType = value; }
        }
        
        /// <summary>
        /// Creates the response.
        /// </summary>
        /// <returns>The response.</returns>
        public HttpResponse CreateResponse()
        {
            return Request.CreateResponse();
        }
        #region Download Helper
        /// <summary>
        /// Downloads the text with the given dlFilename.
        /// </summary>
        /// <returns>The text.</returns>
        /// <param name="data">Data.</param>
        /// <param name="dlFilename">Dl filename (filename on the client).</param>
        /// <param name="contentType"></param>
        public HttpResponse DownloadText(string data, string dlFilename, string contentType)
        {
            var resp = CreateResponse ();

            resp.ContentType = contentType;
            resp.AddHeader("Content-Disposition", "attachment; filename=" + dlFilename);
            resp.AddHeader("Content-Description", "File Transfer");
            resp.AddHeader("Pragma", "Public");
            resp.Body = new MemoryStream (ContentCharset.GetBytes (data));

            return resp;
        }
        /// <summary>
        /// Downloads the file with the given dlFilename.
        /// </summary>
        /// <returns>The file.</returns>
        /// <param name="filename">Filename on the server.</param>
        /// <param name="dlFilename">Dl filename (filename on the client).</param>
        /// <param name="contentType"></param>
        public HttpResponse DownloadFile(string filename, string dlFilename, string contentType)
        {
            var resp = CreateResponse ();

            resp.ContentType = contentType;
            resp.AddHeader("Content-Disposition", "attachment; filename=" + dlFilename);
            resp.AddHeader("Content-Description", "File Transfer");
            resp.AddHeader("Pragma", "Public");

            var fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            resp.Body = fs;

            return resp;
        }
        /// <summary>
        /// Downloads the stream with the given dlFilename.
        /// </summary>
        /// <returns>The stream.</returns>
        /// <param name="data">Data.</param>
        /// <param name="dlFilename">Dl filename (filename on the client).</param>
        /// <param name="contentType"></param>
        public HttpResponse DownloadStream(Stream data, string dlFilename, string contentType)
        {
            var resp = CreateResponse ();

            resp.ContentType = contentType;
            resp.AddHeader("Content-Disposition", "attachment; filename=" + dlFilename);
            resp.AddHeader("Content-Description", "File Transfer");
            resp.AddHeader("Pragma", "Public");
            resp.Body = data;

            return resp;
        }
        #endregion
        #region OK Helper
        /// <summary>
        /// Creates an 200 response object with the given text as body
        /// </summary>
        /// <returns>The text.</returns>
        /// <param name="text">Text.</param>
        public HttpResponse OkText(string text)
        {
            var resp = CreateResponse();

            resp.ContentType = "text/plain";
            resp.Body = new MemoryStream(ContentCharset.GetBytes(text));

            return resp;
        }
        /// <summary>
        /// Creates an 200 response object with the given text as body and the given
        /// contentType as Content-Type
        /// </summary>
        /// <returns>The text.</returns>
        /// <param name="text">Text.</param>
        /// <param name="contentType">Content type.</param>
        public HttpResponse OkText(string text, string contentType)
        {
            var resp = CreateResponse();

            resp.ContentType = contentType;
            resp.Body = new MemoryStream(ContentCharset.GetBytes(text));

            return resp;
        }

        /// <summary>
        /// Creates an 200 response object with the file content at filename as body
        /// and Content-Type from the file extesion
        /// </summary>
        /// <returns>The file.</returns>
        /// <param name="filename">Filename.</param>
        /// <param name="contentType"></param>
        public HttpResponse OkFile(string filename, string contentType)
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException("the given filename was not found", filename);

            var resp = CreateResponse();

            var fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            resp.Body = fs;
            resp.ContentType = contentType;

            return resp;
        }

        /// <summary>
        /// Creates an 200 response object with the given stream content as body
        /// and the contentType as Content-Type
        /// </summary>
        /// <returns>The stream.</returns>
        /// <param name="stream">Stream.</param>
        /// <param name="contentType">Content type.</param>
        public HttpResponse OkStream(Stream stream, string contentType)
        {
            var resp = CreateResponse();

            resp.Body = stream;
            resp.ContentType = contentType;

            return resp;
        }
        #endregion
        #region Other Helper (Redirect, ...)
        /// <summary>
        /// Creates an basic 302 redirect response object with the given target
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public HttpResponse Redirect(string target)
        {
            var resp = CreateResponse();

            resp.StatusCode = (int)HttpStatusCode.Redirect;
            resp.AddHeader("Location", target);

            return resp;
        }
        #endregion
        #region Error Helper
        /// <summary>
        /// Creates an basic 404 response object with the given text as body
        /// </summary>
        /// <param name="text">Text.</param>
        public HttpResponse Error404(string text)
        {
            var resp = CreateResponse();

            resp.StatusCode = (int)HttpStatusCode.NotFound;
            resp.Body = new MemoryStream(ContentCharset.GetBytes(text));
            resp.ContentType = "text/plain";

            return resp;
        }
        /// <summary>
        /// Creates an basic response object with the given status code StatusCode and the given
        /// text as body
        /// </summary>
        /// <param name="code">Code.</param>
        /// <param name="text">Text.</param>
        public HttpResponse Error(HttpStatusCode code, string text)
        {
            var resp = CreateResponse();

            resp.StatusCode = (int)code;
            resp.Body = new MemoryStream(ContentCharset.GetBytes(text));
            resp.ContentType = "text/plain";

            return resp;
        }
        #endregion
    }
}
