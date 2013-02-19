using System;
using Griffin.Framework.Text;

namespace Griffin.Framework.Validation.Rules
{
    /// <summary>
    /// Must only contain the specified letters (does not fail if the string is null or empty)
    /// </summary>
    [Localize("en-us", "'{0}' may only contain one or more of the following characters: '{1}'.")]
    [Serializable]
    public class ContainsOnlyRule : IRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContainsOnlyRule"/> class.
        /// </summary>
        /// <param name="letters">letters that the field must contain.</param>
        public ContainsOnlyRule(string letters)
        {
            Letters = letters;
        }

        /// <summary>
        /// Gets letters that the field must contain.
        /// </summary>
        public string Letters { get; private set; }

        #region IRule Members

        /// <summary>
        /// Validate a field value
        /// </summary>
        /// <param name="context">Value to validate</param>
        /// <returns><c>true</c> if validation was successful; otherwise <c>false</c>.</returns>
        /// <exception cref="NotSupportedException">Value type is not string.</exception>
        public bool Validate(ValidationContext context)
        {
            var str = context.Value as string;
            if (str == null)
                throw new NotSupportedException(GetType() + " only supports string properties.");

            if (string.IsNullOrEmpty(str))
                return true;

            foreach (var c in str)
            {
                if (Letters.IndexOf(c) == -1)
                    return false;
            }

            return true;
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
            return string.Format(Localize.A.Type<ContainsOnlyRule>("Class"), fieldName, Letters);
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