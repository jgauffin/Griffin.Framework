using System;
using Griffin.Framework.Validation.Rules;

namespace Griffin.Framework.Validation
{
    /// <summary>
    /// Manually validate fields.
    /// </summary>
    public class ManualValidator
    {
        private readonly object _model;
        private readonly ValidationErrors _errors;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManualValidator" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException"><c>language</c> is <c>null</c>.</exception>
        public ManualValidator(object model)
        {
            _model = model;
            _errors = new ValidationErrors(model.GetType());
        }

        /// <summary>
        /// Gets all validation errors
        /// </summary>
        public ValidationErrors Errors
        {
            get
            {
                return _errors;
            }
        }

        /// <summary>
        /// Validate using password rule
        /// </summary>
        /// <param name="fieldName">Name of field (used in error message)</param>
        /// <param name="value">Value to validate</param>
        /// <returns>Value if validation succeeded; otherwise <c>null</c>.</returns>
        /// <seealso cref="PasswordRule"/>
        public string Digits(string fieldName, string value)
        {
            return Validate(new DigitsRule(), fieldName, value) ? value : null;
        }

        /// <summary>
        /// Validate using password rule
        /// </summary>
        /// <param name="fieldName">Name of field (used in error message)</param>
        /// <param name="value">Value to validate</param>
        /// <param name="extraCharacters">Allowed characters (digits excluded)</param>
        /// <returns>Value if validation succeeded; otherwise <c>null</c>.</returns>
        /// <seealso cref="PasswordRule"/>
        public string Digits(string fieldName, string value, string extraCharacters)
        {
            return Validate(new DigitsRule(extraCharacters), fieldName, value) ? value : null;
        }

        /// <summary>
        /// Validate using email rule
        /// </summary>
        /// <param name="fieldName">Name of field (used in error message)</param>
        /// <param name="value">Value to validate</param>
        /// <returns>Value if validation succeeded; otherwise <c>null</c>.</returns>
        /// <seealso cref="EmailRule"/>
        public string Email(string fieldName, string value)
        {
            return Validate(new EmailRule(), fieldName, value) ? value : null;
        }

        /// <summary>
        /// Validate using password rule
        /// </summary>
        /// <param name="fieldName">Name of field (used in error message)</param>
        /// <param name="value">Value to validate</param>
        /// <returns>Value if validation succeeded; otherwise <c>null</c>.</returns>
        /// <seealso cref="PasswordRule"/>
        public int Integer(string fieldName, string value)
        {
            if (Validate(new DigitsRule(), fieldName, value))
            {
                int temp;
                if (int.TryParse(fieldName, out temp))
                    return temp;
            }

            return -1;
        }

        /// <summary>
        /// Validate using password rule
        /// </summary>
        /// <param name="fieldName">Name of field (used in error message)</param>
        /// <param name="value">Value to validate</param>
        /// <returns>Value if validation succeeded; otherwise <c>null</c>.</returns>
        /// <seealso cref="PasswordRule"/>
        public string Password(string fieldName, string value)
        {
            return Validate(new PasswordRule(), fieldName, value) ? value : null;
        }

        /// <summary>
        /// Validate a field
        /// </summary>
        /// <param name="rule">Rule to validate against.</param>
        /// <param name="fieldName">Field name (before translations)</param>
        /// <param name="value">Value to validate</param>
        /// <returns><c>true</c> if validation succeeded; otherwise <c>false</c>.</returns>
        protected bool Validate(IRule rule, string fieldName, object value)
        {
            var ctx = new ValidationContext(_model, fieldName, value);
            if (!rule.Validate(ctx))
            {
                rule.Format(fieldName, ctx);
                return false;
            }

            return true;
        }
    }
}