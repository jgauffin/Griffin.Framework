using System;

namespace Griffin.Framework.Validation.Rules
{
    /// <summary>
    /// context used by the validation rules
    /// </summary>
    public class ValidationContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationContext"/> class.
        /// </summary>
        /// <param name="model">The model being validated.</param>
        /// <param name="propertyName">Property currently being validated.</param>
        /// <param name="value">Value of the property.</param>
        public ValidationContext(object model, string propertyName, object value)
        {
            if (model == null) throw new ArgumentNullException("model");
            if (propertyName == null) throw new ArgumentNullException("propertyName");

            Value = value;
            PropertyName = propertyName;
            Model = model;
        }

        /// <summary>
        /// Gets model that the property is in
        /// </summary>
        public object Model { get; private set; }

        /// <summary>
        /// Gets property name
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Gets value being validated.
        /// </summary>
        public object Value { get; private set; }
    }
}