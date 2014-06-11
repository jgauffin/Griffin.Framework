using System;
using System.Threading.Tasks;
using DotNetCqs;

namespace Griffin.Cqs.Demo.Query
{
    public class GetDiscountHandler : IQueryHandler<GetDiscounts, DiscountListItem[]>
    {
        /// <summary>
        /// Method used to execute the query
        /// </summary>
        /// <param name="query">Query to execute.</param>
        /// <returns>
        /// Task which will contain the result once completed.
        /// </returns>
        public async Task<DiscountListItem[]> ExecuteAsync(GetDiscounts query)
        {
            Console.WriteLine("Fetching discounts..");
            return new[] {new DiscountListItem() {Id = Guid.NewGuid(), Name = "Special"}};
        }
    }
}