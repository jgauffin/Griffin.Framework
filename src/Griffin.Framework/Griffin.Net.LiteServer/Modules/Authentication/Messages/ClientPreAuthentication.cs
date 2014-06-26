using System;
using System.Runtime.Serialization;

namespace Griffin.Net.LiteServer.Modules.Authentication.Messages
{
    /// <summary>
    /// Default implementation supporting both DataContract and the old .NET serializers.
    /// </summary>
    [DataContract, Serializable]
    public class ClientPreAuthentication : IClientPreAuthentication
    {
        /// <summary>
        /// Name of the user that would like to authenticate
        /// </summary>
        [DataMember(Order = 1)]
        public string UserName { get; set; }
    }
}