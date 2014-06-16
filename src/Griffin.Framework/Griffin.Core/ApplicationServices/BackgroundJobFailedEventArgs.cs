using System;

namespace Griffin.ApplicationServices
{
    /// <summary>
    ///     Failed to execute a job. 
    /// </summary>
    /// <remarks>
    /// <para>
    /// Assign <see cref="CanContinue" /> specify if the rest of the jobs can be executed. Default is <c>true</c>.
    /// </para>
    /// </remarks>
    public class BackgroundJobFailedEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BackgroundJobFailedEventArgs" /> class.
        /// </summary>
        /// <param name="job">Job that failed</param>
        /// <param name="exception">Exception that the job threw.</param>
        public BackgroundJobFailedEventArgs(IBackgroundJob job, Exception exception)
        {
            if (job == null) throw new ArgumentNullException("job");
            if (exception == null) throw new ArgumentNullException("exception");

            Job = job;
            Exception = exception;
            CanContinue = true;
        }

        /// <summary>
        ///     Job that failed.
        /// </summary>
        public IBackgroundJob Job { get; private set; }

        /// <summary>
        ///     Exception that the job threw.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        ///     If we ca continue execute the rest of the jobs.
        /// </summary>
        /// <value>
        ///     Default is <c>true</c>.
        /// </value>
        public bool CanContinue { get; set; }
    }
}