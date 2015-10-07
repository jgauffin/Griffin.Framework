using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Griffin.Signals
{
    /// <summary>
    ///     Used to transport data to a remote end point (typically a reporting service)
    /// </summary>
    [DataContract]
    public class SignalDTO
    {
        /// <summary>
        /// Create a new instance of SignalDTO.
        /// </summary>
        /// <param name="appName">Application that this signal is for</param>
        /// <param name="signal">Actual signal to generate the DTO from.</param>
        public SignalDTO(string appName, Signal signal)
        {
            if (appName == null) throw new ArgumentNullException("appName");
            if (signal == null) throw new ArgumentNullException("signal");
            IdleSinceUtc = signal.IdleSinceUtc;
            Name = signal.Name;
            ApplicationName = appName;
            IsRaised = signal.IsRaised;
            RaisedSinceUtc = signal.RaisedSinceUtc;
            UserToken = signal.UserToken;
            RaiseCountSinceLastReset = signal.RaiseCountSinceLastReset;
        }

        /// <summary>
        /// Create a new instance of <see cref="SignalDTO"/>.
        /// </summary>
        public SignalDTO()
        {

        }

        /// <summary>
        ///     When the signal was moved into non-signaled state.
        /// </summary>
        [DataMember(Order = 4)]
        public DateTime IdleSinceUtc { get; set; }

        /// <summary>
        ///     Signal name
        /// </summary>
        [DataMember(Order = 2)]
        public string Name { get; set; }

        /// <summary>
        ///     The application that the signal is for.
        /// </summary>
        [DataMember(Order = 1)]
        public string ApplicationName { get; set; }

        /// <summary>
        ///     Signal have been raised.
        /// </summary>
        [DataMember(Order = 3)]
        public bool IsRaised { get; set; }

        /// <summary>
        ///     When the signal was raised
        /// </summary>
        /// <value>
        ///     <c>DateTime.MinValue</c> if it's not raised.
        /// </value>
        [DataMember(Order = 5)]
        public DateTime RaisedSinceUtc { get; set; }

        /// <summary>
        ///     A token which you can use to keep track of if the signal should be raised or not.
        /// </summary>
        [DataMember(Order = 7)]
        public object UserToken { get; set; }

        /// <summary>
        /// Why the signal was raised
        /// </summary>
        [DataMember(Order = 8)]
        public string Reason { get; set; }

        /// <summary>
        /// Method that raised the signal.
        /// </summary>
        [DataMember(Order = 9)]
        public string CallingMethod { get; set; }

        /// <summary>
        /// Exception that caused the signal (if any)
        /// </summary>
        [DataMember(Order = 10)]
        public string Exception { get; set; }

        /// <summary>
        /// Additional properties
        /// </summary>
        [DataMember(Order = 11)]
        public Dictionary<string, string> Properties { get; set; }

        /// <summary>
        /// "Undefined", "Running", "Fault"
        /// </summary>
        [DataMember(Order = 12)]
        public string Kind { get; set; }

        /// <summary>
        ///     AMount of times that <c>Raise()</c> have been invoked since last reset.
        /// </summary>
        [DataMember(Order = 6)]
        public int RaiseCountSinceLastReset { get; set; }
    }
}