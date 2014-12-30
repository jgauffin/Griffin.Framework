using Griffin.Net.Channels;
using System;
using System.Net;

namespace Griffin.Net.Protocols.Http.WebSocket
{
    public class WebSocketListener : HttpListener
    {
        private MessageHandler _webSocketMessageReceived;

        /// <summary>
        ///     A websocket client have connected (websocket handshake request is complete)
        /// </summary>
        public event EventHandler<WebSocketClientConnectEventArgs> WebSocketClientConnect = delegate { };

        /// <summary>
        ///     A websocket client have connected (websocket handshake response is complete)
        /// </summary>
        public event EventHandler<WebSocketClientConnectedEventArgs> WebSocketClientConnected = delegate { };

        /// <summary>
        ///     A websocket client have disconnected
        /// </summary>
        public event EventHandler<ClientDisconnectedEventArgs> WebSocketClientDisconnected = delegate { };

        public WebSocketListener(ChannelTcpListenerConfiguration configuration)
            : base(configuration)
        {
        }

        public WebSocketListener()
        {
            var config = new ChannelTcpListenerConfiguration(
                () => new WebSocketDecoder(),
                () => new WebSocketEncoder());

            Configure(config);
        }

        /// <summary>
        /// WebSocket message received handler
        /// </summary>
        public MessageHandler WebSocketMessageReceived
        {
            get { return _webSocketMessageReceived; }
            set { _webSocketMessageReceived = value ?? delegate { }; }
        }

        protected override void OnMessage(Channels.ITcpChannel source, object msg)
        {
            var httpMessage = msg as IHttpMessage;
            if (WebSocketUtils.IsWebSocketUpgrade(httpMessage))
            {
                if (httpMessage is IHttpRequest) // server mode
                {
                    var args = new WebSocketClientConnectEventArgs(source, (IHttpRequest)httpMessage);
                    WebSocketClientConnect(this, args);

                    if (args.MayConnect)
                    {
                        var webSocketKey = httpMessage.Headers["Sec-WebSocket-Key"];

                        // TODO: why not provide the response in the WebSocketClientConnectEventArgs event?
                        var response = new WebSocketUpgradeResponse(webSocketKey);

                        source.Send(response);

                        WebSocketClientConnected(this, new WebSocketClientConnectedEventArgs(source, (IHttpRequest)httpMessage, response));
                    }
                    else
                    {
                        var response = new HttpResponseBase(HttpStatusCode.NotImplemented, "Not Implemented", "HTTP/1.1");
                        if (args.Response != null)
                            response.Body = args.Response;

                        source.Send(response);
                    }
                    return;
                }
                else if (httpMessage is IHttpResponse) // client mode
                {
                    WebSocketClientConnected(this, new WebSocketClientConnectedEventArgs(source, null, (IHttpResponse)httpMessage));
                }
            }
            var webSocketMessage = msg as IWebSocketMessage;
            if (webSocketMessage != null)
            {
                // standard message responses handled by listener
                switch (webSocketMessage.Opcode)
                {
                    case WebSocketOpcode.Ping:
                        source.Send(new WebSocketMessage(WebSocketOpcode.Pong, webSocketMessage.Payload));
                        return;
                    case WebSocketOpcode.Close:
                        source.Send(new WebSocketMessage(WebSocketOpcode.Close));
                        source.Close();

                        WebSocketClientDisconnected(this, new ClientDisconnectedEventArgs(source, new Exception("WebSocket closed")));
                        return;
                }

                _webSocketMessageReceived(source, webSocketMessage);
                return;
            }

            base.OnMessage(source, msg);
        }

    }
}
