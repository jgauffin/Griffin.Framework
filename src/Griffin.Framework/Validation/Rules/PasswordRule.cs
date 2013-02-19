using System;
using Griffin.Framework.Text;

namespace Griffin.Framework.Validation.Rules
{
    /// <summary>
    /// Validates password. Minimum 8 characters long and both digits and letters.
    /// </summary>
    [Localize("en-us", "'{0}' must contain at least five characters, both digits and letters.")]
    [Serializable]
    public class PasswordRule : IRule
    {
        #region IRule Members

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
            return string.Format(Localize.A.Type<PasswordRule>(), fieldName);
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

        /// <summary>
        /// Validate a field value
        /// </summary>
        /// <param name="context">Context used to identify the model and provide the value being validated.</param>
        /// <returns>
        ///   <c>true</c> if validation was successful; otherwise <c>false</c>.
        /// </returns>
        public bool Validate(ValidationContext context)
        {
            if (string.IsNullOrEmpty(context.Value as string))
                return false;

            var hasLetter = false;
            var hasDigit = false;
            var temp = (string)context.Value;
            if (temp.Length < 8)
                return false;

            foreach (var ch in temp)
            {
                if (char.IsDigit(ch))
                    hasDigit = true;
                if (char.IsLetter(ch))
                    hasLetter = true;
            }

            return hasLetter && hasDigit;
        }

        #endregion
    }
}