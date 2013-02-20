using System;
using System.Collections;
using System.Collections.Generic;
using Griffin.Framework.Validation.Rules;

namespace Griffin.Framework.Validation
{
    /// <summary>
    /// Collection of validation errors.
    /// </summary>
    [Serializable]
    public class ValidationErrors : IEnumerable<ValidationError>
    {
        private readonly List<ValidationError> _errors = new List<ValidationError>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationErrors" /> class.
        /// </summary>
        /// <param name="modelType">Model that the errors are for.</param>
        /// <exception cref="System.ArgumentNullException">modelType</exception>
        public ValidationErrors(Type modelType)
        {
            if (modelType == null) throw new ArgumentNullException("modelType");
            ModelType = modelType;
        }

        /// <summary>
        /// Gets model that the errors are for.
        /// </summary>
        public Type ModelType { get; private set; }

        /// <summary>
        /// Gets number of errors.
        /// </summary>
        public int Count
        {
            get { return _errors.Count; }
        }

        #region IEnumerable<ValidationError> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<ValidationError> GetEnumerator()
        {
            return _errors.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Add a bunch of errors
        /// </summary>
        /// <param name="errors">Errors to add</param>
        public void AddRange(ValidationErrors errors)
        {
            _errors.AddRange(errors._errors);
        }

        /// <summary>
        /// Add a custom error message
        /// </summary>
        /// <param name="propertyName">Property name (not translated)</param>
        /// <param name="rule">Rule that didn't validate.</param>
        /// <param name="errorMessage">The error message.</param>
        public void Add(string propertyName, IRule rule, string errorMessage)
        {
            _errors.Add(new ValidationError(propertyName, rule, errorMessage));
        }

        /// <summary>
        /// Add a custom error message
        /// </summary>
        /// <param name="propertyName">Property name (not translated)</param>
        /// <param name="errorText">Error text, including {0} for translated property name.</param>
        /// <remarks>
        /// </remarks>
        /// <example>
        /// <code>
        /// errors.Add("Animal", "'{0}' är inte en get.");
        /// </code>
        /// </example>
        public void Add(string propertyName, string errorText)
        {
            _errors.Add(new ValidationError(propertyName, null, errorText));
        }

        /// <summary>
        /// Remove all errors.
        /// </summary>
        public void Clear()
        {
            _errors.Clear();
        }
    }
}