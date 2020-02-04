using Griffin.Security.Authentication;

namespace HttpServerTests
{
    internal class FakeUser : UserAccount
    {
        public int Attempts { get; set; }
    }
}