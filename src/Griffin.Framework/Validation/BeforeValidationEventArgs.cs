using System;
using Griffin.Framework.Validation.Attributes;

namespace Griffin.Framework.Validation
{
    /// <summary>
    /// Used to be able to provide context to validation classes during validation.
    /// </summary>
    public class BeforeValidationEventArgs : EventArgs
    {
        private readonly ValidateAttribute _attribute;

        /// <summary>
        /// Initializes a new instance of the <see cref="BeforeValidationEventArgs"/> class.
        /// </summary>
        /// <param name="attribute">Attribute that is going to be validated.</param>
        public BeforeValidationEventArgs(ValidateAttribute attribute)
        {
            _attribute = attribute;
        }

        /// <summary>
        /// Attribute about to be used for validation.
        /// </summary>
        public ValidateAttribute Attribute
        {
            get { return _attribute; }
        }

        /// <summary>
        /// Context that should be passed to the validation class.
        /// </summary>
        public object Context { get; set; }
    }
}