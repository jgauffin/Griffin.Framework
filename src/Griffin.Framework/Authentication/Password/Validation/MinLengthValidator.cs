using System;
using System.Collections.Generic;
using Griffin.Framework.Text;

namespace Griffin.Framework.Authentication.Password.Validation
{
    /// <summary>
    /// Password must be a minimum length
    /// </summary>
    [Localize("en", "Password must be at least {0} digits long.")]
    public class MinLengthValidator : ValidatorBase
    {
        private readonly string _errorMessage;
        private readonly int _minLength;

        public MinLengthValidator(int minLength)
        {
            if (minLength < 1)
                throw new ArgumentOutOfRangeException("minLength", minLength,
                                                      "Password length must be at least one character.");

            _minLength = minLength;
            _errorMessage = "Password must be at least {0} characters.";
        }

        public MinLengthValidator(int minLength, string errorMessage)
        {
            if (minLength < 1)
                throw new ArgumentOutOfRangeException("minLength", minLength,
                                                      "Password length must be at least one character.");

            _minLength = minLength;
            _errorMessage = errorMessage;
        }


        /// <summary>
        /// Validate password
        /// </summary>
        /// <param name="password">Password</param>
        /// <returns>Empty list if successful.</returns>
        public override IEnumerable<IPasswordValidationError> TryValidate(string password)
        {
            if (password.Length < _minLength)
                yield return new PasswordValidationError(this, string.Format(_errorMessage, _minLength));
        }
    }
}