namespace Griffin.ApplicationServices.AppDomains
{
    /// <summary>
    ///     How to restart applications
    /// </summary>
    public enum RestartOrder
    {
        /// <summary>
        ///     Stop the old AppDomain before starting the new one.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The old one will be restarted if the new domain fails to start. This also means that no AppDomain will be
        ///         running for a short while.
        ///     </para>
        /// </remarks>
        StopOldFirst,

        /// <summary>
        ///     Start the new AppDomain before shutting down the old one.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         This means that there will always be an AppDomain running, but both AppDomains will be running side-by-side for
        ///         a short while. This means that
        ///         operations in your application might produce duplicate result depending on how you have designed your
        ///         application.
        ///     </para>
        /// </remarks>
        StartNewFirst
    }
}