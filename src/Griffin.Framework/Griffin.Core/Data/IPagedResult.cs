using System.Collections.Generic;

namespace Griffin.Data
{
    /// <summary>
    /// The result is not complete, just a page.
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    public interface IPagedResult<out T> where T : class
    {
        /// <summary>
        /// Gets all matching items
        /// </summary>
        IEnumerable<T> Items { get; }

        /// <summary>
        /// Gets total number of items (useful when paging is used, otherwise 0)
        /// </summary>
        int TotalCount { get; }
    }
}