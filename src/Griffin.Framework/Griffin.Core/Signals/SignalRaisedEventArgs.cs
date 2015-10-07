using System;

namespace Griffin.Signals
{
    /// <summary>
    ///     Event for <see cref="Signal" />.
    /// </summary>
    public class SignalRaisedEventArgs : EventArgs
    {
        /// <summary>
        /// Create a new instance of <see cref="SignalRaisedEventArgs"/>.
        /// </summary>
        /// <param name="signalName">Name of the generated signal</param>
        /// <param name="reason">Why the signal was raised.</param>
        /// <param name="callingMethod">Method that triggered the signal</param>
        public SignalRaisedEventArgs(string signalName, string reason, string callingMethod)
        {
            if (signalName == null) throw new ArgumentNullException("signalName");
            if (reason == null) throw new ArgumentNullException("reason");
            if (callingMethod == null) throw new ArgumentNullException("callingMethod");
            SignalName = signalName;
            Reason = reason;
            CallingMethod = callingMethod;
        }

        /// <summary>
        /// Create a new instance of <see cref="SignalRaisedEventArgs"/>
        /// </summary>
        /// <param name="signalName">Name of the generated signal</param>
        /// <param name="reason">Why the signal was raised.</param>
        /// <param name="callingMethod">Method that triggered the signal</param>
        /// <param name="exception">exception that caused the signal to get raised.</param>
        public SignalRaisedEventArgs(string signalName, string reason, string callingMethod, Exception exception)
        {
            if (signalName == null) throw new ArgumentNullException("signalName");
            if (reason == null) throw new ArgumentNullException("reason");
            if (callingMethod == null) throw new ArgumentNullException("callingMethod");
            if (exception == null) throw new ArgumentNullException("exception");
            SignalName = signalName;
            Reason = reason;
            CallingMethod = callingMethod;
            Exception = exception;
        }

        /// <summary>
        /// Name of the signal. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// Actual signal is passed to the event as <c>sender</c>.
        /// </para>
        /// </remarks>
        public string SignalName { get; set; }

        /// <summary>
        /// Why the signal was raised.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Method that raised the signal
        /// </summary>
        public string CallingMethod { get; set; }

        /// <summary>
        /// Exception that caused the signal (if any)
        /// </summary>
        public Exception Exception { get; set; }
    }
}