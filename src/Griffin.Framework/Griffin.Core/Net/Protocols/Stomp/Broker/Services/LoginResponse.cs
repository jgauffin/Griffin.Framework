namespace Griffin.Net.Protocols.Stomp.Broker.Services
{
    public class LoginResponse
    {
        public bool IsSuccessful { get; set; }
        public string Token { get; set; }
        public string Reason { get; set; }
    }
}