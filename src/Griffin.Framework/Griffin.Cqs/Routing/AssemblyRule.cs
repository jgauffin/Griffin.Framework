using System;
using System.Reflection;

namespace Griffin.Cqs.Routing
{
    /// <summary>
    /// Check if a CQS object is declared in the specified assembly.
    /// </summary>
    public class AssemblyRule : IRoutingRule
    {
        private readonly Assembly _assembly;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyRule" /> class.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <exception cref="System.ArgumentNullException">assembly</exception>
        public AssemblyRule(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            _assembly = assembly;
        }

        /// <summary>
        /// Match object against a rule
        /// </summary>
        /// <param name="cqsObject">The CQS object.</param>
        /// <returns>
        ///   <c>true</c> if the CQS object is declared in our assembly; otherwise <c>false</c>.
        /// </returns>
        public bool Match(object cqsObject)
        {
            return _assembly.Equals(cqsObject.GetType().Assembly);
        }
    }
}