namespace Griffin.Framework
{
    /// <summary>
    /// A scope listener
    /// </summary>
    /// <seealso cref="ScopeListeners"/>
    public interface IScopeListener
    {
        /// <summary>
        /// The scope has not yet been created, but will be after this invocation
        /// </summary>
        /// <param name="identifier">The identifier depends on the type of framework which started the scope. It might be the <c>HttpContextBase</c> for HTTP requests while being the command for Griffin.Decoupled</param>
        void ScopeStarting(object identifier);

        /// <summary>
        /// Scope has been started
        /// </summary>
        /// <param name="identifier">The identifier depends on the type of framework which started the scope. It might be the <c>HttpContextBase</c> for HTTP requests while being the command for Griffin.Decoupled</param>
        void ScopeStarted(object identifier);


        /// <summary>
        /// All processing has been completed, the scope will be teared down.
        /// </summary>
        /// <param name="identifier">The identifier depends on the type of framework which started the scope. It might be the <c>HttpContextBase</c> for HTTP requests while being the command for Griffin.Decoupled</param>
        void ScopeEnding(object identifier);

        /// <summary>
        /// Scope has been disposed.
        /// </summary>
        /// <param name="identifier">The identifier depends on the type of framework which started the scope. It might be the <c>HttpContextBase</c> for HTTP requests while being the command for Griffin.Decoupled</param>
        void ScopeEnded(object identifier);
    }
}