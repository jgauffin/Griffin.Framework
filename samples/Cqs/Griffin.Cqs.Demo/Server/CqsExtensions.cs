using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetCqs;

namespace Griffin.Cqs.Demo.Server
{
    public static class CqsExtensions
    {
        public static Guid GetCqsId(this object value)
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
