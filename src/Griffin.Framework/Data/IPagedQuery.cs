namespace Griffin.Framework.Data
{
    /// <summary>
    /// The query base interface
    /// </summary>
    /// <typeparam name="TResult">Type of result returned by this query</typeparam>
    /// <remarks>ToQueryInfo should contain <see cref="IPagedQueryInfo"/></remarks>
    public interface IPagedQuery<TResult> : IQuery<TResult>
    {
        /// <summary>
        /// Page items
        /// </summary>
        /// <param name="pageNumber">Page to fetch. One based index.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>Paged result.</returns>
        void Page(int pageNumber, int pageSize);
    }
}