namespace Griffin.Signals
{
    /// <summary>
    /// Kind of signal
    /// </summary>
    public enum SignalKind
    {
        /// <summary>
        /// Not specified, might indicate that something is failing or working.
        /// </summary>
        Undefined,

        /// <summary>
        /// Running as expected
        /// </summary>
        Running,

        /// <summary>
        /// Fault state
        /// </summary>
        Fault
    }
}