using System;

namespace Griffin.Signals
{
    /// <summary>
    ///     Arguments for <see cref="Signal.Supressed" />
    /// </summary>
    public class SignalSupressedEventArgs : EventArgs
    {
        /// <summary>
        ///     Create a new instance of <see cref="SignalRaisedEventArgs" />.
        /// </summary>
        /// <param name="signalName">Name of the supressed signal (as registered by <c>Signal.Create()</c>).</param>
        /// <param name="callingMethod">Type/Method that supressed the signal</param>
        public SignalSupressedEventArgs(string signalName, string callingMethod)
        {
            if (signalName == null) throw new ArgumentNullException("signalName");
            if (callingMethod == null) throw new ArgumentNullException("callingMethod");
            SignalName = signalName;
            CallingMethod = callingMethod;
        }

        /// <summary>
        ///     Name of the supressed signal (as registered by <c>Signal.Create()</c>)
        /// </summary>
        public string SignalName { get; private set; }

        /// <summary>
        ///     Type/Method that supressed the signal
        /// </summary>
        public string CallingMethod { get; private set; }

        /// <summary>
        ///     Signal was by automation (typically by expiry).
        /// </summary>
        public bool Automated { get; set; }
    }
}
