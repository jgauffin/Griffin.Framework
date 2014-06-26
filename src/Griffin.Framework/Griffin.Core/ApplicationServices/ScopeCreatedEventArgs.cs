using System;
using Griffin.Container;

namespace Griffin.ApplicationServices
{
    /// <summary>
    ///     Arg for  <see cref="BackgroundJobManager.ScopeCreated" />
    /// </summary>
    public class ScopeCreatedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScopeCreatedEventArgs"/> class.
        /// </summary>
        /// <param name="scope">That that will be used to resolve job.</param>
        /// <exception cref="System.ArgumentNullException">scope</exception>
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