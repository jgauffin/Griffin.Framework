using System;
using Griffin.Framework.Text;

namespace Griffin.Framework.Validation.Rules
{
    /// <summary>
    ///     Specifies minimum length
    /// </summary>
    /// <remarks>
    ///     On a string it's the specified maximum number of letters, while on int it specifies the max number.
    ///     Works on all IComparable types.
    /// </remarks>
    [Localize("en-us", "'{0}' must be larger or equal to {1}.")]
    [Localize("en-us", "String", "'{0}' must be contain {1} or more characters.")]
    [Serializable]
    public class MinRule : IRule
    {
        private readonly object _minLength;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MinRule" /> class.
        /// </summary>
        /// <param name="value">
        ///     minimum length. should match the type being validated, with the exception of string
        ///     where the type should be int.
        /// </param>
        public MinRule(object value)
        {
            _minLength = value;
        }

        /// <summary>
        ///     Minimum length
        /// </summary>
        public object MinLength
        {
            get { return _minLength; }
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
        /// <exception cref="NotSupportedException">Value type is not supported.</exception>
        public bool Validate(ValidationContext context)
        {
            if (ValueChecker.IsEmpty(context.Value))
                return true;

            if (context.Value is string)
            {
                var s = (string) context.Value;
                return s.Length >= (int) _minLength;
            }

            var comparable = context.Value as IComparable;
            if (comparable != null)
                return comparable.CompareTo(_minLength) >= 0;


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
            string str = context.Value is string
                             ? Localize.A.Type<MinRule>("Class", "String")
                             : Localize.A.Type<MinRule>();

            return string.Format(str, fieldName, _minLength);
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