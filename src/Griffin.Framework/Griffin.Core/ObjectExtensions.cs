using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Griffin
{
    /// <summary>
    /// Extension methods for object.
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        ///     Turn anonymous object to dictionary
        /// </summary>
        /// <param name="data">Anonymous object</param>
        /// <returns>Dictionary</returns>
        public static Dictionary<string, object> ToDictionary(this object data)
        {
            if (data is string || data.GetType().IsPrimitive)
                return new Dictionary<string, object>();

            return (from property in data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                where property.CanRead
                select property)
                .ToDictionary(property => property.Name, property => property.GetValue(data, null));
        }
    }
}