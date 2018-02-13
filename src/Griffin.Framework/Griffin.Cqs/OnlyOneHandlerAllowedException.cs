using System;
using System.Runtime.Serialization;

namespace Griffin.Cqs
{
    /// <summary>
    /// Some of the CQS messages allows only one handler to avoid ambiguity.
    /// </summary>
    
    public class OnlyOneHandlerAllowedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OnlyOneHandlerAllowedException"/> class.
        /// </summary>
        /// <param name="cqsType">Type of the CQS.</param>
        public OnlyOneHandlerAllowedException(Type cqsType)
            : base("Only one handler is allowed for '" + cqsType.FullName + "'.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OnlyOneHandlerAllowedException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected OnlyOneHandlerAllowedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}