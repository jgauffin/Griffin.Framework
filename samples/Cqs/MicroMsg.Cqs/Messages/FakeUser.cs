using Griffin.Security.Authentication;

namespace MicroMsg.Cqs.Messages
{
    internal class FakeUser : UserAccount
    {
        public int Attempts { get; set; }
    }
}