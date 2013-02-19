using System;

namespace Griffin.Framework.Authentication.Password.Validation
{
    /// <summary>
    /// Default implementation of <see cref="IPasswordValidationError"/>.
    /// </summary>
    public class PasswordValidationError : IPasswordValidationError
    {
        public PasswordValidationError(IPasswordValidator source, string errorMessage)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (errorMessage == null) throw new ArgumentNullException("errorMessage");
            ErrorMessage = errorMessage;
            Source = source;
        }

        /// <summary>
        /// Gets validator that generated the error
        /// </summary>
        public IPasswordValidator Source { get; private set; }

        /// <summary>
        /// Gets error
        /// </summary>
        public string ErrorMessage { get; private set; }
    }
}
