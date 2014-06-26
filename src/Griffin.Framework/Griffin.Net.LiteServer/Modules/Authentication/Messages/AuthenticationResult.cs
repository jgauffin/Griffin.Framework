using System;
using System.Runtime.Serialization;

namespace Griffin.Net.LiteServer.Modules.Authentication.Messages
{
    /// <summary>
    /// Default implementation which is Serializable and tagged with data contract attributes.
    /// </summary>
    [DataContract, Serializable]
    public class AuthenticationResult : IAuthenticationResult
    {
        /// <summary>
        /// Returns wether the user may login or not
        /// </summary>
        [DataMember(Order = 1)]
        public AuthenticationResultState State { get; set; }
    }
}