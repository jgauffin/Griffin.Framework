using System;

namespace Griffin.Routing.Attributes
{
    /// <summary>
    /// Set this attribute to create an HttpRoute
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class RouteAttribute : Attribute
    {
        internal string RouteMask { get; set; }
        internal bool IgnorePrefix { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Griffin.Routing.Attributes.RouteAttribute"/> class.
        /// </summary>
        /// <param name="routeMask">Route mask.</param>
        /// <param name="ignorePrefix">If set to <c>true</c> ignore the route prefix.</param>
        public RouteAttribute(string routeMask = "/", bool ignorePrefix = false)
        {
            RouteMask = routeMask;
            IgnorePrefix = ignorePrefix;
        }
    }
}