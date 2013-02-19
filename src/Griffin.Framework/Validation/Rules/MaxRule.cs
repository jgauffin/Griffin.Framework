using System;
using Griffin.Framework.Text;

namespace Griffin.Framework.Validation.Rules
{
    /// <summary>
    ///     Specifies a max length.
    /// </summary>
    /// <remarks>
    ///     On a string it specified maximum number of letters, while on int it specifies the max number.
    /// </remarks>
    [Localize("en-us", "'{0}' must be less or equal to {1}.")]
    [Localize("en-us", "String", "'{0}' must be contain {1} or less characters.")]
    [Serializable]
    public class MaxRule : IRule
    {
        private readonly object _max;


        /// <summary>
        ///     Initializes a new instance of the <see cref="MaxRule" /> class.
        /// </summary>
        /// <param name="max">
        ///     max length. should match the type being validated, with the exception of string
        ///     where the type should be int.
        /// </param>
        public MaxRule(object max)
        {
            _max = max;
        }

        /// <summary>
        ///     Max length
        /// </summary>
        public object Max
        {
            get { return _max; }
        }

        #region IRule Members

        /// <summary>
        ///     Validate a field value
        /// </summary>
        /// <param name="context">Context used to identify the model and provide the value being validated.</param>
        /// <returns>
        ///     <c>true</c> if validation was successful; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="System.NotSupportedException"></exception>
        /// <exception cref="NotSupportedException">Value type is not supported by this rule.</exception>
        public bool Validate(ValidationContext context)
        {
            if (ValueChecker.IsEmpty(context.Value))
                return true;

            if (context.Value is string)
            {
                var s = (string) context.Value;
                return s.Length <= (int) _max;
            }

            var comparable = context.Value as IComparable;
            if (comparable != null)
                return comparable.CompareTo(_max) <= 0;


            throw new NotSupportedException(GetType() + " do not support " + context.Value.GetType());
        }

        /// <summary>
        ///     Format an error message
        /// </summary>
        /// <param name="fieldName">Field (have already been translated)</param>
        /// <param name="context">Context used to identify the model and provide the value being validated.</param>
        /// <returns>
        ///     Error message formatted for the current language.
        /// </returns>
        /// <remarks>
        ///     All rules should go through the <see cref="Localize" /> facade to retrive it's language texts. Caching will
        ///     be done with the facade. Do note that all rules that support localization should be tagged using the
        ///     <see
        ///         cref="LocalizeAttribute" />
        ///     for every prompt that it has. By doing so it's easy to find all strings which can be localized.
        /// </remarks>
        public string Format(string fieldName, ValidationContext context)
        {
            var msg = context.Value is string
                          ? Localize.A.Type<MaxRule>("Class", "String")
                          : Localize.A.Type<MaxRule>();
            return string.Format(msg, fieldName, Max);
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
            return type.GetInterface("IComparable", false) != null;
        }

        #endregion
    }
}