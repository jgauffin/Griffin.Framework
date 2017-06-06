using System;
using System.Linq;
using System.Reflection;

namespace Griffin
{
    /// <summary>
    ///     Extension methods for <c>Type</c>.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        ///     Get assembly qualified name, but without the version and public token.
        /// </summary>
        /// <param name="type">Type to get name for</param>
        /// <returns>Simple assembly qualified name. Example: <code>"MyApp.Contracts.User, MyApp.Contracts"</code></returns>
        public static string GetSimpleAssemblyQualifiedName(this Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (type.GetTypeInfo().Assembly.IsDynamic)
                throw new InvalidOperationException("Can't use dynamic assemblies.");

            return type.FullName + ", " + type.GetTypeInfo().Assembly.GetName().Name;
        }

        /// <summary>
        ///     Get type name as we define it in code.
        /// </summary>
        /// <param name="t">The type to get a name for.</param>
        /// <returns>String representation</returns>
        public static string GetFriendlyTypeName(this Type t)
        {
            if (!t.GetTypeInfo().IsGenericType)
                return t.Name;
            var genericTypeName = t.GetGenericTypeDefinition().Name;
            genericTypeName = genericTypeName.Substring(0, genericTypeName.IndexOf('`'));
            var genericArgs = string.Join(",", t.GetGenericArguments().Select(GetFriendlyTypeName).ToArray());
            return string.Format("{0}<{1}>", genericTypeName, genericArgs);
        }

        /// <summary>
        ///     Check if generic types matches
        /// </summary>
        /// <param name="serviceType">Service/interface</param>
        /// <param name="concreteType">Concrete/class</param>
        /// <returns><c>true</c> if the concrete implements the service; otherwise <c>false</c></returns>
        public static bool IsAssignableFromGeneric(this Type serviceType, Type concreteType)
        {
            var interfaceTypes = concreteType.GetTypeInfo().GetInterfaces();
            if (interfaceTypes.Any(it => it.GetTypeInfo().IsGenericType && it.GetGenericTypeDefinition() == serviceType))
                return true;

            var baseType = concreteType.GetTypeInfo().BaseType;
            if (baseType == null)
                return false;

            return baseType.GetTypeInfo().IsGenericType &&
                   baseType.GetGenericTypeDefinition() == serviceType ||
                   IsAssignableFromGeneric(serviceType, baseType);
        }

        /// <summary>
        ///     Checks if the specified type is a type which should not be traversed when building an object hiararchy.
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns><c>true</c> if it's a simple type; otherwise <c>false</c>.</returns>
        /// <example>
        ///     <code>
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
            return type.GetTypeInfo().IsPrimitive
                   || type == typeof (decimal)
                   || type == typeof (string)
                   || type == typeof (DateTime)
                   || type == typeof (Guid)
                   || type == typeof (DateTimeOffset)
                   || type == typeof (TimeSpan);
        }
    }
}