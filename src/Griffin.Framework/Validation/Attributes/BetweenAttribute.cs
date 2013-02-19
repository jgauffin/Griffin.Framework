using System;
using Griffin.Framework.Validation.Rules;

namespace Griffin.Framework.Validation.Attributes
{
    /// <summary>
    /// Checks if a primitive type is between min and max.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Language file items:
    /// <list type="table">
    ///     <listheader>
    ///         <term>ItemName</term>
    ///         <description>Language text</description>
    ///     </listheader>
    ///     <item>
    ///         <term>Between</term>
    ///         <description>'{0}' must be between {1} and {2}.</description>
    ///     </item>
    ///     <item>
    ///         <term>BetweenString</term>
    ///         <description>'{0}' may only have {1} up to {2} characters.</description>
    ///     </item>
    /// </list>
    /// </para>
    /// </remarks>
    public class BetweenAttribute : ValidateAttribute
    {
        private readonly object _max;
        private readonly object _min;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateAttribute"/> class.
        /// </summary>
        /// <param name="min">Value must be equal to this or larger.</param>
        /// <param name="max">Value must be equal or less than this.</param>
        /// <exception cref="ArgumentException">Thrown if the max value is more than the min value</exception>
        public BetweenAttribute(object min, object max)
        {
            var comparable = (IComparable) min;
            if (comparable.CompareTo(max) > 0)
                throw new ArgumentException("max cannot be less than min.");
            _min = min;
            _max = max;
        }


        /// <summary>
        /// Create a rule based on this attribute.
        /// </summary>
        /// <returns>Created rule</returns>
        public override IRule CreateRule()
        {
            return new BetweenRule(_min, _max);
        }
    }
}