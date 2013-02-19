using System;
using Griffin.Framework.Validation.Rules;

namespace Griffin.Framework.Validation
{
    /// <summary>
    /// An validation error.
    /// </summary>
    [Serializable]
    public class ValidationError
    {
        private readonly string _name;
        private readonly IRule _rule;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Property/Argument that validation failed for.</param>
        /// <param name="rule">A rule that failed.</param>
        public ValidationError(string name, IRule rule)
        {
            _name = name;
            _rule = rule;
        }

        /// <summary>
        /// Property/Argument that validation failed for
        /// </summary>
        public string FieldName
        {
            get { return _name; }
        }

        /// <summary>
        /// Kind of validation that failed
        /// </summary>
        public IRule Rule
        {
            get { return _rule; }
        }

        /// <summary>
        /// Gets or sets localized field name.
        /// </summary>
        public string LocalizedFieldName { get; internal set; }

        /// <summary>
        /// Gets or sets complete localized error message.
        /// </summary>
        public string ErrorMessage { get; internal set; }
    }
}