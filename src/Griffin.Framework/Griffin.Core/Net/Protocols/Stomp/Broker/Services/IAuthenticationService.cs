namespace Griffin.Net.Protocols.Stomp.Broker.Services
{
    public interface IAuthenticationService
    {
        bool IsActivated { get; }
        LoginResponse Login(string user, string passcode);
    }
}