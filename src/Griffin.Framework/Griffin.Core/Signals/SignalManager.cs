using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace Griffin.Signals
{
    /// <summary>
    ///     Takes care of all signals
    /// </summary>
    /// <remarks>
    ///     Allocated and used internally in <see cref="Signal" />.
    /// </remarks>
    internal class SignalManager : IDisposable
    {
        private readonly Timer _lowerSignalsTimer;

        private readonly ConcurrentDictionary<string, Signal> _signals =
            new ConcurrentDictionary<string, Signal>();

        /// <summary>
        ///     Manager.
        /// </summary>
        public SignalManager()
        {
            _lowerSignalsTimer = new Timer(OnLowerSignals, null, 2000, 2000);
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _lowerSignalsTimer.Dispose();
        }

        /// <summary>
        /// </summary>
        /// <param name="signalName"></param>
        /// <param name="addFunction"></param>
        /// <returns></returns>
        public Signal GetOrAdd(string signalName, Func<object, Signal> addFunction)
        {
            return _signals.GetOrAdd(signalName, addFunction);
        }


        /// <summary>
        ///     Do not check signals for expirations
        /// </summary>
        public void SuspendExpirations()
        {
            _lowerSignalsTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }


        public bool TryAdd(string signalName, Signal signal)
        {
            return _signals.TryAdd(signalName, signal);
        }

        public bool TryGet(string signalName, out Signal signal)
        {
            return _signals.TryGetValue(signalName, out signal);
        }

        private void OnLowerSignals(object state)
        {
            var values = _signals.Values.ToList();
            foreach (var signal in values)
            {
                var date = signal.RaisedSinceUtc;
                if (date == DateTime.MinValue)
                    continue;

                var elapsed = DateTime.UtcNow.Subtract(date);
                if (elapsed >= signal.Expiration)
                {
                    signal.Expire();
                }
            }
        }

        /// <summary>
        /// Remove all mapped signals
        /// </summary>
        public void Clear()
        {
            _signals.Clear();
        }
    }
}