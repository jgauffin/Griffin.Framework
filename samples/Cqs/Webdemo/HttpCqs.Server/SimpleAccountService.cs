using System;
using System.Collections.Generic;
using System.Linq;
using Griffin.Net.Protocols.Http.Authentication;

namespace HttpCqs.Server
{
    public class SimpleAccountService : IAccountService
    {
        private readonly List<BasicUser> _users = new List<BasicUser>();

        /// <summary>
        ///     Lookups the specified user
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <param name="host">Typically web server domain name.</param>
        /// <returns>User if found; otherwise <c>null</c>.</returns>
        /// <remarks>
        ///     User name can basically be anything. For instance name entered by user when using
        ///     basic or digest authentication, or SID when using Windows authentication.
        /// </remarks>
        public IAuthenticationUser Lookup(string userName, Uri host)
        {
            return _users.FirstOrDefault(x => x.Username == userName);
        }

        /// <summary>
        ///     Hash password to be able to do comparison
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="host"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string HashPassword(string userName, Uri host, string password)
        {
            return password;
        }

        /// <summary>
        ///     Allows you to manage passwords by your self. Only works if the client supplies clear text passwords.
        /// </summary>
        /// <param name="user">User that your service returned from <c>Lookup</c>.</param>
        /// <param name="password">Password supplied by the client (web browser / http client)</param>
        /// <returns><c>true</c> if the user was authenticated</returns>
        public bool ComparePassword(IAuthenticationUser user, string password)
        {
            return user.Password.Equals(password);
        }

        public void Add(string userName, string password)
        {
            _users.Add(new BasicUser
            {
                Username = userName,
                Password = password
            });
        }

        public void Add(string userName, string password, string[] roles)
        {
            _users.Add(new BasicUser
            {
                Username = userName,
                Password = password,
                RoleNames = roles
            });
        }

        public class BasicUser : IUserWithRoles
        {
            public BasicUser()
            {
                RoleNames = new string[0];
            }
            /// <summary>
            ///     Gets or sets user name used during authentication.
            /// </summary>
            public string Username { get; set; }

            /// <summary>
            ///     Gets or sets unencrypted password.
            /// </summary>
            /// <remarks>
            ///     Password as clear text. You could use <see cref="HA1" /> instead if your passwords
            ///     are encrypted in the database.
            /// </remarks>
            public string Password { get; set; }

            /// <summary>
            ///     Gets or sets HA1 hash.
            /// </summary>
            /// <remarks>
            ///     <para>
            ///         Digest authentication requires clear text passwords to work. If you
            ///         do not have that, you can store a HA1 hash in your database (which is part of
            ///         the Digest authentication process).
            ///     </para>
            ///     <para>
            ///         A HA1 hash is simply a Md5 encoded string: "UserName:Realm:Password". The quotes should
            ///         not be included. Realm is the currently requested Host (as in <c>Request.Headers["host"]</c>).
            ///     </para>
            ///     <para>
            ///         Leave the string as <c>null</c> if you are not using HA1 hashes.
            ///     </para>
            /// </remarks>
            public string HA1 { get; set; }

            public string[] RoleNames { get; set; }
        }
    }
}