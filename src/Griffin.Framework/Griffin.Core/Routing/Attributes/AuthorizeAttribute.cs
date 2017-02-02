using System;

namespace Griffin.Routing.Attributes
{
    /// <summary>
    /// Set this attribute to require group authorization of the user
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class AuthorizeAttribute : Attribute
    {
        internal string[] Groups { get; set; }
        /// <summary>
        /// only works on methods
        /// true to append the class groups with the method groups
        /// </summary>
        public bool AppendClassGroups { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Griffin.Routing.Attributes.AuthorizeAttribute"/> class.
        /// </summary>
        /// <param name="groups">Groups.</param>
        public AuthorizeAttribute(params string[] groups)
        {
            Groups = groups;
        }
    }
}