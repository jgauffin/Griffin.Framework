using System;
using System.Runtime.Serialization;

namespace Griffin.Net.Authentication.Messages
{
    /// <summary>
    /// Step three, Used by the client to prove that it knows the password (i.e. is who it states that it is)
    /// </summary>
    /// <remarks>
    /// <para>Uses the data contract attributes</para>
    /// </remarks>
    [DataContract, Serializable]
    public class Authenticate : IAuthenticate
    {
        /// <summary>
        /// On the client, salt the password with the account salt and hash it (keep this as " client shared secret"),
        /// then salt the result with the session salt and hash it again. Transmit this as an authentication token (not secret)
        /// </summary>
        [DataMember(Order = 1)]
        public string AuthenticationToken { get; set; }

        /// <summary>
        ///     Salt that the client want the server to use to prove it's identity
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The server must hash the password hash with this hash to prove that it's the real server.
        ///     </para>
        /// </remarks>
        [DataMember(Order = 2)]
        public string ClientSalt { get; set; }
    }
}