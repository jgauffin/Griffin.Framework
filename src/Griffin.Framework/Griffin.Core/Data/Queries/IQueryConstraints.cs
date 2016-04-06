using System;
using System.Linq.Expressions;

namespace Griffin.Data.Queries
{
    /// <summary>
    /// Typed paging
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    public interface IQueryConstraints<T> where T : class
    {
        /// <summary>
        /// Gets number of items per page (when paging is used)
        /// </summary>
        int PageSize { get; }

        /// <summary>
        /// Gets page number (one based index)
        /// </summary>
        int PageNumber { get; }

        /// <summary>
        /// Gets the kind of sort order
        /// </summary>
        SortOrder SortOrder { get; }

        /// <summary>
        /// Gets property name for the property to sort by.
        /// </summary>
        string SortPropertyName { get; }

        /// <summary>
        /// Use paging
        /// </summary>
        /// <param name="pageNumber">Page to get (one based index).</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <returns>Current instance</returns>
        IQueryConstraints<T> Page(int pageNumber, int pageSize);

        /// <summary>
        /// Sort ascending by a property
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Current instance</returns>
        IQueryConstraints<T> SortBy(string propertyName);

        /// <summary>
        /// Sort descending by a property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Current instance</returns>
        IQueryConstraints<T> SortByDescending(string propertyName);

        /// <summary>
        /// Property to sort by (ascending)
        /// </summary>
        /// <param name="property">The property.</param>
        IQueryConstraints<T> SortBy(Expression<Func<T, object>> property);

        /// <summary>
        /// Property to sort by (descending)
        /// </summary>
        /// <param name="property">The property</param>
        IQueryConstraints<T> SortByDescending(Expression<Func<T, object>> property);
    }
}