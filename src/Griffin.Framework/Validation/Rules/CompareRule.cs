using System;
using Griffin.Framework.Text;

namespace Griffin.Framework.Validation.Rules
{
    /// <summary>
    ///     Compares two different properties.
    /// </summary>
    [Localize("en-us", "The value of '{0}' must equal the value of '{0}'.")]
    public class CompareRule : IRule
    {
        private readonly string _otherPropertyName;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CompareRule" /> class.
        /// </summary>
        /// <param name="otherPropertyName">Name of the other property (that we should compare value with).</param>
        /// <exception cref="System.ArgumentNullException">otherPropertyName</exception>
        public CompareRule(string otherPropertyName)
        {
            if (otherPropertyName == null) throw new ArgumentNullException("otherPropertyName");
            _otherPropertyName = otherPropertyName;
        }

        /// <summary>
        ///     Format an error message
        /// </summary>
        /// <param name="fieldName">Field (have already been translated)</param>
        /// <param name="context">Context used to identify the model and provide the value being validated.</param>
        /// <returns>
        ///     Error message formatted for the current language.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        /// <remarks>
        ///     All rules should go through the <see cref="Localize" /> facade to retrive it's language texts. Caching will
        ///     be done with the facade. Do note that all rules that support localization should be tagged using the
        ///     <see
        ///         cref="LocalizeAttribute" />
        ///     for every prompt that it has. By doing so it's easy to find all strings which can be localized.
        /// </remarks>
        public string Format(string fieldName, ValidationContext context)
        {
            return string.Format(Localize.A.Type<CompareRule>("Class"), fieldName,
                                 Localize.A.Type(context.Model.GetType(), _otherPropertyName));
        }

        /// <summary>
        ///     Checks if this rule support values of the specified format.
        /// </summary>
        /// <param name="type">Type of value</param>
        /// <returns>
        ///     <c>true</c> if the specified type can be validated; otherwise <c>false</c>.
        /// </returns>
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
        public bool Validate(ValidationContext context)
        {
            //TODO: Make it more robust by allowing errors such as lousy sepllning ;)
            var otherValue = context.Model.GetType().GetProperty(_otherPropertyName).GetValue(context.Model);
            return otherValue.Equals(context.Value);
        }
    }
}