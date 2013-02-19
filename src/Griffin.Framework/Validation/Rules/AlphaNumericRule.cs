using System;
using Griffin.Framework.Text;

namespace Griffin.Framework.Validation.Rules
{
    /// <summary>
    /// Validates that a string only contains letter, digits and any of: <![CDATA['"\<>|-_.:,;'^~¨'*!#¤%&/()=?`´+}][{€$££@§½ ]]>
    /// </summary>
    [Localize("en-us", "'{0}' may only contain alpha numeric characters.")]
    [Serializable]
    public class AlphaNumericRule : LettersAndDigitsRule
    {
        private const string ValidChars = "'\"\\<>|-_.:,;'^~¨'*!#¤%&/()=?`´+}][{€$££@§½€ ";

        /// <summary>
        /// Initializes a new instance of the <see cref="AlphaNumericRule"/> class.
        /// </summary>
        public AlphaNumericRule()
            : this(ValidChars)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlphaNumericRule"/> class.
        /// </summary>
        /// <param name="extraCharacters">The extra characters.</param>
        public AlphaNumericRule(string extraCharacters)
            : base(extraCharacters + ValidChars)
        {
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
        public override string Format(string fieldName, ValidationContext context)
        {
            var str = Localize.A.Type<AlphaNumericRule>("Class");
            return string.Format(str, fieldName);
        }
    }
}