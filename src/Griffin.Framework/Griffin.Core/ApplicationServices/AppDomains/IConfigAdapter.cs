namespace Griffin.ApplicationServices.AppDomains
{
    /// <summary>
    ///     A writable configuration source
    /// </summary>
    public interface IConfigAdapter
    {
        /// <summary>
        ///     Access a config setting
        /// </summary>
        /// <param name="name">Name of the setting</param>
        /// <returns>value if found; otherwise <c>null</c>.</returns>
        string this[string name] { get; set; }
    }
}