using Griffin.Framework.Text;
using Griffin.Framework.Validation.Rules;

namespace Griffin.Framework.Validation.Attributes
{
    /// <summary>
    /// Checks whether a field have been specified or not.
    /// </summary>
    /// <seealso cref="RequiredRule"/>
    [Localize("en-us", "The field '{0}' is required.")]
    public class RequiredAttribute : ValidateAttribute
    {
        private readonly RequiredFlags _flags;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredAttribute"/> class.
        /// </summary>
        public RequiredAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredAttribute"/> class.
        /// </summary>
        /// <param name="flags">The flags.</param>
        public RequiredAttribute(RequiredFlags flags)
        {
            _flags = flags;
        }

        /// <summary>
        /// Create a rule based on this attribute.
        /// </summary>
        /// <returns>Created rule</returns>
        public override IRule CreateRule()
        {
            return new RequiredRule(_flags);
        }
    }
}