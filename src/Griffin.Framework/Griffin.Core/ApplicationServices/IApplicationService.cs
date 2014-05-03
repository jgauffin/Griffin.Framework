namespace Griffin.ApplicationServices
{
    /// <summary>
    /// A service (class) which is running during the entire application lifetime
    /// </summary>
    /// <remarks>
    /// <para>
    /// Services are intended to be started when the application is started and stopped when the application is stopped.
    /// </para>
    /// </remarks>
    public interface IApplicationService
    {
        /// <summary>
        /// Start service
        /// </summary>
        void Start();

        /// <summary>
        /// stop service
        /// </summary>
        void Stop();
    }
}
