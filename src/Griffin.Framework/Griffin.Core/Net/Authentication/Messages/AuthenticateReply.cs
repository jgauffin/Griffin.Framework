using System;
using System.Runtime.Serialization;

namespace Griffin.Net.Authentication.Messages
{
    /// <summary>
    /// Default implementation which is Serializable and tagged with data contract attributes.
    /// </summary>
    [DataContract]
    public class AuthenticateReply : IAuthenticateReply
    {
        /// <summary>
        /// Returns wether the user may login or not
        /// </summary>
        [DataMember(Order = 1)]
        public AuthenticateReplyState State { get; set; }

        /// <summary>
        ///     Token created by the server to prove it's identity
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         It's generated with the help of the client salt and the password hash that is stored for the user.
        ///     </para>
        /// </remarks>
        public string AuthenticationToken { get; set; }
    }
}