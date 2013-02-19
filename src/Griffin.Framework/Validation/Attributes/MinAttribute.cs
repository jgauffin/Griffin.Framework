using System;
using Griffin.Framework.Validation.Rules;

namespace Griffin.Framework.Validation.Attributes
{
    /// <summary>
    /// Specifies a max length.
    /// </summary>
    /// <remarks>
    /// On a string it specified maximum number of letters, while on int it specifies the max number.
    /// <para>
    /// Language file items:
    /// <list type="table">
    ///     <listheader>
    ///         <term>ItemName</term>
    ///         <description>Language text</description>
    ///     </listheader>
    ///     <item>
    ///         <term>Min</term>
    ///         <description>'{0}' must be at least {1}.</description>
    ///     </item>
    ///     <item>
    ///         <term>MinString</term>
    ///         <description>'{0}' must be at least {1} letters.</description>
    ///     </item>
    /// </list>
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter)]
    public class MinAttribute : ValidateAttribute
    {
        private readonly object _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="MinAttribute"/> class.
        /// </summary>
        /// <param name="value">minimum length. should match the type being validated, with the exception of string
        /// where the type should be int.</param>
        public MinAttribute(object value)
        {
            _value = value;
        }


        /// <summary>
        /// Create a rule based on this attribute.
        /// </summary>
        /// <returns>Created rule</returns>
        public override IRule CreateRule()
        {
            return new MinRule(_value);
        }
    }
}