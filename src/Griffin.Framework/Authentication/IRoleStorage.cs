using System;

namespace Griffin.Framework.Authentication
{
    /// <summary>
    /// Persistance of account roles.
    /// </summary>
    public interface IRoleStorage
    {
        /// <summary>
        /// Add a member to a role
        /// </summary>
        /// <param name="roleName">role</param>
        /// <param name="accountName">account</param>
        void AddMember(string roleName, string accountName);

        /// <summary>
        /// Create a new role
        /// </summary>
        /// <param name="roleName">Role to create</param>
        void Create(string roleName);

        /// <summary>
        /// Delete a role
        /// </summary>
        /// <param name="roleName"></param>
        /// <exception cref="InvalidOperationException">There are members of that role.</exception>
        void Delete(string roleName);

        /// <summary>
        /// Get all members of a specific role
        /// </summary>
        /// <param name="roleName">Name of the role</param>
        /// <returns>A list of all users (or an empty array)</returns>
        string[] GetMembersOfRole(string roleName);


        /// <summary>
        /// Get all roles that a user is a member of
        /// </summary>
        /// <param name="accountName">username</param>
        /// <returns></returns>
        string[] GetRoles(string accountName);

        /// <summary>
        /// Remove a member from a role
        /// </summary>
        /// <param name="roleName">role</param>
        /// <param name="accountName">account</param>
        void RemoveMember(string roleName, string accountName);
    }
}