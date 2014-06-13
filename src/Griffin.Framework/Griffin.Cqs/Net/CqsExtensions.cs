using System;
using DotNetCqs;

namespace Griffin.Cqs.Net
{
    public static class CqsExtensions
    {
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
