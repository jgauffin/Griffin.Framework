using System;
using DotNetCqs;
using Griffin.Container;

namespace Griffin.Cqs.InversionOfControl
{
    /// <summary>
    ///     A request have been successfully invoked
    /// </summary>
    public class RequestInvokedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestInvokedEventArgs"/> class.
        /// </summary>
        /// <param name="scope">scope that was used to resolve the handler.</param>
        /// <param name="request">Request that was processed.</param>
        /// <exception cref="System.ArgumentNullException">
        /// scope
        /// or
        /// request
        /// </exception>
        public RequestInvokedEventArgs(IContainerScope scope, IRequest request)
        {
            if (scope == null) throw new ArgumentNullException("scope");
            if (request == null) throw new ArgumentNullException("request");
            Scope = scope;
            Request = request;
        }

        /// <summary>
        /// scope that was used to resolve the handler
        /// </summary>
        public IContainerScope Scope { get; private set; }

        /// <summary>
        /// Request that was processed
        /// </summary>
        public IRequest Request { get; private set; }
    }
}