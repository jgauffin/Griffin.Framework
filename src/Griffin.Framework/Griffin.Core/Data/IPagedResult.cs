using System.Collections.Generic;

namespace Griffin.Data
{
    public interface IPagedResult<T> where T : class
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