using System;

namespace Griffin.Cqs.Net
{
    /// <summary>
    /// Response from CQS server.
    /// </summary>
    public class ClientResponse
    {
        public ClientResponse(Guid identifier, object body)
        {
            Identifier = identifier;
            Body = body;
        }

        protected ClientResponse()
        {
            
        }

        /// <summary>
        ///     Id from inbound command/query. Allows us to match the response to a specific message.
        /// </summary>
        public Guid Identifier { get; set; }

        /// <summary>
        ///     Response if any
        /// </summary>
        /// <remarks>
        ///     <para>Is an exception if something failed at server side.</para>
        /// </remarks>
        public object Body { get; set; }
    }
}