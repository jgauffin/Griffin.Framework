namespace Griffin.Framework.Authentication
{
    /// <summary>
    /// Current state of an account
    /// </summary>
    public enum AccountState
    {
        /// <summary>
        /// Locked (might have behaved utterly destructive etc etc)
        /// </summary>
        Locked,

        /// <summary>
        /// Registered or reseted the password, but have not clicked on the activation link
        /// </summary>
        NotActivated,

        /// <summary>
        /// Invited (someone else have created the account, but the user have not clicked on the activation link)
        /// </summary>
        Invited,

        /// <summary>
        /// Alive and kicking.
        /// </summary>
        Active
    }
}