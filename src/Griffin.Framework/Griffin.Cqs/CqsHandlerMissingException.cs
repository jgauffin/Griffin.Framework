using System;
using System.Runtime.Serialization;
using DotNetCqs;

namespace Griffin.Cqs
{
    /// <summary>
    /// Did not find a handler for a specific CQS object (i.e. a subclass of <see cref="Command"/>, <see cref="Query{TResult}"/>, <see cref="ApplicationEvent"/> or <see cref="Request{TReply}"/>).
    /// </summary>
    
    public class CqsHandlerMissingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CqsHandlerMissingException"/> class.
        /// </summary>
        /// <param name="type">message that a handler was not found for.</param>
        public CqsHandlerMissingException(Type type)
            : base(string.Format("Missing a handler for '{0}'.", type.FullName))
        {
            CqsType = type.FullName;
        }
        
        /// <summary>
        /// Full name of the type that we are missing a handler for.
        /// </summary>
        public string CqsType { get; set; }
        
    }
}