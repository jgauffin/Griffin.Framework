namespace Griffin.Framework.Data
{
    /// <summary>
    /// The query base interface
    /// </summary>
    /// <typeparam name="TResult">Type of result returned by this query</typeparam>
    public interface IQuery<TResult>
    {
        /// <summary>
        /// Extract the information that the query contains.
        /// </summary>
        /// <returns></returns>
        /// <remarks>Used to let different implementations process the information provided by this query.</remarks>
        IQueryInfo ToQueryInfo();
    }
}
