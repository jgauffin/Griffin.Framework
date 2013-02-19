using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace Griffin.Framework.Authentication.Password.Strategies.Digest
{
    /// <summary>
    /// Monitors all nonces.
    /// </summary>
    public class NonceService : IDisposable
    {
        private readonly TimeSpan _expiresTimeout = new TimeSpan(0, 0, 15);
        private readonly ConcurrentDictionary<string, Nonce> _items = new ConcurrentDictionary<string, Nonce>();
        private Timer _timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="NonceService"/> class.
        /// </summary>
        public NonceService()
        {
            _timer = new Timer(Sweep, null, _expiresTimeout, _expiresTimeout);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NonceService"/> class.
        /// </summary>
        /// <param name="expiresTimeout">How long a nonce is valid, default is 15 seconds.</param>
        public NonceService(TimeSpan expiresTimeout)
        {
            _expiresTimeout = expiresTimeout;
            _timer = new Timer(Sweep, null, _expiresTimeout, _expiresTimeout);
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (_timer == null)
                return;

            _timer.Dispose();
            _timer = null;
        }

        #endregion

        /// <summary>
        /// Checks if a nonce is valid
        /// </summary>
        /// <param name="value">nonce value</param>
        /// <param name="counter">nc counter</param>
        /// <returns>true if nonce is valid; otherwise false.</returns>
        public virtual bool IsValid(string value, int counter)
        {
            Nonce nonce;
            if (!_items.TryGetValue(value, out nonce))
                return false;

            if (!nonce.Validate(counter))
                return false;

            return !nonce.Expired;
        }

        /// <summary>
        /// Create a new nonce
        /// </summary>
        /// <returns>Created nonce.</returns>
        /// <remarks>Valid Time span is configured in the <see cref="NonceService(System.TimeSpan)"/> constructor. Default time is 15 seconds.</remarks>
        public string CreateNonce()
        {
            var nonce = Guid.NewGuid().ToString("N");
            _items.AddOrUpdate(nonce, new Nonce(DateTime.Now.Add(_expiresTimeout)), (x, y) => null);
            return nonce;
        }

        /// <summary>
        /// Remove expired nonces.
        /// </summary>
        /// <param name="state"></param>
        private void Sweep(object state)
        {
            _items.Where(kvp => (DateTime.Now - kvp.Value.LastUpdate) > _expiresTimeout)
                .Select(kvp => kvp.Key)
                .ToList()
                .ForEach(key =>
                             {
                                 Nonce item;
                                 _items.TryRemove(key, out item);
                             });
        }
    }
}