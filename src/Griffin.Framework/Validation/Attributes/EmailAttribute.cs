using Griffin.Framework.Validation.Rules;

#if TEST
using Xunit;
#endif

namespace Griffin.Framework.Validation.Attributes
{
    /// <summary>
    /// Validates that a string property is an correct email.
    /// </summary>
    /// <para>
    /// Language file items:
    /// <list type="table">
    ///     <listheader>
    ///         <term>ItemName</term>
    ///         <description>Language text</description>
    ///     </listheader>
    ///     <item>
    ///         <term>Email</term>
    ///         <description>'{0}' must contain a valid email address.</description>
    ///     </item>
    /// </list>
    /// </para>
    public class EmailAttribute : ValidateAttribute
    {
        /// <summary>
        /// Create a rule based on this attribute.
        /// </summary>
        /// <returns>Created rule</returns>
        public override IRule CreateRule()
        {
            return new EmailRule();
        }
    }
}