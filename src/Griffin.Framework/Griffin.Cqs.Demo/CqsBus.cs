using System;
using System.Reflection;
using DotNetCqs;

namespace Griffin.Cqs.Demo
{

    /// <summary>
    /// To be used when we are not using a container.
    /// </summary>
    public class CqsBus
    {
        public static ICommandBus CmdBus { get; set; }
        public static IQueryBus QueryBus { get; set; }
        public static IRequestReplyBus RequestReplyBus { get; set; }
        public static IEventBus EventBus { get; set; }
    }
}