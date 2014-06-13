using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Griffin.Core.Json;
using Griffin.Cqs.Demo.Contracts.Cqs;
using Griffin.Cqs.Net;

namespace Griffin.Cqs.Demo.Client
{
    class ClientDemo
    {
        private CqsClient _client;

        public ClientDemo()
        {
            _client = new CqsClient(() => new JsonMessageSerializer());
        }

        public async Task RunAsync(int port)
        {
            await _client.StartAsync(IPAddress.Loopback, port);

            Console.WriteLine("Client: Executing request/reply");
            var response = await _client.ExecuteAsync<LoginReply>(new Login("jonas", "mamma"));
            if (response.Success)
                Console.WriteLine("Client: Logged in successfully");
            else
            {
                Console.WriteLine("Client: Failed to login");
                return;
            }

            Console.WriteLine("Client: Executing command");
            await _client.ExecuteAsync(new IncreaseDiscount(20));


            Console.WriteLine("Client: Executing query");
            var discounts = await _client.QueryAsync(new GetDiscounts());
            Console.WriteLine("Client: First discount: " + discounts[0].Name);
        }
    }

}
