using System;
using System.Collections.Concurrent;

namespace Griffin.Framework.Validation.Fluent
{
    /// <summary>
    /// Finds fluent validation mappings for a model by trying to create a model
    /// </summary>
    /// <seealso cref="FluentValidator"/>
    public class FluentProvider : IRulesProvider
    {
        private static ConcurrentDictionary<Type, Type> ValidatorMappings = new ConcurrentDictionary<Type, Type>();

        /// <summary>
        /// Determines if the provider contains rules for a specific model.
        /// </summary>
        /// <param name="type">Type of model to fetch validations for</param>
        /// <returns><c>true</c> if this provider can validate the model; otherwise <c>false</c>.</returns>
        public bool Contains(Type type)
        {
            var validatorType = GetValidatorType(type);
            return validatorType != null;
        }

        /// <summary>
        /// Create a new validator for the specified type
        /// </summary>
        /// <param name="type">Type to create validator for.</param>
        /// <returns>Validator if attributes was present; otherwise <c>null</c>.</returns>
        /// <exception cref="FormatException">A rule do not support the property type that is set to validate.</exception>
        public ModelValidator Create(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");

            var validatorType = GetValidatorType(type);
            if (validatorType == null)
                throw new NotSupportedException(string.Format("We do not have a validator for type {0}", type));

            var instance = Activator.CreateInstance(validatorType);
            var validator = (FluentValidator) instance;
            return validator.GetValidator();
        }

        /// <summary>
        /// Get type for the fluent validator
        /// </summary>
        /// <param name="modelType"></param>
        /// <returns></returns>
        protected Type GetValidatorType(Type modelType)
        {
            var baseType = typeof (FluentValidator<>).MakeGenericType(modelType);
            Type validatorType = null;
            if (!ValidatorMappings.TryGetValue(modelType, out validatorType))
            {
                foreach (var type1 in modelType.Assembly.GetTypes())
                {
                    if (!baseType.IsAssignableFrom(type1))
                        continue;


                    validatorType = type1;
                    break;
                }

                ValidatorMappings.TryAdd(modelType, validatorType);
            }

            return validatorType;
        }
    }
}