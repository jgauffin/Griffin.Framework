using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Griffin.Cqs.Net.Authentication
{
    /// <summary>
    /// Step one during the authentication steps.
    /// </summary>
    /// <remarks>
    /// Should be sent to the server directly after a successful connect.
    /// </remarks>
    public interface IClientPreAuthentication
    {
        /// <summary>
        /// Name of the user that would like to authenticate
        /// </summary>
        string UserName { get; }
    }

    /// <summary>
    /// Step two, server responds with salts.
    /// </summary>
    /// <remarks>
    /// Should be the reply from the server on the <see cref="IClientInitAuthentication"/> messag.e
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

    /// <summary>
    /// Step three, Used by the client to prove that it knows the password (i.e. is who it states that it is)
    /// </summary>
    public interface IClientAuthentication
    {
        /// <summary>
        /// On the client, salt the password with the account salt and hash it (keep this as " client shared secret"), 
        /// then salt the result with the session salt and hash it again. Transmit this as an authentication token (not secret)
        /// </summary>
        string AuthenticationToken { get; }
    }

}
