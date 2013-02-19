namespace Griffin.Framework
{
    /// <summary>
    /// Interface used to trigger the scope events.
    /// </summary>
    public interface IScopePublisher
    {
        /// <summary>
        /// Trigger that a scope has been started.
        /// </summary>
        /// <param name="identifier"></param>
        void TriggerStarting(object identifier);
        void TriggerStarted(object identifier);
        void TriggerEnding(object identifier);
        void TriggerEnded(object identifier);
    }
}