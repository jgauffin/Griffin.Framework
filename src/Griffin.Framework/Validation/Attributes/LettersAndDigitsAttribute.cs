using Griffin.Framework.Validation.Rules;

namespace Griffin.Framework.Validation.Attributes
{
    /// <summary>
    /// Input may only be letters and digits, foreign chars (for instance åäö) etc are included.
    /// </summary>
    /// <para>
    /// Language file items:
    /// <list type="table">
    ///     <listheader>
    ///         <term>ItemName</term>
    ///         <description>Language text</description>
    ///     </listheader>
    ///     <item>
    ///         <term>LettersAndDigits</term>
    ///         <description>'{0}' may only contain letters and digits.</description>
    ///     </item>
    ///     <item>
    ///         <term>LettersAndDigitsExtra</term>
    ///         <description>'{0}' may only contain letters, digits and '{1}'.</description>
    ///     </item>
    /// </list>
    /// </para>
    public class LettersAndDigitsAttribute : ValidateAttribute
    {
        /// <summary>
        /// Foreign characters (letters used for instance in the Swedish language)
        /// </summary>
        public const string ForeignCharacters = "åäëïöüâêîÛôãõÄËÏÖÜÂÊÎÔÛÃÕáéíóúÁÉÍÓÚàèìòùÀÈÌòÙ";

        /// <summary>
        /// Foreign characters (letters used for instance in the Swedish language) and space, dot and comma.
        /// </summary>
        public const string ForeignCharactersSDC = "åäëïöüâêîÛôãõÄËÏÖÜÂÊÎÔÛÃÕáéíóúÁÉÍÓÚàèìòùÀÈÌòÙ .,";

        /// <summary>
        /// Space, dot and comma.
        /// </summary>
        public const string SpaceCommaDot = " .,";

        private readonly string _extraCharacters;

        /// <summary>
        /// Initializes a new instance of the <see cref="LettersAndDigitsAttribute"/> class.
        /// </summary>
        public LettersAndDigitsAttribute() : this(string.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LettersAndDigitsAttribute"/> class.
        /// </summary>
        /// <param name="extraCharacters">The extra characters.</param>
        public LettersAndDigitsAttribute(string extraCharacters)
        {
            _extraCharacters = extraCharacters;
        }


        /// <summary>
        /// Create a rule based on this attribute.
        /// </summary>
        /// <returns>Created rule</returns>
        public override IRule CreateRule()
        {
            return new LettersAndDigitsRule(_extraCharacters);
        }
    }
}