using System;
using System.Threading.Tasks;

namespace Griffin.Net.LiteServer.Modules.Authentication
{
    public abstract class MicroMessageMiddleware : IMiddleware<MicroMessageContext>
    {
        public abstract Task Process(MicroMessageContext context, Func<Task> next);
    }
}