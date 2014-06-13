using System;
using System.Security.Authentication;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Griffin.Cqs.Demo.Contracts;
using Griffin.Cqs.Demo.Contracts.Cqs;
using Griffin.Cqs.Net;
using Griffin.Cqs.Net.Modules;
using Griffin.Net.Server.Modules;

namespace Griffin.Cqs.Demo.Server
{
    public class AuthenticationModule : IServerModule
    {
        public async Task BeginRequestAsync(IClientContext context)
        {
        }

        public async Task EndRequest(IClientContext context)
        {
        }

        public async Task<ModuleResult> ProcessAsync(IClientContext context)
        {
            //already authenticated
            if (context.ChannelData["Principal"] != null)
            {
                Console.WriteLine("Server: Loading identity");
                Thread.CurrentPrincipal = (IPrincipal) context.ChannelData["Principal"];
                return ModuleResult.Continue;
            }

            // Not authenticated, ignore messages
            var msg = context.RequestMessage as Login;
            if (msg == null)
            {
                Console.WriteLine("Server: Not logged in and not the Login request.");
                context.ResponseMessage = new AuthenticationException("You must run the Login request.");
                return ModuleResult.SendResponse;
            }

            // successful authentication
            if (msg.UserName == "jonas")
            {
                Console.WriteLine("Server: Authenticated successfully.");
                context.ChannelData["Principal"] = new GenericPrincipal(new GenericIdentity("jonas", "manual"),
                    new string[0]);
                context.ResponseMessage = new LoginReply() {Success = true, Account = new Account()};
                return ModuleResult.SendResponse;
            }

            Console.WriteLine("Server: Login failed.");
            context.ResponseMessage = new LoginReply() { Success = false };
            return ModuleResult.SendResponse;
        }
    }
}