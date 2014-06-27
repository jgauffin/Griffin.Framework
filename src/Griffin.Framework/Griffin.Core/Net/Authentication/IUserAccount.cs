using Griffin.Security;

namespace Griffin.Net.Authentication
{
    /// <summary>
    ///     Implement this class with your own user account class.
    /// </summary>
    public interface IUserAccount
    {
        /// <summary>
        ///     User identity (as entered by the user during the login process)
        /// </summary>
        string UserName { get; }

        /// <summary>
        ///     Password hashed in the same was as used by <see cref="IPasswordHasher" />.
        /// </summary>
        string HashedPassword { get; }

        /// <summary>
        ///     Salt generated when the password was hashed.
        /// </summary>
        string PasswordSalt { get; }

        /// <summary>
        ///     Account is locked (the user may not login event if the password is correct)
        /// </summary>
        bool IsLocked { get; }
    }
}