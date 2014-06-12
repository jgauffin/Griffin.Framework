namespace Griffin.Container
{
    /// <summary>
    /// Defines which lifetime a service implementation should have in the inversion of control container
    /// </summary>
    /// <seealso cref="ContainerServiceAttribute"/>
    public enum ContainerLifetime
    {
        /// <summary>
        /// Scoped, i.e. will be diposed when the child container is disposed.
        /// </summary>
        Scoped,

        /// <summary>
        /// Same instance should be used for all retrievals.
        /// </summary>
        SingleInstance,

        /// <summary>
        /// Return a new instance every time
        /// </summary>
        Transient

    }
}