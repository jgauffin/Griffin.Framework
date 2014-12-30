namespace Griffin.Net.Protocols.Stomp.Broker.Services
{
    /// <summary>
    /// Response for a login attempt.
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// successful login
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// session token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Why it failed.
        /// </summary>
        public string Reason { get; set; }
    }
}