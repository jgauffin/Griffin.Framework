using System;

namespace Griffin.Net.Protocols.Http.Messages
{
    /// <summary>
    /// Failed to parse message
    /// </summary>
    public class ParseException : Exception
    {
        /// <summary>
        /// Creates a new instance of <see cref="ParseException"/>.
        /// </summary>
        /// <param name="errorMessage">error message</param>
        public ParseException(string errorMessage):base(errorMessage)
        {
            
        }
    }
}