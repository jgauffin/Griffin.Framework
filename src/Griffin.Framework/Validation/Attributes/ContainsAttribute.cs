using System;
using Griffin.Framework.Validation.Rules;

namespace Griffin.Framework.Validation.Attributes
{
    /// <summary>
    /// Must only contain the specified letters (does not fail if the string is null or empty)
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
    ///         <term>ContainsOnly</term>
    ///         <description>'{0}' may only contain one ore more of the following letters: {1}.</description>
    ///     </item>
    /// </list>
    /// </para>
    /// </remarks>
    public class ContainsOnlyAttribute : ValidateAttribute
    {
        private readonly string _letters = string.Empty;


        /// <summary>
        /// Initializes a new instance of the <see cref="ContainsOnlyAttribute"/> class.
        /// </summary>
        /// <param name="allowedLetters">The allowed letters</param>
        /// <exception cref="ArgumentNullException">Argument is <c>null</c>.</exception>
        public ContainsOnlyAttribute(string allowedLetters)
        {
            if (string.IsNullOrEmpty(allowedLetters))
                throw new ArgumentNullException("allowedLetters");
            _letters = allowedLetters;
        }


        /// <summary>
        /// Create a rule based on this attribute.
        /// </summary>
        /// <returns>Created rule</returns>
        public override IRule CreateRule()
        {
            return new ContainsOnlyRule(_letters);
        }
    }
}