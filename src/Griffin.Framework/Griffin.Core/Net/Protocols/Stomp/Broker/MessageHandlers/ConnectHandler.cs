using System;
using Griffin.Net.Protocols.Stomp.Broker.Services;
using Griffin.Net.Protocols.Stomp.Frames;

namespace Griffin.Net.Protocols.Stomp.Broker.MessageHandlers
{
    /// <summary>
    /// CONNECT frame. 
    /// </summary>
    /// <remarks>
    /// <para>
    /// Used instead of the initial STOMP frame for earlier versions.
    /// </para>
    /// </remarks>
    public class ConnectHandler : IFrameHandler
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly string _serverName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectHandler"/> class.
        /// </summary>
        /// <param name="authenticationService">The authentication service.</param>
        /// <param name="serverName">Name of the server.</param>
        /// <exception cref="System.ArgumentNullException">
        /// authenticationService
        /// or
        /// serverName
        /// </exception>
        public ConnectHandler(IAuthenticationService authenticationService, string serverName)
        {
            if (authenticationService == null) throw new ArgumentNullException("authenticationService");
            if (serverName == null) throw new ArgumentNullException("serverName");
            _authenticationService = authenticationService;
            _serverName = serverName;
        }

        /// <summary>
        /// Process an inbound frame.
        /// </summary>
        /// <param name="client">Connection that received the frame</param>
        /// <param name="request">Inbound frame to process</param>
        /// <returns>
        /// Frame to send back; <c>null</c> if no message should be returned;
        /// </returns>
        public IFrame Process(IStompClient client, IFrame request)
        {
            var versions = request.Headers["accept-version"];
            if (versions == null)
            {
                var error = request.CreateError("Missing the 'accept-version' header.");
                error.Headers["version"] = "2.0";
                return error;
            }

            if (!versions.Contains("2.0"))
            {
                var error = request.CreateError("Only accepting stomp 2.0 clients.");
                error.Headers["version"] = "2.0";
                return error;
            }

            IFrame frame;
            if (!CheckCredentials(client, request, out frame)) 
                return frame;

            //TODO: Heartbeating.


            var response = new BasicFrame("CONNECTED");
            response.Headers["version"] = "2.0";
            response.Headers["server"] = _serverName;

            if (client.SessionKey != null)
                response.Headers["session"] = client.SessionKey;

            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <param name="errorFrame"></param>
        /// <returns><c>true</c> means that we should exist.</returns>
        private bool CheckCredentials(IStompClient client, IFrame request, out IFrame errorFrame)
        {
            if (_authenticationService.IsActivated)
            {
                var user = request.Headers["login"];
                var passcode = request.Headers["passcode"];
                if (user == null || passcode == null)
                {
                    var error =
                        request.CreateError(
                            "This broker have been configured to only allow authenticated clients. Send the 'login'/'password' headers in the 'STOMP' errorFrame.");
                    error.Headers["version"] = "2.0";
                    {
                        errorFrame = error;
                        return false;
                    }
                }

                var loginResult = _authenticationService.Login(user, passcode);
                if (!loginResult.IsSuccessful)
                {
                    var error = request.CreateError(loginResult.Reason);
                    error.Headers["version"] = "2.0";
                    {
                        errorFrame = error;
                        return false;
                    }
                }

                client.SetAsAuthenticated(loginResult.Token);
            }
            else
                client.SetAsAuthenticated(Guid.NewGuid().ToString());

            errorFrame = null;
            return true;
        }
    }
}
