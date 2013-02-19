using System;
using System.Collections.Generic;

namespace Griffin.Framework.Validation
{
    /// <summary>
    /// Checks whether a column is empty or not.
    /// </summary>
    public static class ValueChecker
    {
        private static readonly Dictionary<Type, IValueCheck> _checkers = new Dictionary<Type, IValueCheck>();

        /// <summary>
        /// Add a class that determines if a certain type is empty or not.
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <param name="check">Object checking values.</param>
        /// <exception cref="InvalidOperationException">Type have already been mapped.</exception>
        public static void AddChecker(Type type, IValueCheck check)
        {
            if (_checkers.ContainsKey(type))
                throw new InvalidOperationException("Type have already been mapped.");

            _checkers.Add(type, check);
        }

        /// <summary>
        /// Determines if an object is empty.
        /// </summary>
        /// <param name="value">object to check</param>
        /// <returns>true if empty; otherwise false.</returns>
        public static bool IsEmpty(object value)
        {
            if (value == null)
                return true;

            if (_checkers.ContainsKey(value.GetType()))
                return _checkers[value.GetType()].IsEmpty(value);

            if (value is double || value is float)
                return value.Equals(0.0);

            if (value is int || value is short || value is long)
                return value.Equals(0);

            if (value is string)
                return string.IsNullOrEmpty((string) value);

            if (value is DateTime)
                return value.Equals(default(DateTime));


            return false;
        }
    }

    /// <summary>
    /// Implementation checking if a value is empty.
    /// </summary>
    public interface IValueCheck
    {
        /// <summary>
        /// Determines if an object is empty.
        /// </summary>
        /// <param name="value">object to check</param>
        /// <returns>true if empty; otherwise false.</returns>
        bool IsEmpty(object value);
    }
}