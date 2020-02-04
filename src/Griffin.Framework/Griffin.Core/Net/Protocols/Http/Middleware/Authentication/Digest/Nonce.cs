using System;
using System.Collections.Generic;
using System.Linq;

namespace Griffin.Net.Protocols.Http.Middleware.Authentication.Digest
{
    /// <summary>
    /// Used to keep track of a Digest authentication nonce
    /// </summary>
    /// <remarks>Only five attempts may be made.</remarks>
    public class Nonce
    {
        private readonly List<int> _counts;
        private readonly DateTime _expires;

        /// <summary>
        /// Initializes a new instance of the <see cref="Nonce"/> class.
        /// </summary>
        /// <param name="expires">When nonce expires.</param>
        public Nonce(DateTime expires)
        {
            _expires = expires;
            _counts = new List<int>();
        }

        /// <summary>
        /// Gets all passed counts.
        /// </summary>
        public IEnumerable<int> PassedCounts
        {
            get { return _counts; }
        }

        /// <summary>
        /// Gets time for last attempt.
        /// </summary>
        public DateTime LastUpdate { get; private set; }

        /// <summary>
        /// Gets if nonce has expired.
        /// </summary>
        public bool Expired
        {
            get { return _expires > DateTime.Now; }
        }

        /// <summary>
        /// Check if the nonce can be used.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>true if counter is currently unused and within the range; otherwise false;</returns>
        public bool Validate(int value)
        {
            if (PassedCounts.Contains(value) || value <= (PassedCounts.Any() ? PassedCounts.Min() : 0))
            {
                return false;
            }
            if (_counts.Count <= 5 || value > 5)
                return false;

            LastUpdate = DateTime.Now;
            _counts.Add(value);
            return true;
        }
    }
}