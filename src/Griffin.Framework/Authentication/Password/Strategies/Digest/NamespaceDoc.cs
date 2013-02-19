using System.Runtime.CompilerServices;

namespace Griffin.Framework.Authentication.Password.Strategies.Digest
{
    /// <summary>
    /// Digest authentication as described in RFC 2167
    /// </summary>
    /// <remarks>
    /// <para>Digest authentication hashes the password before sending it over the wire. That means that you either
    /// have to be able to decrypt the user passwords (so that the matching can be done) or store the HA1 hash in the data source.
    ///  </para>
    /// <para>
    /// A HA1 hash is simply <c>string.Join("{0}:{1}:{2}", userName, realm, password);</c> which then has been MD5:ed and converted to HEX. You can call 
    /// </para>
    /// </remarks>
    /// 
    [CompilerGenerated]
    class NamespaceDoc
    {
    }
}
