using System;
using System.Runtime.Serialization;

namespace Griffin.Cqs.Net.Authentication.Messages
{
    /// <summary>
    /// Step three, Used by the client to prove that it knows the password (i.e. is who it states that it is)
    /// </summary>
    /// <remarks>
    /// <para>Uses the data contract attributes</para>
    /// </remarks>
    [DataContract, Serializable]
    public class ClientAuthentication : IClientAuthentication
    {
        /// <summary>
        /// On the client, salt the password with the account salt and hash it (keep this as " client shared secret"),
        /// then salt the result with the session salt and hash it again. Transmit this as an authentication token (not secret)
        /// </summary>
        [DataMember(Order = 1)]
        public string AuthenticationToken { get; set; }
    }
}