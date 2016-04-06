using System.Collections.Generic;

namespace Griffin.Data.Queries
{
    /// <summary>
    ///     Defines minimum parameters for query search results.
    /// </summary>
    /// <typeparam name="T">Type of return model</typeparam>
    public interface IQueryResult<out T> where T : class
    {
        /// <summary>
        ///     Gets all matching items
        /// </summary>
        IEnumerable<T> Items { get; }

        /// <summary>
        ///     Gets total number of items (useful when paging is used)
        /// </summary>
        int TotalCount { get; }
    }
}