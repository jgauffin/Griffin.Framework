using System;

namespace Griffin.Cqs.Demo.Contracts
{
    public class Account
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public DateTime RegisteredAtUtc { get; set; }
    }
}