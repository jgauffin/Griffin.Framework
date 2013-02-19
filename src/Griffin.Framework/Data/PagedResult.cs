using System;
using System.Collections.Generic;

namespace Griffin.Framework.Data
{
    /// <summary>
    /// We have received a paged result
    /// </summary>
    /// <typeparam name="T">Type of entity</typeparam>
    public class PagedResult<T> where T  : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PagedResult{T}" /> class.
        /// </summary>
        /// <param name="items">Gets items in this page.</param>
        /// <param name="totalCount">The total count.</param>
        public PagedResult(IEnumerable<T> items, int totalCount)
        {
            if (items == null) throw new ArgumentNullException("items");
            TotalCount = totalCount;
            Items = items;
        }

        /// <summary>
        /// Gets total amount of matching rows.
        /// </summary>
        public int TotalCount { get; private set; }

        /// <summary>
        /// Gets items in this page
        /// </summary>
        public IEnumerable<T> Items { get; private set; }
    }
}
