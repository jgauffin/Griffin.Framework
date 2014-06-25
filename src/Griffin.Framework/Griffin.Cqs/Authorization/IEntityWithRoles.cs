namespace Griffin.Cqs.Authorization
{
    /// <summary>
    ///     Contract that defines that the CQS object contains a role and can therefore be validated against that role
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This interface is used in conjuction with the <see cref="AuthorizeAttribute" /> to be able to validate that the
    ///         logged in user
    ///         have access to the specified object.
    ///     </para>
    /// </remarks>
    public interface IEntityWithRoles
    {
        /// <summary>
        ///     Gets a list of role names that the user MUST be member in.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Do not have to be traditional roles, you can use articial roles like "Organization1" to indicate that the user
        ///         only have access
        ///         to a specific organization in a multi-tenant solution.
        ///     </para>
        /// </remarks>
        string[] Roles { get; }
    }
}