namespace Griffin.Cqs.Net.Authentication.Messages
{
    /// <summary>
    /// Step three, Used by the client to prove that it knows the password (i.e. is who it states that it is)
    /// </summary>
    public interface IClientAuthentication
    {
        /// <summary>
        /// On the client, salt the password with the account salt and hash it (keep this as " client shared secret"), 
        /// then salt the result with the session salt and hash it again. Transmit this as an authentication token (not secret)
        /// </summary>
        string AuthenticationToken { get; }
    }
}