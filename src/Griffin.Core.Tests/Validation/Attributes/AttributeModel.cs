using Griffin.Framework.Validation.Attributes;

namespace Griffin.Framework.Tests.Validation
{
    public class AttributeModel
    {
        /// <summary>
        /// Gets or sets first name.
        /// </summary>
        [Required]
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets last name.
        /// </summary>
        [Required]
        [LettersAndDigits(LettersAndDigitsAttribute.SpaceCommaDot)]
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets age.
        /// </summary>
        [Min(10)]
        public int Age { get; set; }
    }
}
