using System;

namespace Griffin.Net.Protocols.Stomp.Broker
{
    public class BadRequestException : Exception
    {
        public IFrame Request { get; set; }

        public BadRequestException(IFrame request, string errorMessage)
            : base(errorMessage)
        {
            Request = request;
        }
    }
}
