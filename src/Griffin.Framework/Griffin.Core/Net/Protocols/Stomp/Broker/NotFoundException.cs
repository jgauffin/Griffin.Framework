using System;

namespace Griffin.Net.Protocols.Stomp.Broker
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string errorMessage)
            : base(errorMessage)
        {
            
        }
    }
}