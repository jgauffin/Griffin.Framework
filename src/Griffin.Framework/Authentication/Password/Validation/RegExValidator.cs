using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Griffin.Framework.Authentication.Password.Validation
{
    /// <summary>
    /// Uses a regular expression to validate the password
    /// </summary>
    public class RegExValidator : ValidatorBase
    {
        private readonly string _errorMessage;
        private readonly Regex _regEx;

        public RegExValidator(string regEx, string errorMessage)
        {
            _regEx = new Regex(regEx);
            _errorMessage = errorMessage;
        }


        /// <summary>
        /// Validate password
        /// </summary>
        /// <param name="password">Password</param>
        /// <returns>Empty list if successful.</returns>
        public override IEnumerable<IPasswordValidationError> TryValidate(string password)
        {
            if (!_regEx.IsMatch(password))
                yield return new PasswordValidationError(this, _errorMessage);
        }
    }
}