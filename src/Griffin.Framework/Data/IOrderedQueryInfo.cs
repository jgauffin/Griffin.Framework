namespace Griffin.Framework.Data
{
    /// <summary>
    /// Exposes information for queries
    /// </summary>
    public interface IOrderedQueryInfo : IQueryInfo
    {
        /// <summary>
        /// Gets property to sort by (one of the result properties)
        /// </summary>
        /// <remarks><c>null</c> if not specified.</remarks>
        string OrderByPropertyName { get; }

        /// <summary>
        /// Gets if ascending sort should be used
        /// </summary>
        /// <value>true per default.</value>
        bool OrderAscending { get; }
    }
}