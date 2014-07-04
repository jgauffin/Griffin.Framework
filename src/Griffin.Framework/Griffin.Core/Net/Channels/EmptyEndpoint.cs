using System.Net;
using System.Net.Sockets;

namespace Griffin.Net.Channels
{
    /// <summary>
    /// Used when there are no connected endpoint for a channel
    /// </summary>
    public class EmptyEndpoint : EndPoint
    {
        /// <summary>
        /// Instance representing an unassigned end point.
        /// </summary>
        public static readonly EmptyEndpoint Instance = new EmptyEndpoint();

        private EmptyEndpoint()
        {
        }

        /// <summary>
        ///     Gets the address family to which the endpoint belongs.
        /// </summary>
        /// <returns>
        ///     <c>AddressFamily.Unspecified</c>
        /// </returns>
        /// <exception cref="T:System.NotImplementedException">
        ///     Any attempt is made to get or set the property when the property is
        ///     not overridden in a descendant class.
        /// </exception>
        /// <PermissionSet>
        ///     <IPermission
        ///         class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
        ///         version="1" Flags="UnmanagedCode, ControlEvidence" />
        /// </PermissionSet>
        public override AddressFamily AddressFamily
        {
            get { return AddressFamily.Unspecified; }
        }

        /// <summary>
        ///     Returns "None"
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return "None";
        }
    }
}