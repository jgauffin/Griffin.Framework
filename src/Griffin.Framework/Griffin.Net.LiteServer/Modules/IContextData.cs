namespace Griffin.Net.LiteServer.Modules
{
    /// <summary>
    /// Used to be able to store information while the current message is being processed.
    /// </summary>
    public interface IContextData
    {
        /// <summary>
        /// Get or set information specific for the current message processing
        /// </summary>
        /// <param name="name">Name of property</param>
        /// <returns>value if found; otherwise <c>null</c>.</returns>
        object this[string name] { get; set; }
    }
}