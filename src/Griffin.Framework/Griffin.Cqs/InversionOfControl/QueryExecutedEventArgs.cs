using System;
using DotNetCqs;
using Griffin.Container;

namespace Griffin.Cqs.InversionOfControl
{
    /// <summary>
    /// A query have been successfully invoked
    /// </summary>
    public class QueryExecutedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryExecutedEventArgs"/> class.
        /// </summary>
        /// <param name="scope">Scope used to resolve the handler.</param>
        /// <param name="query">Query to execute.</param>
        /// <param name="handler">Query handler that executed the query (implements <see cref="IQueryHandler{TQuery,TResult}"/>).</param>
        /// <exception cref="System.ArgumentNullException">
        /// scope
        /// or
        /// query
        /// </exception>
        public QueryExecutedEventArgs(IContainerScope scope, object query, object handler)
        {
            if (scope == null) throw new ArgumentNullException("scope");
            if (query == null) throw new ArgumentNullException("query");

            Scope = scope;
            Query = query;
            Handler = handler;
        }

        /// <summary>
        /// Scope used to resolve the handler
        /// </summary>
        public IContainerScope Scope { get; private set; }

        /// <summary>
        /// Query to execute
        /// </summary>
        public object Query { get; private set; }

        /// <summary>
        /// Query handler that executed the query (implements <see cref="IQueryHandler{TQuery,TResult}"/>).
        /// </summary>
        public object Handler { get; set; }
    }
}