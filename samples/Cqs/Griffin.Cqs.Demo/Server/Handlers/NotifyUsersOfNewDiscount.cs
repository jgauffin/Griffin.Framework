using System;
using System.Threading.Tasks;
using DotNetCqs;
using Griffin.Container;
using Griffin.Cqs.Demo.Contracts.Cqs;

namespace Griffin.Cqs.Demo.Server.Handlers
{
    [ContainerService]
    public class NotifyUsersOfNewDiscount : IApplicationEventSubscriber<DiscountUpdated>
    {
        /// <summary>
        /// ProcessAsync an event asynchronously.
        /// </summary>
        /// <param name="e">event to process</param>
        /// <returns>
        /// Task to wait on.
        /// </returns>
        public async Task HandleAsync(DiscountUpdated e)
        {
            Console.WriteLine("Server: Send email to users for discount " + e.DiscountId);
        }
    }
}