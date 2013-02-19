using System;
using Griffin.Framework.Validation.Rules;

namespace Griffin.Framework.Validation.Attributes
{
    /// <summary>
    /// Specifies a max length.
    /// </summary>
    /// <remarks>
    /// On a string it specified maximum number of letters, while on int it specifies the max number.
    /// </remarks>
    /// <para>
    /// Language file items:
    /// <list type="table">
    ///     <listheader>
    ///         <term>ItemName</term>
    ///         <description>Language text</description>
    ///     </listheader>
    ///     <item>
    ///         <term>Max</term>
    ///         <description>'{0}' must be less or equal to {1}.</description>
    ///     </item>
    ///     <item>
    ///         <term>MaxString</term>
    ///         <description>'{0}' must be contain {1} or less characters.</description>
    ///     </item>
    /// </list>
    /// </para>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter)]
    public class MaxAttribute : ValidateAttribute
    {
        private readonly object _max;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxAttribute"/> class.
        /// </summary>
        /// <param name="max">max length. should match the type being validated, with the exception of string
        /// where the type should be int.</param>
        public MaxAttribute(object max)
        {
            _max = max;
        }


        /// <summary>
        /// Create a rule based on this attribute.
        /// </summary>
        /// <returns>Created rule</returns>
        public override IRule CreateRule()
        {
            return new MaxRule(_max);
        }
    }
}