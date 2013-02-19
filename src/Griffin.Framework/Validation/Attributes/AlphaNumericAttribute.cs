using Griffin.Framework.Validation.Rules;

namespace Griffin.Framework.Validation.Attributes
{
    /// <summary>
    /// Validates that a string only contains letter, digits and any of: <![CDATA['"\<>|-_.:,;'^~¨'*!#¤%&/()=?`´+}][{€$££@§½ ]]>
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
    ///         <term>AlphaNumeric</term>
    ///         <description>'{0}' may only contain alpha numeric letters.</description>
    ///     </item>
    /// </list>
    /// </para>
    /// </remarks>
    public class AlphaNumericAttribute : ValidateAttribute
    {
        private readonly string _validChars;

        /// <summary>
        /// Initializes a new instance of the <see cref="AlphaNumericAttribute"/> class.
        /// </summary>
        public AlphaNumericAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlphaNumericAttribute"/> class.
        /// </summary>
        /// <param name="extraCharacters">The extra characters.</param>
        public AlphaNumericAttribute(string extraCharacters)
        {
            _validChars = extraCharacters;
        }


        /// <summary>
        /// Create a rule based on this attribute.
        /// </summary>
        /// <returns>Created rule</returns>
        public override IRule CreateRule()
        {
            return new AlphaNumericRule(_validChars);
        }
    }
}