namespace Griffin.Net.Authentication.Messages
{
    /// <summary>
    ///     Step three, Used by the client to prove that it knows the password (i.e. is who it states that it is)
    /// </summary>
    public interface IAuthenticate
    {
        /// <summary>
        ///     On the client, salt the password with the account salt and hash it (keep this as " client shared secret"),
        ///     then salt the result with the session salt and hash it again. Transmit this as an authentication token (not secret)
        /// </summary>
        string AuthenticationToken { get; }

        /// <summary>
        ///     Salt that the client want the server to use to prove it's identity
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The server must hash the password hash with this hash to prove that it's the real server.
        ///     </para>
        /// </remarks>
        string ClientSalt { get; }
    }
}