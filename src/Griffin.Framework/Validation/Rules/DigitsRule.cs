using System;
using Griffin.Framework.Text;
using Griffin.Framework.Validation.Attributes;

namespace Griffin.Framework.Validation.Rules
{
    /// <summary>
    /// Makes sure that only digits have been entered.
    /// </summary>
    [Localize("en-us", "The field '{0}' may only contain digits.")]
    [Localize("en-us", "WithExtra", "The field '{0}' may only contain digits and '{1}'.")]
    [Serializable]
    public class DigitsRule : IRule
    {
        private readonly string _extraChars;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateAttribute"/> class.
        /// </summary>
        public DigitsRule()
        {
            _extraChars = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateAttribute"/> class.
        /// </summary>
        /// <param name="extraChars">extra characters that are valid.</param>
        /// <exception cref="ArgumentNullException">Argument is <c>null</c>.</exception>
        public DigitsRule(string extraChars)
        {
            _extraChars = extraChars ?? string.Empty;
        }

        #region IRule Members

        /// <summary>
        /// Validate a field value
        /// </summary>
        /// <param name="context">Context used to identify the model and provide the value being validated.</param>
        /// <returns>
        ///   <c>true</c> if validation was successful; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        /// <exception cref="NotSupportedException">Supports only string values.</exception>
        public bool Validate(ValidationContext context)
        {
            if (ValueChecker.IsEmpty(context.Value))
                return true;

            if (context.Value is string)
            {
                var s = (string)context.Value;
                foreach (var ch in s)
                {
                    if (char.IsDigit(ch) || _extraChars.IndexOf(ch) != -1)
                        continue;

                    return false;
                }
                return true;
            }

            throw new NotSupportedException(GetType() + " only supports strings.");
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
        public string Format(string fieldName,ValidationContext context)
        {
            return string.IsNullOrEmpty(_extraChars) == false
                       ? string.Format(Localize.A.Type<DigitsRule>("Class", "Extra"), fieldName, "1234567890" + _extraChars)
                       : string.Format(Localize.A.Type<DigitsRule>(), fieldName);
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