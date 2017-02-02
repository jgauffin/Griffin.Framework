using System;

namespace Griffin.Routing.Attributes
{
    /// <summary>
    /// Set this attribute to require authentication of the caller.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class AuthenticateAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Griffin.Routing.Attributes.AuthenticateAttribute"/> class.
        /// </summary>
        public AuthenticateAttribute()
        {
        }
    }
}