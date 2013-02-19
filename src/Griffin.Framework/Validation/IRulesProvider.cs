using System;

namespace Griffin.Framework.Validation
{
    /// <summary>
    /// Provides validations for a model.
    /// </summary>
    public interface IRulesProvider
    {
        /// <summary>
        /// Determines if the provider contains rules for a specific model.
        /// </summary>
        /// <param name="type">Type of model to fetch validations for</param>
        /// <returns><c>true</c> if this provider can validate the model; otherwise <c>false</c>.</returns>
        bool Contains(Type type);

        /// <summary>
        /// Create a new validator for the specified type
        /// </summary>
        /// <param name="type">Type to create validator for.</param>
        /// <returns>Validator if attributes was present; otherwise <c>null</c>.</returns>
        /// <exception cref="FormatException">A rule do not support the property type that is set to validate.</exception>
        ModelValidator Create(Type type);
    }
}