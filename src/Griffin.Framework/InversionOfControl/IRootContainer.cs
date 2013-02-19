namespace Griffin.Framework.InversionOfControl
{
    /// <summary>
    /// Root container.
    /// </summary>
    public interface IRootContainer : IServiceLocator
    {
        /// <summary>
        /// Create a new child container (holds all scoped services)
        /// </summary>
        /// <returns>Child container.</returns>
        IChildContainer CreateChildContainer();

        /// <summary>
        /// Gets current child
        /// </summary>
        /// <returns>child/scoped container which is allocated for the current thread (the last one if several has been created); otherwise <c>null</c>.</returns>
        IChildContainer CurrentChild { get; }
    }
}