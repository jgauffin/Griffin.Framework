using System;
using Griffin.Framework.Validation.Rules;

namespace Griffin.Framework.Validation.Attributes
{
    /// <summary>
    /// Base class for all validation attributes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each validation class should document what their language entries should say, and their names.
    /// This should be done in the remarks section using a table list, example:
    /// <list type="table">
    ///     <listheader>
    ///         <term>ItemName</term>
    ///         <description>Language text</description>
    ///     </listheader>
    ///     <item>
    ///         <term>ValidateLetters</term>
    ///         <description>'{0}' may only contain letters.</description>
    ///     </item>
    ///     <item>
    ///         <term>ValidateLettersExtra</term>
    ///         <description>'{0}' may only contain letters and one of the following characters: {1}</description>
    ///     </item>
    /// </list>
    /// It's recommended that the item names (or terms in the table above) equals the validation class name.
    /// i.e. ValidateEmailAttributes adds a language item called "Email".
    /// </para>
    /// </remarks>
    public abstract class ValidateAttribute : Attribute
    {
        /// <summary>
        /// Create a rule based on this attribute.
        /// </summary>
        /// <returns>Created rule</returns>
        public abstract IRule CreateRule();
    }
}