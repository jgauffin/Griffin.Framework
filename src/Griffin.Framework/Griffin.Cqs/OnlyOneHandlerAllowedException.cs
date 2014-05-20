using System;
using System.Runtime.Serialization;

namespace Griffin.Cqs
{
    [Serializable]
    public class OnlyOneHandlerAllowedException : Exception
    {
        public OnlyOneHandlerAllowedException(Type cqsType)
            : base("Only one handler is allowed for '" + cqsType.FullName + "'.")
        {
        }

        protected OnlyOneHandlerAllowedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}