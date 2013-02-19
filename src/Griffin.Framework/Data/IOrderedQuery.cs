using System;
using System.Linq.Expressions;

namespace Griffin.Framework.Data
{
    /// <summary>
    /// The query base interface
    /// </summary>
    /// <typeparam name="TResult">Type of result returned by this query</typeparam>
    /// <remarks>ToQueryInfo should contain <see cref="IOrderedQueryInfo"/></remarks>
    public interface IOrderedQuery<TResult> : IQuery<TResult>
    {
        /// <summary>
        /// Sort by the specified property
        /// </summary>
        /// <param name="propertyName">Property name (in the result model)</param>
        void OrderBy(string propertyName);

        /// <summary>
        /// Order result in descending fashion.
        /// </summary>
        /// <param name="propertyName">Property name (in the result model)</param>
        void OrderByDescending(string propertyName);

        /// <summary>
        /// Order result
        /// </summary>
        /// <param name="property">expression pointing on the property to use</param>
        void OrderBy(Expression<Func<TResult, object>> property);

        /// <summary>
        /// Order result in descending fashion
        /// </summary>
        /// <param name="property">expression pointing on the property to use</param>
        void OrderByDescending(Expression<Func<TResult, object>> property);
    }
}