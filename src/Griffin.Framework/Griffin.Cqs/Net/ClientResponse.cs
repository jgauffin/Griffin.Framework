using System;

namespace Griffin.Cqs.Net
{
    /// <summary>
    /// Response from CQS server.
    /// </summary>
    public class ClientResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientResponse"/> class.
        /// </summary>
        /// <param name="identifier">Identifier for the incoming message.</param>
        /// <param name="body">The body, may be null.</param>
        public ClientResponse(Guid identifier, object body)
        {
            Identifier = identifier;
            Body = body;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientResponse"/> class.
        /// </summary>
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