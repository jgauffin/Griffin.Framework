using System;
using Griffin.Framework.Text;

namespace Griffin.Framework.Validation.Rules
{
    /// <summary>
    /// Base interface for all rules.
    /// </summary>
    /// <remarks>
    /// Rules are used to validate data.
    /// </remarks>
    public interface IRule
    {
        /// <summary>
        /// Format an error message
        /// </summary>
        /// <param name="fieldName">Field (have already been translated)</param>
        /// <param name="context">Context used to identify the model and provide the value being validated.</param>
        /// <returns>Error message formatted for the current language.</returns>
        /// <remarks>All rules should go through the <see cref="Localize"/> facade to retrive it's language texts. Caching will
        /// be done with the facade. Do note that all rules that support localization should be tagged using the <see cref="LocalizeAttribute"/> for every prompt that it has. By doing so it's easy to find all strings which can be localized.</remarks>
        string Format(string fieldName, ValidationContext context);

        /// <summary>
        /// Checks if this rule support values of the specified format.
        /// </summary>
        /// <param name="type">Type of value</param>
        /// <returns><c>true</c> if the specified type can be validated; otherwise <c>false</c>.</returns>
        bool SupportsType(Type type);

        /// <summary>
        /// Validate a field value
        /// </summary>
        /// <param name="context">Context used to identify the model and provide the value being validated.</param>
        /// <returns><c>true</c> if validation was successful; otherwise <c>false</c>.</returns>
        bool Validate(ValidationContext context);
    }
}