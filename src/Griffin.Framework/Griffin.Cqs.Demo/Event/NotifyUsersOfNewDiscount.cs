using System;
using System.Threading.Tasks;
using DotNetCqs;

namespace Griffin.Cqs.Demo.Event
{
    public class NotifyUsersOfNewDiscount : IApplicationEventSubscriber<DiscountUpdated>
    {
        /// <summary>
        /// Process an event asynchronously.
        /// </summary>
        /// <param name="e">event to process</param>
        /// <returns>
        /// Task to wait on.
        /// </returns>
        public async Task HandleAsync(DiscountUpdated e)
        {
            Console.WriteLine("Send email to users for discount " + e.DiscountId);
        }
    }
}