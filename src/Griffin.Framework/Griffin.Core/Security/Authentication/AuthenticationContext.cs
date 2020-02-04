using System;

namespace Griffin.Net.Protocols.Http.Authentication
{
    public class AuthenticationContext
    {
        public string AuthenticationType { get; set; }
        public string UserName { get; set; }
        public string Secret { get; set; }
        public Func<HashingContext, bool> CompareHashFunc;
    }
}