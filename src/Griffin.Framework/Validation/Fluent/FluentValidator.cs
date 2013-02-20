using System;
using System.Collections.Generic;
using System.Reflection;

namespace Griffin.Framework.Validation.Fluent
{
    /// <summary>
    /// Validates a model.
    /// </summary>
    /// <remarks>this class should only be used for custom validations. You must manually add these validators
    /// to the <see cref="FluentProvider"/>.
    /// <para>All implementations must be thread safe.</para>
    /// </remarks>
    /// <seealso cref="FluentValidator{T}"/>
    public class FluentValidator
    {
        private readonly Dictionary<string, Property> _properties = new Dictionary<string, Property>();
        private readonly Type _type;
        private readonly ModelValidator _validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentValidator" /> class.
        /// </summary>
        /// <param name="type">The type to validate.</param>
        /// <exception cref="System.ArgumentNullException">type</exception>
        public FluentValidator(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            _type = type;
            _validator = new ModelValidator(type);
        }

        internal ModelValidator GetValidator()
        {
            return _validator;
        }

        /// <summary>
        /// Get fluent accessor for the validation
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        protected Property Property(string name)
        {
            Property property;
            if (_properties.TryGetValue(name, out property))
                return property;

            var pi = _type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (pi == null || !pi.CanRead || !pi.CanWrite)
                throw new InvalidOperationException(
                    string.Format("{0} do not have a read/writable property named '{1}'.", _type.FullName, name));

            property = new Property(name, _validator);
            _properties.Add(name, property);
            return property;
        }
    }
}