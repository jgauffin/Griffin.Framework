namespace Griffin.Framework.Data
{
    /// <summary>
    /// Returns information used for paging
    /// </summary>
    public interface IPagedQueryInfo : IQueryInfo
    {
        /// <summary>
        /// Gets page number (one based inddex)
        /// </summary>
        int PageNumber { get; }

        /// <summary>
        /// Gets items per page.
        /// </summary>
        int PageSize { get; }
    }
}