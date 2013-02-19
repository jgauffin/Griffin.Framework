using System;
using Griffin.Framework.Text;

namespace Griffin.Framework.Validation.Rules
{
    /// <summary>
    /// Checks if value is a valid hex number.
    /// </summary>
    [Localize("en-us", "Hex", "'{0}' must be a valid HEX number.")]
    [Serializable]
    public class HexRule : IRule
    {
        #region IRule Members

        /// <summary>
        /// Validate a field value
        /// </summary>
        /// <param name="context">Context used to identify the model and provide the value being validated.</param>
        /// <returns>
        ///   <c>true</c> if validation was successful; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        /// <exception cref="NotSupportedException">Only strings are supported.</exception>
        public bool Validate(ValidationContext context)
        {
            if (ValueChecker.IsEmpty(context.Value))
                return true;

            var temp = context.Value as string;
            if (temp == null)
                throw new NotSupportedException(GetType() + " only supports string types.");

            temp = temp.ToLower();
            foreach (var ch in temp)
            {
                if (ch < 'a' || ch > 'f')
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
            return string.Format(Localize.A.Type<HexRule>(), fieldName);
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