namespace Griffin.Net.LiteServer.Modules
{
    /// <summary>
    /// Specifies how the modules want the current request to proceed
    /// </summary>
    public enum ModuleResult
    {
        /// <summary>
        /// Continue with the next module
        /// </summary>
        Continue,

        /// <summary>
        /// Send response directly without invoking any more modules
        /// </summary>
        SendResponse,

        /// <summary>
        /// Disconnect client after sending the response (if there are a response)
        /// </summary>
        Disconnect
    }
}