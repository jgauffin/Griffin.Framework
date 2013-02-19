using System.Collections.Generic;
using System.Linq;

namespace Griffin.Framework.Authentication.Password.Validation
{
    public abstract class ValidatorBase : IPasswordValidator
    {
        /// <summary>
        /// Validate the password
        /// </summary>
        /// <param name="password">Password to validate</param>
        /// <exception cref="PasswordValidationException">thrown if validation failed.</exception>
        public void Validate(string password)
        {
            var errors = TryValidate(password);
            if (Enumerable.Any<IPasswordValidationError>(errors))
                throw new PasswordValidationException(errors);
        }

        /// <summary>
        /// Validate password
        /// </summary>
        /// <param name="password">Password</param>
        /// <returns>Empty list if successful.</returns>
        public abstract IEnumerable<IPasswordValidationError> TryValidate(string password);
    }
}