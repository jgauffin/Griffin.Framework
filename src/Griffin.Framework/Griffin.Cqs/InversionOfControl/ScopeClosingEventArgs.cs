using System;
using Griffin.Container;

namespace Griffin.Cqs.InversionOfControl
{
    /// <summary>
    ///     Event arguments to represent when an IoC scope is about to be closed.
    /// </summary>
    public class ScopeClosingEventArgs
    {
        /// <summary>
        ///     Created a new instance of <see cref="ScopeCreatedEventArgs" />.
        /// </summary>
        /// <param name="scope">created scope</param>
        public ScopeClosingEventArgs(IContainerScope scope)
        {
            if (scope == null) throw new ArgumentNullException(nameof(scope));
            Scope = scope;
        }

        /// <summary>
        ///     Scopes about to be closed.
        /// </summary>
        public IContainerScope Scope { get; }

        /// <summary>
        ///     The CQS handlers executed successfully.
        /// </summary>
        public bool HandlersWasSuccessful { get; set; }
    }
}