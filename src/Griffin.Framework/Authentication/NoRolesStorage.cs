using System;

namespace Griffin.Framework.Authentication
{
    /// <summary>
    /// Will throw errors for everything but <see cref="GetRoles"/>.
    /// </summary>
    public class NoRolesStorage : IRoleStorage
    {
        /// <summary>
        /// Add a member to a role
        /// </summary>
        /// <param name="roleName">role</param>
        /// <param name="accountName">account</param>
        /// <exception cref="System.NotSupportedException"></exception>
        public void AddMember(string roleName, string accountName)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Create a new role
        /// </summary>
        /// <param name="roleName">Role to create</param>
        /// <exception cref="System.NotSupportedException"></exception>
        public void Create(string roleName)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Delete a role
        /// </summary>
        /// <param name="roleName"></param>
        /// <exception cref="System.NotSupportedException"></exception>
        public void Delete(string roleName)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Get all members of a specific role
        /// </summary>
        /// <param name="roleName">Name of the role</param>
        /// <returns>
        /// A list of all users (or an empty array)
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        public string[] GetMembersOfRole(string roleName)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns an empty list
        /// </summary>
        /// <param name="accountName">username</param>
        /// <returns></returns>
        public string[] GetRoles(string accountName)
        {
            return new string[0];
        }

        /// <summary>
        /// Remove a member from a role
        /// </summary>
        /// <param name="roleName">role</param>
        /// <param name="accountName">account</param>
        /// <exception cref="System.NotSupportedException"></exception>
        public void RemoveMember(string roleName, string accountName)
        {
            throw new NotSupportedException();
        }
    }
}
