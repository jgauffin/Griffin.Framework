using System;
using System.Collections.Generic;

namespace Griffin.Data.Queries
{
    /// <summary>
    ///     Search result implementation
    /// </summary>
    /// <typeparam name="T">Model type (i.e. denormalized row)</typeparam>
    public class QueryResult<T> : IQueryResult<T> where T : class
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="QueryResult{T}" /> class.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="totalCount">The total count (if paging is used, otherwise <c>0</c>).</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public QueryResult(IEnumerable<T> items, int totalCount)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (totalCount < 0 || totalCount == int.MaxValue)
                throw new ArgumentOutOfRangeException("totalCount", totalCount, "Incorrect value.");

            Items = items;
            TotalCount = totalCount;
        }

        /// <summary>
        ///     Gets all matching items
        /// </summary>
        public IEnumerable<T> Items { get; private set; }

        /// <summary>
        ///     Gets total number of items (useful when paging is used, otherwise 0)
        /// </summary>
        public int TotalCount { get; private set; }
    }
}