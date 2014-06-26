using System;
using System.Threading.Tasks;

namespace Griffin.Cqs.Net.Authentication
{
    /// <summary>
    ///     Used to fetch accounts from your data source.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The implementation will be treated as a single instance. That means that you can not keep a database connection
    ///         open in your implementation unless you have some sort of reconnect logic if a connection goes away.
    ///     </para>
    /// </remarks>
    public interface IUserFetcher
    {
        /// <summary>
        ///     Get a user from your data source
        /// </summary>
        /// <param name="userName">
        ///     Some sort of user identity (which the user supplied at client-side). Do not necessarily have to be a user name, but
        ///     could be a email or similar
        ///     depending on how you let users log in.
        /// </param>
        /// <returns>User if found; otherwise <c>null</c>.</returns>
        /// <exception cref="ArgumentNullException">userName was not supplied</exception>
        Task<IUserAccount> FindUserAsync(string userName);


        /// <summary>
        ///     Get all roles that the specified user is a member of
        /// </summary>
        /// <param name="account">Account previously fetched using <see cref="FindUserAsync" />.</param>
        /// <returns>An array of roles (or an empty array if roles are not used)</returns>
        /// <exception cref="ArgumentException">user is not found</exception>
        /// <exception cref="ArgumentNullException">User was not supplied</exception>
        Task<string[]> GetRolesAsync(IUserAccount account);
    }
}