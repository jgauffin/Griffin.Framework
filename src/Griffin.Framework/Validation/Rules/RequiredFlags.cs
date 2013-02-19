using System;

namespace Griffin.Framework.Validation.Rules
{
    /// <summary>
    /// Flags used in the required rule
    /// </summary>
    [Flags]
    public enum RequiredFlags
    {
        /// <summary>
        /// Accept an empty string as a valid value.
        /// </summary>
        AcceptEmptyString
    }
}