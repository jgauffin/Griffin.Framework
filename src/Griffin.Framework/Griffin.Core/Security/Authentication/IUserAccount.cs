namespace Griffin.Security.Authentication
{
    /// <summary>
    ///     Account used for authentication.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This should be considered as a base class, on which you add your own fields. The object that you create with be
    ///         passed
    ///         through the entire infrastructure so that you can use it to populate the claims principal (if the
    ///         authentication succeeds).
    ///     </para>
    /// </remarks>
    public class UserAccount
    {
        /// <summary>
        ///     Hashed password. Some authentication schemes requires a specific type of hashing.
        /// </summary>
        public string HashedPassword { get; set; }

        /// <summary>
        ///     Account is locked and may not be used to login.
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        ///     Salt used to hash password.
        /// </summary>
        public string PasswordSalt { get; set; }

        /// <summary>
        ///     User name, as entered by the user during authentication/login.
        /// </summary>
        public string UserName { get; set; }
    }
}