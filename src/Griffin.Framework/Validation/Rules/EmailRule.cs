using System;
using System.Text.RegularExpressions;
using Griffin.Framework.Text;

namespace Griffin.Framework.Validation.Rules
{
    /// <summary>
    /// Validates that a value is a correct email address.
    /// </summary>
    /// <para>
    /// Language file items:
    /// <list type="table">
    ///     <listheader>
    ///         <term>ItemName</term>
    ///         <description>Language text</description>
    ///     </listheader>
    ///     <item>
    ///         <term>Email</term>
    ///         <description>'{0}' must be a valid email address.</description>
    ///     </item>
    /// </list>
    /// </para>
    [Localize("en-us", "'{0}' must contain a valid email address.")]
    [Serializable]
    public class EmailRule : IRule
    {
        private const string EmailRegEx = @"^[a-zA-Z0-9_\.\-]+@[a-zA-Z0-9_\.\-]+\.[a-zA-Z]{2,4}$";

        #region IRule Members

        /// <summary>
        /// Validate a field value
        /// </summary>
        /// <param name="context">Context used to identify the model and provide the value being validated.</param>
        /// <returns>
        ///   <c>true</c> if validation was successful; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        /// <exception cref="NotSupportedException">Only string types are supported.</exception>
        public bool Validate(ValidationContext context)
        {
            if (context.Value == null)
                return true;

            if (context.Value is string)
            {
                var email = (string)context.Value;
                return string.IsNullOrEmpty(email) || Regex.IsMatch(email, EmailRegEx);
            }

            throw new NotSupportedException(string.Format("{0} is not supported.", context.Value.GetType()));
        }

        /// <summary>
        /// Format an error message
        /// </summary>
        /// <param name="fieldName">Field (have already been translated)</param>
        /// <param name="context">Context used to identify the model and provide the value being validated.</param>
        /// <returns>
        /// Error message formatted for the current language.
        /// </returns>
        /// <remarks>
        /// All rules should go through the <see cref="Localize" /> facade to retrive it's language texts. Caching will
        /// be done with the facade. Do note that all rules that support localization should be tagged using the <see cref="LocalizeAttribute" /> for every prompt that it has. By doing so it's easy to find all strings which can be localized.
        /// </remarks>
        public string Format(string fieldName, ValidationContext context)
        {
            return string.Format(Localize.A.Type<EmailRule>(), fieldName);
        }

        /// <summary>
        /// Checks if this rule support values of the specified format.
        /// </summary>
        /// <param name="type">Type of value</param>
        /// <returns><c>true</c> if the specified type can be validated; otherwise <c>false</c>.</returns>
        public bool SupportsType(Type type)
        {
            return typeof (string).IsAssignableFrom(type);
        }

        #endregion
    }
}