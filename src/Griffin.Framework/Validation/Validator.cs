using System;
using System.Collections.Generic;
using System.Linq;

namespace Griffin.Framework.Validation
{
    public class Validator
    {
        /// <summary>
        /// Model name used to retreive translations for rule prompts.
        /// </summary>
        public const string ModelName = "ShureValidation";

        private static readonly List<IRulesProvider> _providers = new List<IRulesProvider>();

        /// <summary>
        /// Add a rules provider
        /// </summary>
        /// <param name="provider">Provider to add</param>
        /// <remarks>
        /// Providers are used to load model rules from Attributes, Fluent classes or any other source.
        /// </remarks>
        public static void Add(IRulesProvider provider)
        {
            _providers.Add(provider);
        }


        private static ModelValidator GetValidator(Type type)
        {
            return _providers.Select(provider => provider.Create(type)).FirstOrDefault(validator => validator != null);
        }

        /// <summary>
        /// Register a new rule provider
        /// </summary>
        /// <param name="provider"></param>
        public void Register(IRulesProvider provider)
        {
            _providers.Add(provider);
        }

        /// <summary>
        /// Validate a model.
        /// </summary>
        /// <typeparam name="T">Type to use during validation</typeparam>
        /// <param name="model">Model to validate</param>
        /// <remarks>List of errors, or an empty list if no errors was found.</remarks>
        public static ValidationErrors Validate<T>(T model)
        {
            return Validate(model, typeof (T));
        }

        /// <summary>
        /// Validate a model.
        /// </summary>
        /// <param name="model">Model to validate</param>
        /// <remarks>List of errors, or an empty list if no errors was found.</remarks>
        public static ValidationErrors Validate(object model)
        {
            return Validate(model, model.GetType());
        }

        /// <summary>
        /// Validate a model.
        /// </summary>
        /// <param name="model">Model to validate</param>
        /// <param name="typeToValidate">Type to use during validation</param>
        /// <remarks>List of errors, or an empty list if no errors was found.</remarks>
        /// <exception cref="InvalidOperationException">Validator was not found for the specified model type.</exception>
        public static ValidationErrors Validate(object model, Type typeToValidate)
        {
            var validator = GetValidator(typeToValidate);
            if (validator == null)
                throw new InvalidOperationException("Failed to find validator for type '" + typeToValidate.FullName +
                                                    "'.");
            return validator.Validate(model);
        }
    }
}