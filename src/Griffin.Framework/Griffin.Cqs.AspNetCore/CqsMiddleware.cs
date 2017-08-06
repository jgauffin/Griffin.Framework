using Griffin.Cqs.Authorization;
using Griffin.Cqs.Http;
using Griffin.Cqs.Net;
using Griffin.Net.Protocols.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.Cqs.AspNetCore
{
    public class CqsMiddleware
    {
        public const string CqsPath = "/cqs";

        private readonly RequestDelegate _next;
        private readonly CqsMessageProcessor _messageProcessor;
        private readonly CqsObjectMapper _cqsObjectMapper = new CqsObjectMapper();
        private readonly ILogger<CqsMiddleware> _logger;

        public CqsMiddleware(RequestDelegate next, CqsMessageProcessor messageProcessor, ILogger<CqsMiddleware> logger)
        {
            _next = next;
            _messageProcessor = messageProcessor;
            _logger = logger;

            _logger.LogInformation("Griffin.Cqs.AspNetCore is running on path: {0}", CqsPath);
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path != CqsPath)
            {
                await _next(context);
                return;
            }

            var name = context.Request.Headers["X-Cqs-Type"];
            var dotNetType = context.Request.Headers["X-Cqs-Object-Type"];
            var cqsName = context.Request.Headers["X-Cqs-Name"];

            var json = "{}";
            var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
            json = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(json))
                json = "{}";

            object cqsObject;
            if (!string.IsNullOrWhiteSpace(dotNetType))
            {
                cqsObject = _cqsObjectMapper.Deserialize(dotNetType, json);
                if (cqsObject == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Features.Get<IHttpResponseFeature>().ReasonPhrase = "Unknown type: " + dotNetType;
                    _logger.LogWarning("Unknwon type: {0}", dotNetType);
                    return;
                }
            }
            else if (!string.IsNullOrWhiteSpace(cqsName))
            {
                cqsObject = _cqsObjectMapper.Deserialize(cqsName, json);
                if (cqsObject == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Features.Get<IHttpResponseFeature>().ReasonPhrase = "Unknown type: " + cqsName;
                    _logger.LogWarning("Unknwon type: {0}", cqsName);
                    return;
                }
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                const string noCqsNameErrorMessage = "Expected a class name in the header 'X-Cqs-Name' or a .NET type name in the header 'X-Cqs-Object-Type'.";
                context.Features.Get<IHttpResponseFeature>().ReasonPhrase = noCqsNameErrorMessage;
                _logger.LogWarning(noCqsNameErrorMessage);
                return;
            }

            ClientResponse cqsReplyObject = null;
            Exception ex = null;
            try
            {
                cqsReplyObject = await _messageProcessor.ProcessAsync(cqsObject);
            }
            catch (AggregateException e1)
            {
                ex = e1.InnerException;
            }

            if (ex is HttpException)
            {
                _logger.LogWarning("Failed to process {0}, Exception: \r\n{1}", json, ex);
                // TODO: log to OneTrueError or something
                context.Response.StatusCode = ((HttpException)ex).HttpCode;
                context.Features.Get<IHttpResponseFeature>().ReasonPhrase = FirstLine(ex.Message);
                return;
            }
            if (ex is AuthorizationException)
            {
                _logger.LogWarning("Failed to process {0}, Exception: \r\n{1}", json, ex);
                // TODO: log to OneTrueError or something
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Features.Get<IHttpResponseFeature>().ReasonPhrase = FirstLine(ex.Message);
                return;
            }
            if (ex != null)
            {
                _logger.LogError("Failed to process {0}, Exception: \r\n{1}", json, ex.Message);
                // TODO: log to OneTrueError or something
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Features.Get<IHttpResponseFeature>().ReasonPhrase = FirstLine(ex.Message);
                return;
            }

            context.Response.ContentType = "application/json;encoding=utf8";

            if (cqsReplyObject.Body != null)
            {
                context.Response.Headers["X-Cqs-Object-Type"] = cqsReplyObject.Body.GetType().GetSimpleAssemblyQualifiedName();
                context.Response.Headers["X-Cqs-Name"] = cqsReplyObject.Body.GetType().Name;
                if (cqsReplyObject.Body is Exception)
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var contentType = "application/json;encoding=utf8";
                json = _cqsObjectMapper.Deserializer != null ? _cqsObjectMapper.Deserializer.Serialize(cqsReplyObject.Body, out contentType) : JsonConvert.SerializeObject(cqsReplyObject.Body);

                var buffer = Encoding.UTF8.GetBytes(json);
                await context.Response.Body.WriteAsync(buffer, 0, buffer.Length);
            }
            else
                context.Response.StatusCode = (int)HttpStatusCode.NoContent;
        }


        private string FirstLine(string msg)
        {
            var pos = msg.IndexOfAny(new[] { '\r', '\n' });
            if (pos == -1)
                return msg;

            return msg.Substring(0, pos);
        }

    }
}
