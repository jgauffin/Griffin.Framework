namespace Griffin.Net.LiteServer.Modules.Authentication.Messages
{
    /// <summary>
    /// Step two, server responds with salts.
    /// </summary>
    /// <remarks>
    /// Should be the reply from the server on the <see cref="IClientPreAuthentication"/> message.
    /// </remarks>
    public interface IServerPreAuthentication
    {
        /// <summary>
        /// Salt that you have used to salt the password before storing it in the database.
        /// </summary>
        string AccountSalt { get; }

        /// <summary>
        /// Used to prevent man in the middle attacks. You can use a random string, a guid or whatever you prefer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The string must however be unique for this session (client connection).
        /// </para>
        /// </remarks>
        string SessionSalt { get; }
    }
}