using System;

namespace Griffin.Routing.Attributes
{
    /// <summary>
    /// Route prefix attribute.
    /// </summary>
    /// <remarks>
    /// Set this attribute to an controller to define an basic prefix for all routes
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class RoutePrefixAttribute : Attribute
    {
        internal string Prefix { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Griffin.Routing.Attributes.RoutePrefixAttribute"/> class.
        /// </summary>
        /// <param name="prefix">Prefix.</param>
        public RoutePrefixAttribute(string prefix)
        {
            Prefix = prefix;
        }
    }
}