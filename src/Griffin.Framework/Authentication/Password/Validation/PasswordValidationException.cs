using System;
using System.Collections.Generic;
using System.Linq;

namespace Griffin.Framework.Authentication.Password.Validation
{
    /// <summary>
    /// Thrown if password is not valid according the the chosen rules.
    /// </summary>
    public class PasswordValidationException : Exception
    {
        public PasswordValidationException(string errorMessage)
            : base(errorMessage)
        {

        }

        public PasswordValidationException(IEnumerable<IPasswordValidationError> errorMessages)
            : base(string.Join(", ", errorMessages.Select(x => x.ErrorMessage)))
        {

        }
    }
}