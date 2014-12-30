using System;

namespace Griffin.ApplicationServices.AppDomains
{
    /// <summary>
    ///     Implement this class to run one or more services inside an isolated AppDomain.
    /// </summary>
    public interface IApplicationInitialize
    {
        /// <summary>
        ///     Supports pausing (still running but not processing anything)
        /// </summary>
        bool SupportsPause { get; }

        /// <summary>
        ///     Start a service
        /// </summary>
        /// <param name="argv">Application arguments</param>
        void Start(string[] argv);

        /// <summary>
        ///     Stop service
        /// </summary>
        void Stop();

        /// <summary>
        ///     Pause service (continue to run, but do not process anything)
        /// </summary>
        void Pause();

        /// <summary>
        ///     Resume after being paused.
        /// </summary>
        /// <exception cref="InvalidOperationException">Is not paused.</exception>
        void Resume();

        /// <summary>
        ///     We've detected an update.
        /// </summary>
        /// <param name="information">Information about the update.</param>
        void OnUpdateAvailable(IUpdateInformation information);

        /// <summary>
        ///     Want to restart application
        /// </summary>
        /// <returns><c>true</c> if application can be restarted; otherwise <c>false</c>.</returns>
        bool RequestRestartPermission();
    }
}