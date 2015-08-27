using System.Net;

namespace Griffin.Core.Tests.External
{
    class PrivateClass
    {
        public string A { get; private set; }
        public int B { get; private set; }

        public PrivateClass(string a, int b)
        {
            A = a;
            B = b;
        }

        protected PrivateClass()
        {
            
        }
    }

    class PocoWithEnum
    {
        public HttpStatusCode StatusCode { get; set; }
    }
}