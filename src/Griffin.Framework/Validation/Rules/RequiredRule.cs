using System;
using Griffin.Framework.Text;

namespace Griffin.Framework.Validation.Rules
{
    /// <summary>
    /// Checks whether a field have been specified or not.
    /// </summary>
   
    [Localize("en-us", "Required", "'{0}' is required.")]
    [Serializable]
    public class RequiredRule : IRule
    {
        private readonly RequiredFlags _flags;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredRule"/> class.
        /// </summary>
        public RequiredRule()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredRule"/> class.
        /// </summary>
        /// <param name="flags">The flags.</param>
        public RequiredRule(RequiredFlags flags)
        {
            _flags = flags;
        }

        #region IRule Members

        /// <summary>
        /// Validate a field value
        /// </summary>
        /// <param name="context">Context used to identify the model and provide the value being validated.</param>
        /// <returns>
        ///   <c>true</c> if validation was successful; otherwise <c>false</c>.
        /// </returns>
        public bool Validate(ValidationContext context)
        {
            if (context.Value == null)
                return false;

            var temp = context.Value as string;
            if (temp == string.Empty)
                return (_flags & RequiredFlags.AcceptEmptyString) != 0;

            return !ValueChecker.IsEmpty(context.Value);
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
            return string.Format(Localize.A.Type<RequiredRule>(), fieldName);
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

        #endregion
    }
}