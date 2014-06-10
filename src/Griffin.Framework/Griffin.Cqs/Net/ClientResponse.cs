using System;

namespace Griffin.Cqs.Net
{
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

        public Guid Identifier { get; set; }
        public object Body { get; set; }
    }
}