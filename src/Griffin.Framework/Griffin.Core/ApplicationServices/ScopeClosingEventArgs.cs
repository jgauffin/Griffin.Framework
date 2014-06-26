using System;
using Griffin.Container;

namespace Griffin.ApplicationServices
{
    /// <summary>
    ///     Arg for <see cref="BackgroundJobManager.ScopeClosing" />
    /// </summary>
    public class ScopeClosingEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScopeClosingEventArgs"/> class.
        /// </summary>
        /// <param name="scope">scope that is being closed.</param>
        /// <param name="successful">job was executed successfully.</param>
        /// <exception cref="System.ArgumentNullException">scope</exception>
        public ScopeClosingEventArgs(IContainerScope scope, bool successful)
        {
            if (scope == null) throw new ArgumentNullException("scope");
            Scope = scope;
            Successful = successful;
        }

        /// <summary>
        ///     Scope that this event is for.
        /// </summary>
        public IContainerScope Scope { get; private set; }

        /// <summary>
        /// Job was executed successfully
        /// </summary>
        public bool Successful { get; set; }

        /// <summary>
        /// Exception if <see cref="Successful"/> is false.
        /// </summary>
        public Exception Exception { get; set; }
    }
}