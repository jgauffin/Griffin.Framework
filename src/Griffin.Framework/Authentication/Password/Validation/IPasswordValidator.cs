using System.Collections.Generic;

namespace Griffin.Framework.Authentication.Password.Validation
{
    /// <summary>
    /// Used to validate the password
    /// </summary>
    public interface IPasswordValidator
    {
        /// <summary>
        /// Validate the password
        /// </summary>
        /// <param name="password">Password to validate</param>
        /// <exception cref="PasswordValidationException">thrown if validation failed.</exception>
        void Validate(string password);

        /// <summary>
        /// Validate password
        /// </summary>
        /// <param name="password">Password</param>
        /// <returns>Empty list if successful.</returns>
        IEnumerable<IPasswordValidationError> TryValidate(string password);
    }
}
