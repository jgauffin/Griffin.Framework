namespace Griffin.ApplicationServices.AppDomains
{
    /// <summary>
    /// Implement this class to run one or more services inside an isolated AppDomain.
    /// </summary>
    public interface IApplicationInitialize
    {
        void Start(string[] argv);
        void Stop();
        bool SupportsPause { get; }
        void Pause();
        void Resume();

        /// <summary>
        /// We've 
        /// </summary>
        /// <param name="updateInformation"></param>
        void OnUpdateAvailable(IUpdateInformation information);

        /// <summary>
        /// Want to restart application
        /// </summary>
        /// <returns><c>true</c> if application can be restarted; otherwise <c>false</c>.</returns>
        bool RequestRestartPermission();
    }
}