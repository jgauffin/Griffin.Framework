namespace Griffin.Framework.Text
{
    /// <summary>
    /// Interface used to access the data source.
    /// </summary>
    /// <remarks>You can create a facade which supports scopes (to be able to handle
    /// database connections etc)</remarks>
    public interface IDataSourceProvider
    {
        /// <summary>
        /// Gets current data source.
        /// </summary>
        ITextDataSource DataSource { get; }
    }
}