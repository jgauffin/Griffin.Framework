using System;

namespace Griffin.Net.Protocols
{
    /// <summary>
    /// Something failed when we 
    /// </summary>
    public class CodecException : Exception
    {
        public CodecException(string errorMessage)
        :base(errorMessage)
        {
            
        }
    }
}
