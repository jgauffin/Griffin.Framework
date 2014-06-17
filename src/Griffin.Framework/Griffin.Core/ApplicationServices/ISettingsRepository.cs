using System;

namespace Griffin.ApplicationServices
{
    /// <summary>
    ///     Settings used by <see cref="ApplicationServiceManager" /> and <see cref="BackgroundJobManager"/>.
    /// </summary>
    public interface ISettingsRepository
    {
        /// <summary>
        ///     Check if a service/job should be running.
        /// </summary>
        /// <param name="type">
        ///     A type that implements <see cref="IApplicationService" /> or <see cref="IBackgroundJob"/>.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the service/job should be running.;<c>false</c> if it should be shut down.
        /// </returns>
        /// <exception cref="ArgumentNullException">type</exception>
        bool IsEnabled(Type type);
    }
}



