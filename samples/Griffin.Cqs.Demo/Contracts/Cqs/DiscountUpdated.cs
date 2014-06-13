using System;
using DotNetCqs;

namespace Griffin.Cqs.Demo.Contracts.Cqs
{
    public class DiscountUpdated : ApplicationEvent
    {
        public DiscountUpdated(Guid discountId)
        {
            DiscountId = discountId;
        }

        public Guid DiscountId { get; private set; }
    }
}