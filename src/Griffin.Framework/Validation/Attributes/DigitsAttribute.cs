using System;
using Griffin.Framework.Validation.Rules;

#if TEST
using Xunit;
#endif

namespace Griffin.Framework.Validation.Attributes
{
    /// <summary>
    /// Makes sure that only digits have been entered.
    /// </summary>
    /// <para>
    /// Language file items:
    /// <list type="table">
    ///     <listheader>
    ///         <term>ItemName</term>
    ///         <description>Language text</description>
    ///     </listheader>
    ///     <item>
    ///         <term>Digits</term>
    ///         <description>'{0}' may only contain digits.</description>
    ///     </item>
    ///     <item>
    ///         <term>DigitsExtra</term>
    ///         <description>'{0}' may only contain digits and "{1}".</description>
    ///     </item>
    /// </list>
    /// </para>
    public class DigitsAttribute : ValidateAttribute
    {
        private readonly string _extraChars;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateAttribute"/> class.
        /// </summary>
        public DigitsAttribute()
        {
            _extraChars = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateAttribute"/> class.
        /// </summary>
        /// <param name="extraChars">extra characters that are valid.</param>
        /// <exception cref="ArgumentNullException">Argument is <c>null</c>.</exception>
        public DigitsAttribute(string extraChars)
        {
            if (string.IsNullOrEmpty(extraChars))
                _extraChars = string.Empty;

            _extraChars = extraChars;
        }

        /// <summary>
        /// Create a rule based on this attribute.
        /// </summary>
        /// <returns>Created rule</returns>
        public override IRule CreateRule()
        {
            return new DigitsRule(_extraChars);
        }
    }
}