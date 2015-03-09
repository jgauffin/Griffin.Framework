using System;
using System.Runtime.Serialization;
using Griffin.Net.Authentication.Messages;

namespace Griffin.Net.Authentication.HashAuthenticator.Messages
{
    /// <summary>
    ///     Default implementation supporting data contracts
    /// </summary>
    [DataContract, Serializable]
    public class AuthenticationHandshakeReply : IAuthenticationHandshakeReply
    {
        /// <summary>
        ///     Salt that you have used to salt the password before storing it in the database.
        /// </summary>
        [DataMember(Order = 1)]
        public string AccountSalt { get; set; }

        /// <summary>
        ///     Used to prevent man in the middle attacks. You can use a random string, a guid or whatever you prefer.
        /// </summary>
        /// <remarks>
        ///     The string must however be unique for this session (client connection).
        /// </remarks>
        [DataMember(Order = 2)]
        public string SessionSalt { get; set; }
    }
}