using System;

namespace Griffin.Net.Protocols.Http.Results
{
    public class ServerErrorResult : HttpResult
    {
        public ServerErrorResult(Exception exception)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Disconnect the client once the error reply have been sent.
        /// </summary>
        public bool Shutdown { get; set; }
    }
}