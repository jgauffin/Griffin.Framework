using System;
using Griffin.Framework.Text;
using Griffin.Framework.Validation.Attributes;

namespace Griffin.Framework.Validation.Rules
{
    /// <summary>
    /// Checks if a primitive type is between min and max.
    /// </summary>
    [Localize("en-us", "'{0}' must be between {1} and {2}.")]
    [Localize("en-us", "String", "'{0}' may only have {1} up to {2} characters.")]
    [Serializable]
    public class BetweenRule : IRule
    {
        private readonly object _max;
        private readonly object _min;
        private object _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateAttribute"/> class.
        /// </summary>
        /// <param name="min">Value must be equal to this or larger.</param>
        /// <param name="max">Value must be equal or less than this.</param>
        /// <exception cref="ArgumentException">Thrown if the max value is more than the min value</exception>
        public BetweenRule(object min, object max)
        {
            var comparable = (IComparable) min;
            if (comparable.CompareTo(max) > 0)
                throw new ArgumentException("max cannot be less than min.");
            _min = min;
            _max = max;
        }


        /// <summary>
        /// Max value
        /// </summary>
        public object Max
        {
            get { return _max; }
        }

        /// <summary>
        /// Min value
        /// </summary>
        public object Min
        {
            get { return _min; }
        }

        #region IRule Members

        /// <summary>
        /// Validate a field value
        /// </summary>
        /// <param name="context">Context used to identify the model and provide the value being validated.</param>
        /// <returns>
        ///   <c>true</c> if validation was successful; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="System.NotSupportedException">Is neither a string or implementing <c>IComparable</c></exception>
        public bool Validate(ValidationContext context)
        {
            if (ValueChecker.IsEmpty(context.Value))
                return true;

            _value = context.Value;
            if (context.Value is string)
            {
                var s = (string)context.Value;
                return s.Length >= (int) Min && s.Length <= (int) Max;
            }

            var comparable = context.Value as IComparable;
            if (comparable != null)
                return comparable.CompareTo(Max) <= 0 && comparable.CompareTo(Min) >= 0;


            throw new NotSupportedException(string.Format("'{0}' do not support '{1}'", GetType(), context.Value.GetType()));
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
            if (_value is string)
                return string.Format(Localize.A.Type<BetweenRule>("String"),
                                     fieldName, _min, _max);

            return string.Format(Localize.A.Type<BetweenRule>("Class"), fieldName, _min, _max);
        }

        /// <summary>
        /// Checks if this rule support values of the specified format.
        /// </summary>
        /// <param name="type">Type of value</param>
        /// <returns><c>true</c> if the specified type can be validated; otherwise <c>false</c>.</returns>
        public bool SupportsType(Type type)
        {
            return typeof (IComparable).IsAssignableFrom(type);
        }

        #endregion
    }
}