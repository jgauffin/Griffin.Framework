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
        ///     An updated have been detected and an restart is pending.
        /// </summary>
        /// <returns><c>true</c> if application can be restarted; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>
        /// You can deny a restart if your application is doing something that will fail during a restart. The framework will the try again later.
        /// If you return <c>true</c> the <c>Stop()</c> method will be called for this app domain and then <c>Start()</c> will be called in the new app domain.
        /// </para>
        /// </remarks>
        bool RequestRestartPermission();
    }
}