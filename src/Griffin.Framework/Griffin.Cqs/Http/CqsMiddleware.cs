using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Griffin.Cqs;
using Griffin.Cqs.Authorization;
using Griffin.Cqs.Http;
using Griffin.Cqs.Net;
using Griffin.Net;
using Griffin.Net.Protocols.Http;
using Griffin.Net.Protocols.Http.Middleware;
using Newtonsoft.Json;

namespace Griffin.Http
{
    /// <summary>
    ///     Receives CQS objects over HTTP, processes them and return replies.
    /// </summary>
    public class CqsMiddleware : HttpMiddleware
    {
        private readonly CqsObjectMapper _cqsObjectMapper;
        private readonly CqsMessageProcessor _messageProcessor;
        private Action<string> _logger = s => { };

        /// <summary>
        ///     Initializes a new instance of the <see cref="CqsMiddleware" /> class.
        /// </summary>
        /// <param name="messageProcessor">Used to execute the actual messages.</param>
        /// <param name="objectMapper">Used to map type names to .NET types.</param>
        public CqsMiddleware(CqsMessageProcessor messageProcessor, CqsObjectMapper objectMapper = null)
        {
            _messageProcessor = messageProcessor ?? throw new ArgumentNullException(nameof(messageProcessor));
            _cqsObjectMapper = objectMapper ?? new CqsObjectMapper();
        }

        /// <summary>
        ///     Will use the internal JSON serializer if this property is not specified.
        /// </summary>
        public ICqsDeserializer CqsSerializer
        {
            get => _cqsObjectMapper.Deserializer;
            set => _cqsObjectMapper.Deserializer = value;
        }

        /// <summary>
        ///     Assign to get important log messages (typically errors)
        /// </summary>
        public Action<string> Logger
        {
            get => _logger;
            set
            {
                if (value == null)
                    _logger = s => { };
                else
                    _logger = value;
            }
        }

        /// <summary>
        ///     Map a type directly.
        /// </summary>
        /// <param name="type">Must implement one of the handler interfaces.</param>
        /// <exception cref="System.ArgumentNullException">type</exception>
        /// <exception cref="System.ArgumentException">
        ///     ' + type.FullName + ' do not implement one of the handler interfaces.
        ///     or
        ///     ' + type.FullName + ' is abstract or an interface.
        /// </exception>
        /// <exception cref="System.InvalidOperationException">
        ///     Duplicate mappings for a name (two different handlers may not have
        ///     the same class name).
        /// </exception>
        /// <remarks>
        ///     Required if the HTTP client do not supply the full .NET type name (just the class name of the command/query).
        /// </remarks>
        public void Map(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            _cqsObjectMapper.Map(type);
        }


        public override async Task Process(HttpContext context, Func<Task> next)
        {
            var request = context.Request;
            var response = context.Response;

            var json = "{}";
            if (request.Body != null)
            {
                var reader = new StreamReader(request.Body);
                json = await reader.ReadToEndAsync();
            }


            var cqsObject = DeserializeRequestBody(request, response, json);
            if (cqsObject == null)
            {
                if (response.StatusCode < 300)
                    await next();
                else
                    return;
            }

            ClientResponse cqsReplyObject = null;
            Exception ex = null;
            try
            {
                cqsReplyObject = _messageProcessor.ProcessAsync(cqsObject).Result;
            }
            catch (AggregateException e1)
            {
                ex = e1.InnerException;
            }

            if (ex is HttpException)
            {
                Logger($"Failed to process {json}, Exception:\r\n{ex}");
                response.StatusCode = ((HttpException) ex).HttpCode;
                response.ReasonPhrase = FirstLine(ex.Message);
                return;
            }

            if (ex is AuthorizationException)
            {
                Logger($"Failed to process {json}, Exception:\r\n{ex}");
                var authEx = (AuthorizationException) ex;
                response.StatusCode = 401;
                response.ReasonPhrase = FirstLine(ex.Message);
                return;
            }

            if (ex != null)
            {
                Logger($"Failed to process {json}, Exception:\r\n{ex}");
                response.StatusCode = 500;
                response.ReasonPhrase = FirstLine(ex.Message);
                return;
            }


            // for instance commands do not have a return value.
            if (cqsReplyObject?.Body != null)
            {
                response.ContentType = "application/json;encoding=utf8";
                response.AddHeader("X-Cqs-Object-Type", cqsReplyObject.Body.GetType().GetSimpleAssemblyQualifiedName());
                response.AddHeader("X-Cqs-Name", cqsReplyObject.Body.GetType().Name);
                if (cqsReplyObject.Body is Exception)
                    response.StatusCode = 500;

                var contentType = "application/json;encoding=utf8";
                json = CqsSerializer == null
                    ? JsonConvert.SerializeObject(cqsReplyObject.Body)
                    : CqsSerializer.Serialize(cqsReplyObject.Body, out contentType);
                response.ContentType = contentType;

                var buffer = Encoding.UTF8.GetBytes(json);
                response.Body = new MemoryStream();
                response.Body.Write(buffer, 0, buffer.Length);
                response.Body.Position = 0;
            }
            else
                response.StatusCode = (int) HttpStatusCode.NoContent;
        }

        /// <summary>
        ///     Scan assembly for handlers.
        /// </summary>
        /// <remarks>
        ///     Required if the HTTP client do not supply the full .NET type name (just the class name of the command/query).
        /// </remarks>
        /// <param name="assembly">The assembly to scan for handlers.</param>
        /// <exception cref="System.InvalidOperationException">
        ///     Duplicate mappings for a name (two different handlers may not have
        ///     the same class name).
        /// </exception>
        public void ScanAssembly(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            _cqsObjectMapper.ScanAssembly(assembly);
        }

        private object DeserializeRequestBody(HttpRequest request, HttpResponse response, string json)
        {
            object cqsObject;
            var dotNetType = request.Headers["X-Cqs-Object-Type"] ?? request.Headers["X-Cqs-Type"];
            var cqsName = request.Headers["X-Cqs-Name"];
            if (!string.IsNullOrEmpty(dotNetType))
            {
                cqsObject = _cqsObjectMapper.Deserialize(dotNetType, json);
                if (cqsObject != null)
                    return cqsObject;

                response.StatusCode = 400;
                response.ReasonPhrase = $"Unknown type: {dotNetType}";
                Logger($"Unknown type: {dotNetType} for {request.Uri}");
                return null;
            }

            if (string.IsNullOrEmpty(cqsName))
                return null;

            cqsObject = _cqsObjectMapper.Deserialize(cqsName, json);
            if (cqsObject != null)
                return cqsObject;

            response.StatusCode = 400;
            response.ReasonPhrase = $"Unknown type: {cqsName}";
            Logger($"Unknown type: {cqsName} for {request.Uri}");
            return null;
        }

        private string FirstLine(string msg)
        {
            var pos = msg.IndexOfAny(new[] {'\r', '\n'});
            if (pos == -1)
                return msg;

            return msg.Substring(0, pos);
        }
    }
}