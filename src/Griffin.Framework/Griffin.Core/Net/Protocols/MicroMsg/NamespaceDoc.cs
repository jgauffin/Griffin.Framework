using System.Runtime.CompilerServices;

namespace Griffin.Net.Protocols.MicroMsg
{
    /// <summary>
    ///     MicroMessage is a small message format with a binary header.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The header was designed to be backwards compatible and support newer versions. The contents of the body is unspecified and up to the peers to decide.
    /// </para>
    ///     The header contains the following fields:
    ///     <list type="table">
    ///         <item>
    ///             <term>Headerlength</term>
    ///             <description><c>ushort</c>, number of bytes that are for the header. First byte is directly after this field. <para>This field was added
    /// to be able to include new features without affecting previous versions</para></description>
    ///         </item>
    ///         <item>
    ///             <term>Version</term>
    ///             <description><c>byte</c>, Defines the version of the micro protocol. Current version is 1.</description>
    ///         </item>
    ///         <item>
    ///             <term>ContentLength</term>
    ///             <description><c>int</c>, Defines the length of the body. The body starts directly after the header.</description>
    ///         </item>
    ///         <item>
    ///             <term>Typelength</term>
    ///             <description><c>ubyte</c>, Defines the length of the next header value.</description>
    ///         </item>
    ///         <item>
    ///             <term>TypeName</term>
    ///             <description>
    ///                 Fully qualified assembly name of the type that is our payload.
    ///                 <para>
    ///                     The default encoding of the name is UTF8. You may use another encoding (to support foreign characters in the type names), but then it's important that you communicate out the encoding.
    ///                 </para>
    ///             </description>
    ///         </item>
    ///     </list>
    ///     <para>
    ///         To work with computers that use different byte ordering, all integer values that are sent over the network are sent in network byte order which has the most significant byte first.
    ///     </para>
    /// </remarks>
    [CompilerGenerated]
    internal class NamespaceDoc
    {
    }
}