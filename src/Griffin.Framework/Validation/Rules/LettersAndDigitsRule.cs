using System;
using System.Linq;
using Griffin.Framework.Text;

namespace Griffin.Framework.Validation.Rules
{
    /// <summary>
    /// Input may only be letters and digits, foreign chars (for instance åäö) etc are included.
    /// </summary>
    [Localize("en-us", "The field '{0}' may only contain letters and digits.")]
    [Localize("en-us", "Extra", "The field '{0}' may only contain letters, digits and '{1}'.")]
    [Serializable]
    public class LettersAndDigitsRule : IRule
    {
        /// <summary>
        /// Foreign characters (letters used for instance in the Swedish language)
        /// </summary>
        public static string ForeignCharacters = "åäëïöüâêîÛôãõÄËÏÖÜÂÊÎÔÛÃÕáéíóúÁÉÍÓÚàèìòùÀÈÌòÙ";

        /// <summary>
        /// Foreign characters (letters used for instance in the Swedish language) and space, dot and comma.
        /// </summary>
        public static string ForeignCharactersSDC = "åäëïöüâêîÛôãõÄËÏÖÜÂÊÎÔÛÃÕáéíóúÁÉÍÓÚàèìòùÀÈÌòÙ .,";

        /// <summary>
        /// Space, dot and comma.
        /// </summary>
        public const string SpaceCommaDot = " .,";


        /// <summary>
        /// Initializes a new instance of the <see cref="LettersAndDigitsRule"/> class.
        /// </summary>
        public LettersAndDigitsRule() : this(string.Empty)
        {
            ExtraCharacters = ForeignCharacters;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LettersAndDigitsRule"/> class.
        /// </summary>
        /// <param name="extraCharacters">The extra characters.</param>
        /// <exception cref="ArgumentNullException"><c>extraCharacters</c> is <c>null</c>.</exception>
        public LettersAndDigitsRule(string extraCharacters)
        {
            if (extraCharacters == null)
                throw new ArgumentNullException("extraCharacters");

            ExtraCharacters = extraCharacters + ForeignCharacters;
        }

        /// <summary>
        /// Gets the extra characters.
        /// </summary>
        /// <value>The extra characters.</value>
        public string ExtraCharacters { get; private set; }

        #region IRule Members

        /// <summary>
        /// Determines if the validation support the specified type.
        /// </summary>
        /// <param name="type">Property/Value type.</param>
        /// <returns>true if type is supported.</returns>
        /// <remarks>
        /// Used when validation objects are generated.
        /// </remarks>
        public bool SupportsType(Type type)
        {
            return type == typeof (string);
        }

        /// <summary>
        /// Validate a field value
        /// </summary>
        /// <param name="context">Context used to identify the model and provide the value being validated.</param>
        /// <returns>
        ///   <c>true</c> if validation was successful; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="System.NotSupportedException">Only strings are valid for this type.</exception>
        public virtual bool Validate(ValidationContext context)
        {
            if (ValueChecker.IsEmpty(context.Value))
                return true;

            if (!(context.Value is string))
                throw new NotSupportedException("Only strings are valid for this type.");

            var valueStr = (string)context.Value;
            return valueStr.All(ch => char.IsLetterOrDigit(ch) || ExtraCharacters.IndexOf(ch) != -1);
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
        public virtual string Format(string fieldName, ValidationContext context)
        {
            return ExtraCharacters != null
                       ? string.Format(Localize.A.Type<LettersAndDigitsRule>(), fieldName)
                       : string.Format(Localize.A.Type<LettersAndDigitsRule>("Class", "Extra"), fieldName);
        }

        #endregion
    }
}