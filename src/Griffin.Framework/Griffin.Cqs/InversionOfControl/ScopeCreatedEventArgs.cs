using System;
using Griffin.Container;

namespace Griffin.Cqs.InversionOfControl
{
    /// <summary>
    ///     Event arguments to represent when a new IoC scope have been created by some part of the library.
    /// </summary>
    public class ScopeCreatedEventArgs
    {
        /// <summary>
        ///     Created a new instance of <see cref="ScopeCreatedEventArgs" />.
        /// </summary>
        /// <param name="scope">created scope</param>
        public ScopeCreatedEventArgs(IContainerScope scope)
        {
            if (scope == null) throw new ArgumentNullException(nameof(scope));
            Scope = scope;
        }

        /// <summary>
        ///     Newly created scope.
        /// </summary>
        public IContainerScope Scope { get; }
    }
}