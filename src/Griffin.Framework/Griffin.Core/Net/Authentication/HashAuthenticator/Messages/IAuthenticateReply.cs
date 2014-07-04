namespace Griffin.Net.Authentication.Messages
{
    /// <summary>
    ///     Finaly step of the authentication process.
    /// </summary>
    public interface IAuthenticateReply
    {
        /// <summary>
        ///     Returns wether the user may login or not
        /// </summary>
        AuthenticateReplyState State { get; }

        /// <summary>
        ///     Token created by the server to prove it's identity
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         It's generated with the help of the client salt and the password hash that is stored for the user.
        ///     </para>
        /// </remarks>
        string AuthenticationToken { get; }
    }
}