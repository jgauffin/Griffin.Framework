using System;
using System.Collections.Generic;
using System.Linq;
using Griffin.Framework.Text;

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


        internal static void Translate(ValidationErrors errors)
        {
            foreach (var error in errors)
            {
                if (!string.IsNullOrEmpty(error.ErrorMessage) && !string.IsNullOrEmpty(error.LocalizedFieldName))
                    continue; // do not translated fixed fields.

                var fieldName = Localize.A.Property(error.FieldName);
                error.ErrorMessage = error.Rule.Format(fieldName, ruleLanguage);
                error.LocalizedFieldName = fieldName;
            }
        }

        /// <summary>
        /// Translate errors into current language
        /// </summary>
        /// <param name="errors">Errors to translate</param>
        /// <param name="modelType">Model that have been validated</param>
        public static void Translate(ValidationErrors errors, Type modelType)
        {
            // check if translation exists, else load prompts from attributes.
            var ruleLanguage = new RuleLanguage(_language);

            foreach (var error in errors)
            {
                if (!string.IsNullOrEmpty(error.ErrorMessage) && !string.IsNullOrEmpty(error.LocalizedFieldName))
                    continue; // do not translate fixed fields.

                ruleLanguage.Rule = error.Rule;
                var fieldName = _language[modelType.Name, error.FieldName];
                error.ErrorMessage = error.Rule.Format(fieldName, ruleLanguage);
                error.LocalizedFieldName = fieldName;
            }
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
            var errors = validator.Validate(model);
            Translate(errors, typeToValidate);
            return errors;
        }
    }
}