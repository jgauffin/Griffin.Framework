using System;
using Griffin.Framework.Text;
using Griffin.Framework.Validation.Rules;

namespace Griffin.Framework.Validation
{
    /// <summary>
    /// Only used to translate custom error messages.
    /// </summary>
    [Serializable]
    internal class TranslationRule : IRule
    {
        private readonly string _formatting;

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationRule"/> class.
        /// </summary>
        /// <param name="formatting">Text with a formatter for the field name only. No formatters = already been translated.</param>
        public TranslationRule(string formatting)
        {
            _formatting = formatting;
        }

        /// <summary>
        /// Format an error message
        /// </summary>
        /// <param name="fieldName">Field (have already been translated)</param>
        /// <param name="context">Context used to identify the model and provide the value being validated.</param>
        /// <returns>
        /// Error message formatted for the current language.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">fieldName</exception>
        /// <remarks>
        /// All rules should go through the <see cref="Localize" /> facade to retrive it's language texts. Caching will
        /// be done with the facade. Do note that all rules that support localization should be tagged using the <see cref="LocalizeAttribute" /> for every prompt that it has. By doing so it's easy to find all strings which can be localized.
        /// </remarks>
        public string Format(string fieldName, ValidationContext context)
        {
            if (fieldName == null) throw new ArgumentNullException("fieldName");
            var formatting = _formatting;

            if (formatting.Contains("{0}"))
                formatting = Localize.A.String(formatting, formatting, formatting);

            return string.Format(formatting, fieldName);
        }

        /// <summary>
        /// Checks if this rule support values of the specified format.
        /// </summary>
        /// <param name="type">Type of value</param>
        /// <returns><c>true</c> if the specified type can be validated; otherwise <c>false</c>.</returns>
        public bool SupportsType(Type type)
        {
            return true;
        }

        /// <summary>
        /// Validate a field value
        /// </summary>
        /// <param name="context">Context used to identify the model and provide the value being validated.</param>
        /// <returns>
        ///   <c>true</c> if validation was successful; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool Validate(ValidationContext context)
        {
            return false;
        }
    }
}