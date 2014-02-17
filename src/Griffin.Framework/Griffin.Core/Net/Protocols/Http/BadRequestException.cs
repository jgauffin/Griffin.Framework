using System;

namespace Griffin.Net.Protocols.Http
{
    class BadRequestException : Exception
    {
        public BadRequestException(string errorMessage)
            :base(errorMessage)
        {
            
        }
    }
}
