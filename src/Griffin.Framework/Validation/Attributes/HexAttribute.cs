using Griffin.Framework.Validation.Rules;

#if TEST
using Xunit;
#endif

namespace Griffin.Framework.Validation.Attributes
{
    /// <summary>
    /// Checks if value is a valid hex number.
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
    ///         <term>Hex</term>
    ///         <description>'{0}' is not a valid HEX number.</description>
    ///     </item>
    /// </list>
    /// </para>
    /// </remarks>
    public class HexAttribute : ValidateAttribute
    {
        /// <summary>
        /// Create a rule based on this attribute.
        /// </summary>
        /// <returns>Created rule</returns>
        public override IRule CreateRule()
        {
            return new HexRule();
        }
    }
}