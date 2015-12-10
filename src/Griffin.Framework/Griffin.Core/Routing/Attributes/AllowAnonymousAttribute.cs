using System;

namespace Griffin.Routing.Attributes
{
    /// <summary>
    /// Set this attribute to an controller or method to allow anonymous calling.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class AllowAnonymousAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Griffin.Routing.Attributes.AllowAnonymousAttribute"/> class.
        /// </summary>
        public AllowAnonymousAttribute()
        {
        }
    }
}