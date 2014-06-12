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
        public RequestInvokedEventArgs(IContainerScope scope, IRequest request)
        {
            if (scope == null) throw new ArgumentNullException("scope");
            if (request == null) throw new ArgumentNullException("request");
            Scope = scope;
            Request = request;
        }

        public IContainerScope Scope { get; private set; }
        public IRequest Request { get; private set; }
    }
}