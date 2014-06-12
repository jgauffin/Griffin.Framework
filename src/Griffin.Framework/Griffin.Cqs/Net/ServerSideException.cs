using System;
using System.Runtime.Serialization;

namespace Griffin.Cqs.Net
{
    [Serializable]
    public class ServerSideException : Exception
    {
        public ServerSideException(string message) : base(message)
        {
        }

        public ServerSideException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ServerSideException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}