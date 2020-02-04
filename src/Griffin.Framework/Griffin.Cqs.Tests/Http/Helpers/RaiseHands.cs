using DotNetCqs;

namespace Griffin.Cqs.Tests.Http.Helpers
{
    public class RaiseHands : Command
    {
        public string Reason { get; set; }
    }
}