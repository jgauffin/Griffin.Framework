using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Griffin.Cqs;
using Griffin.Cqs.Http;
using Griffin.Cqs.Net;
using Griffin.Net.Protocols.Http;
using Griffin.Net.Protocols.Http.WebSocket;
using Newtonsoft.Json;

namespace Griffin.Http
{
    /// <summary>
    ///     CQS server that works over WebSockets.
    /// </summary>
    public class CqsWebSocketMiddleware : WebSocketMiddleware
    {
        private readonly CqsObjectMapper _cqsObjectMapper = new CqsObjectMapper();
        private readonly CqsMessageProcessor _messageProcessor;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CqsWebSocketMiddleware" /> class.
        /// </summary>
        /// <param name="messageProcessor">Used to execute the actual messages.</param>
        public CqsWebSocketMiddleware(CqsMessageProcessor messageProcessor)
        {
            _messageProcessor = messageProcessor ?? throw new ArgumentNullException(nameof(messageProcessor));
        }

        /// <summary>
        ///     Will use the internal JSON serializer if this property is not specified.
        /// </summary>
        public ICqsDeserializer CqsSerializer
        {
            get => _cqsObjectMapper.Deserializer;
            set => _cqsObjectMapper.Deserializer = value;
        }


        public override async Task Process(WebsocketContext context, Func<Task> next)
        {
            var request = context.Request;
            if (request.OpCode != WebSocketOpcode.Text)
            {
                await next();
                return;
            }

            var streamReader = new StreamReader(request.Payload);
            var data = streamReader.ReadToEnd();
            var pos = data.IndexOf(':');
            if (pos == -1 || pos > 50)
            {
                await next();
                return;
            }

            var cqsName = data.Substring(0, pos);
            var json = data.Substring(pos + 1);

            var cqsObject = _cqsObjectMapper.Deserialize(cqsName, json);
            if (cqsObject == null)
            {
                var response = CreateWebSocketResponse($@"{{ ""error"": ""Unknown type: {cqsName}"" }}");
                await context.Reply(response);
                return;
            }

            ClientResponse cqsReplyObject;
            try
            {
                cqsReplyObject = await _messageProcessor.ProcessAsync(cqsObject);
            }
            catch (HttpException ex)
            {
                var responseJson = JsonConvert.SerializeObject(new
                {
                    error = ex.Message,
                    statusCode = ex.HttpCode
                });
                var response = CreateWebSocketResponse(responseJson);
                await context.Reply(response);
                return;
            }
            catch (Exception ex)
            {
                var responseJson = JsonConvert.SerializeObject(new
                {
                    error = ex.Message,
                    statusCode = 500
                });
                var response = CreateWebSocketResponse(responseJson);
                await context.Reply(response);
                return;
            }


            if (cqsReplyObject.Body is Exception)
            {
                var responseJson = JsonConvert.SerializeObject(new
                {
                    error = ((Exception) cqsReplyObject.Body).Message,
                    statusCode = 500
                });
                var response = CreateWebSocketResponse(responseJson);
                await context.Reply(response);
            }
            else
            {
                json = JsonConvert.SerializeObject(cqsReplyObject.Body);
                var reply = CreateWebSocketResponse(json);
                await context.Reply(reply);
            }
        }


        private static WebSocketResponse CreateWebSocketResponse(string json)
        {
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)) {Position = 0};
            var frame = new WebSocketFrame(WebSocketFin.Final, WebSocketOpcode.Text,
                WebSocketMask.Unmask, ms);
            return new WebSocketResponse(frame);
        }
    }
}