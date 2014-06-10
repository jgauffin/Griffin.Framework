using System;
using System.Linq;

namespace Griffin
{
    /// <summary>
    /// Extension methods for <c>Type</c>.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Check if generic types matches
        /// </summary>
        /// <param name="serviceType">Service/interface</param>
        /// <param name="concreteType">Concrete/class</param>
        /// <returns><c>true</c> if the concrete implements the service; otherwise <c>false</c></returns>
        public static bool IsAssignableFromGeneric(this Type serviceType, Type concreteType)
        {
            var interfaceTypes = concreteType.GetInterfaces();
            if (interfaceTypes.Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == serviceType))
                return true;

            var baseType = concreteType.BaseType;
            if (baseType == null)
                return false;

            return baseType.IsGenericType &&
                baseType.GetGenericTypeDefinition() == serviceType ||
                IsAssignableFromGeneric(serviceType, baseType);
        }

        /// <summary>
        /// Checks if the specified type is a type which should not be traversed when building an object hiararchy.
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns><c>true</c> if it's a simple type; otherwise <c>false</c>.</returns>
        /// <example>
        /// <code>
        /// public string Build(object instance)
        /// {
        ///     var sb = new StringBuilder();
        ///     Build(instance, "", sb);
        ///     return sb.ToString();
        /// }
        /// 
        /// protected void Build(object instance, string prefix, StringBuilder result)
        /// {
        ///     foreach (var propInfo in instance.GetType().GetProperties())
        ///     {
        ///         if (instance.GetType().IsSimpleType())
        ///         {
        ///             var value = propInfo.GetValue(instance, null);
        ///             result.AppendLine(prefix + propInfo.Name + ": " + value);
        ///         }
        ///         else
        ///         {
        ///             var newPrefix = prefix == "" ? propInfo.Name : prefix + ".";
        ///             Build(newPrefix, 
        /// }
        /// 
        /// while (!type.IsSimpleType())
        /// {
        /// }
        /// 
        /// </code>
        /// </example>
        public static bool IsSimpleType(this Type type)
        {

            return type.IsPrimitive
                   || type == typeof (Decimal)
                   || type == typeof (String)
                   || type == typeof (DateTime)
                   || type == typeof (Guid)
                   || type == typeof (DateTimeOffset)
                   || type == typeof(TimeSpan);
        }

        /// <summary>
        /// Gets the assembly qualified name, but without public token and version.
        /// </summary>
        /// <param name="type">Type to get name from</param>
        /// <returns>for instance <c>"MyApp.Contracts.User, MyApp"</c></returns>
        public static string GetSimplifiedAssemblyQualifiedName(this Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            return string.Format("{0}, {1}", type.FullName, type.Assembly.GetName().Name);
        }
    }
}
