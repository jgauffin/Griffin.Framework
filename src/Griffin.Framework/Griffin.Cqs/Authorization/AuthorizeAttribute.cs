using System;

namespace Griffin.Cqs.Authorization
{
    /// <summary>
    /// Allows you to restrict access to different CQS objects.
    /// </summary>
    /// <remarks>
    /// <para>Using multiple roles in the same attribute works like OR (i.e. user must be in one of the defined roles). Using multiple attributes
    /// on the same object works like AND (i.e. the user must be in one of the roles that are defined in all of the used attributes)</para>
    /// </remarks>
    /// <example>
    /// <para>Must be either in Administrator or SuperUser role.</para>
    /// <code>
    /// <![CDATA[
    /// [Authorize("Administrator", "SuperUser")]
    /// public class ActivateUser : Command
    /// {
    ///     public ActivateUser(int userId)
    ///     {
    ///         if (userId <= 0) throw new ArgumentOutOfRangeException("userId", "userId", "Invalid user id. Did you fail to parse it?");
    ///         UserId = userId;
    ///     }
    /// 
    ///     public int UserId { get; private set; }
    /// }
    /// ]]>
    /// </code>
    /// <para>Must be both in Administrator and Organization1 role.</para>
    /// <code>
    /// <![CDATA[
    /// [Authorize("Administrator")]
    /// [Authorize("Organization1")]
    /// public class ActivateUser : Command
    /// {
    ///     public ActivateUser(int userId)
    ///     {
    ///         if (userId <= 0) throw new ArgumentOutOfRangeException("userId", "userId", "Invalid user id. Did you fail to parse it?");
    ///         UserId = userId;
    ///     }
    /// 
    ///     public int UserId { get; private set; }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class AuthorizeAttribute : Attribute
    {
        public AuthorizeAttribute(params string[] roles)
        {
            if (roles == null) throw new ArgumentNullException("roles");
            Roles = roles;
        }

        /// <summary>
        ///     Gets a list of role names
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Do not have to be traditional roles, you can use articial roles like "Organization1" to indicate that the user
        ///         only have access
        ///         to a specific organization in a multi-tenant solution.
        ///     </para>
        /// </remarks>
        public string[] Roles { get; private set; }
    }
}