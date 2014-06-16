using System;
using Griffin.Container;

namespace Griffin.ApplicationServices
{
    /// <summary>
    ///     Arg for  <see cref="BackgroundJobManager.ScopeCreated" />
    /// </summary>
    public class ScopeCreatedEventArgs : EventArgs
    {
        public ScopeCreatedEventArgs(IContainerScope scope)
        {
            if (scope == null) throw new ArgumentNullException("scope");
            Scope = scope;
        }

        /// <summary>
        ///     Scope that this event is for.
        /// </summary>
        public IContainerScope Scope { get; private set; }
    }
}