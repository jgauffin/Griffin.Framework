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
        /// Initializes a new instance of the <see cref="ValidationError" /> class.
        /// </summary>
        /// <param name="name">Property/Argument that validation failed for.</param>
        /// <param name="rule">A rule that failed.</param>
        /// <param name="errorMessage"></param>
        public ValidationError(string name, IRule rule, string errorMessage)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (errorMessage == null) throw new ArgumentNullException("errorMessage");

            _name = name;
            _rule = rule;
            ErrorMessage = errorMessage;
            RuleName = rule.GetType().Name;
        }

        /// <summary>
        /// Property/Argument that validation failed for
        /// </summary>
        public string FieldName
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets name of the rule which validation failed for
        /// </summary>
        public string RuleName { get; private set; }

        /// <summary>
        /// Gets or sets complete localized error message.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}