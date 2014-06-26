using System;
using DotNetCqs;

namespace Griffin.Cqs.Net
{
    /// <summary>
    /// Extension methods
    /// </summary>
    public static class CqsExtensions
    {
        /// <summary>
        /// Gets the identifier from the CQS objects
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>message identifer</returns>
        /// <exception cref="System.NotSupportedException">Is not one of the messages defined in DotNetCqs</exception>
        public static Guid GetId(this object value)
        {
            if (value is IRequest)
                return ((IRequest) value).RequestId;
            if (value is IQuery)
                return ((IQuery)value).QueryId;
            if (value is Command)
                return ((Command)value).CommandId;
            if (value is ApplicationEvent)
                return ((ApplicationEvent)value).EventId;

            throw new NotSupportedException(value.GetType().ToString());
        }

    }
}
