namespace Griffin.Net.Protocols.Http.Authentication
{
    /// <remarks>The </remarks>
    public interface IAuthenticationUser
    {
        /// <summary>
        /// Gets or sets user name used during authentication.
        /// </summary>
        string Username { get; set; }

        /// <summary>
        /// Gets or sets unencrypted password.
        /// </summary>
        /// <remarks>
        /// Password as clear text. You could use <see cref="HA1"/> instead if your passwords
        /// are encrypted in the database.
        /// </remarks>
        string Password { get; set; }

        /// <summary>
        /// Gets or sets HA1 hash.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Digest authentication requires clear text passwords to work. If you
        /// do not have that, you can store a HA1 hash in your database (which is part of
        /// the Digest authentication process).
        /// </para>
        /// <para>
        /// A HA1 hash is simply a Md5 encoded string: "UserName:Realm:Password". The quotes should
        /// not be included. Realm is the currently requested Host (as in <c>Request.Headers["host"]</c>).
        /// </para>
        /// <para>
        /// Leave the string as <c>null</c> if you are not using HA1 hashes.
        /// </para>
        /// </remarks>
        string HA1 { get; set; }
    }
}