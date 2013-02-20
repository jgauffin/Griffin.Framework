using System;
using System.Linq;
using Griffin.Framework.Validation.Attributes;

namespace Griffin.Framework.Validation
{
    /// <summary>
    /// Provides validation rules by scanning model for validation attributes.
    /// </summary>
    public class AttributeProvider : IRulesProvider
    {
        #region IRulesProvider Members

        /// <summary>
        /// Determines if the provider contains rules for a specific model.
        /// </summary>
        /// <param name="type">Type of model to fetch validations for</param>
        /// <returns><c>true</c> if this provider can validate the model; otherwise <c>false</c>.</returns>
        public bool Contains(Type type)
        {
            var attributeType = typeof (ValidateAttribute);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (!propertyInfo.CanWrite || !propertyInfo.CanRead)
                    continue;

                var attrs = propertyInfo.GetCustomAttributes(true);
                if (attrs.Any(attributeType.IsInstanceOfType))
                    return true;

            }

            return false;
        }

        /// <summary>
        /// Create a new validator for the specified type
        /// </summary>
        /// <param name="type">Type to create validator for.</param>
        /// <returns>Validator if attributes was present; otherwise <c>null</c>.</returns>
        /// <exception cref="FormatException">A rule do not support the property type that is set to validate.</exception>
        public ModelValidator Create(Type type)
        {
            if (!Contains(type))
                return null;

            var validator = new ModelValidator(type);
            var attributeType = typeof (ValidateAttribute);


            foreach (var propertyInfo in type.GetProperties())
            {
                if (!propertyInfo.CanWrite || !propertyInfo.CanRead)
                    continue;

                var attrs = propertyInfo.GetCustomAttributes(true);
                foreach (var attr in attrs)
                {
                    if (!attributeType.IsInstanceOfType(attr))
                        continue;

                    var attribute = (ValidateAttribute) attr;

                    var rule = attribute.CreateRule();
                    if (!rule.SupportsType(propertyInfo.PropertyType))
                        throw new FormatException(rule.GetType().Name + " do not support property type used by " +
                                                  type.Name + "." + propertyInfo.PropertyType.Name + ".");

                    validator.Add(propertyInfo.Name, rule);
                }
            }

            return validator;
        }

        #endregion
    }
}