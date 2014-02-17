using System;
using Griffin.Net.Protocols.Stomp.Broker.Services;
using Griffin.Net.Protocols.Stomp.Frames;

namespace Griffin.Net.Protocols.Stomp.Broker.MessageHandlers
{
    public class ConnectHandler : IFrameHandler
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly string _serverName;

        public ConnectHandler(IAuthenticationService authenticationService, string serverName)
        {
            _authenticationService = authenticationService;
            _serverName = serverName;
        }

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
