using System;
using System.Threading.Tasks;
using DotNetCqs;
using Griffin.Container;

namespace Griffin.Cqs.Demo.Request
{
    [ContainerService]
    public class LoginHandler : IRequestHandler<Login, LoginReply>
    {
        /// <summary>
        /// Execute the request and generate a reply.
        /// </summary>
        /// <param name="request">Request to execute</param>
        /// <returns>
        /// Task which will contain the reply once completed.
        /// </returns>
        public async Task<LoginReply> ExecuteAsync(Login request)
        {
            Console.WriteLine("Logging in "+ request.UserName);
            return new LoginReply() {Success = true, Account = new Account() {UserName = request.UserName}};
        }
    }
}