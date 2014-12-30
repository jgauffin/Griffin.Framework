namespace Griffin.Net.Protocols.Stomp.Broker.Services
{
    /// <summary>
    /// Contract for authentication service
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Activated
        /// </summary>
        bool IsActivated { get; }

        /// <summary>
        /// Login 
        /// </summary>
        /// <param name="user">User name (ascii)</param>
        /// <param name="passcode">Password (encrypted if the provider supports it)</param>
        /// <returns></returns>
        LoginResponse Login(string user, string passcode);
    }
}