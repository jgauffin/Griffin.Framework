using System;
using System.Collections;
using System.Collections.Generic;

namespace Griffin.Net.Protocols.Http.Messages
{
    /// <summary>
    /// Collection of ranges which have been specified in the Range header.
    /// </summary>
    public class RangeCollection : IEnumerable<Range>
    {
        private readonly List<Range> _ranges = new List<Range>();

        /// <summary>
        /// Total length of all ranges (i.e. the number of bytes to transfer)
        /// </summary>
        public int TotalLength { get; private set; }

        /// <summary>
        /// Get one of the ranges.
        /// </summary>
        /// <param name="index">Zero based index</param>
        /// <returns>Range</returns>
        public Range this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException("index", index,
                        string.Format("Must be between 0 and {0}.", Count - 1));
                return _ranges[index];
            }
        }

        /// <summary>
        /// Gets number of ranges
        /// </summary>
        public int Count
        {
            get { return _ranges.Count; }
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Range> GetEnumerator()
        {
            return _ranges.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Parse range header value
        /// </summary>
        /// <param name="header">The "Range" header value</param>
        /// <param name="streamLength">File size (or size of the entire stream)</param>
        public void Parse(string header, int streamLength)
        {
            if (header == null) throw new ArgumentNullException("header");
            if (streamLength <= 0)
                throw new ArgumentOutOfRangeException("streamLength", streamLength, "Must be 1 or larger.");

            /*  - The first 500 bytes (byte offsets 0-499, inclusive):  bytes=0-
        499
      - The second 500 bytes (byte offsets 500-999, inclusive):
        bytes=500-999
      - The final 500 bytes (byte offsets 9500-9999, inclusive):
        bytes=-500
      - Or bytes=9500-
      - The first and last bytes only (bytes 0 and 9999):  bytes=0-0,-1
      - Several legal but not canonical specifications of the second 500
        bytes (byte offsets 500-999, inclusive):
         bytes=500-600,601-999
         bytes=500-700,601-999*/

            if (!header.StartsWith("bytes=", StringComparison.OrdinalIgnoreCase))
                throw new FormatException("Expected range to start with 'bytes='.");

            header = header.Remove(0, "bytes=".Length);

            var ranges = header.Split(',');
            foreach (var range in ranges)
            {
                var ourRange = new Range(range, streamLength);

                _ranges.Add(ourRange);
                TotalLength = TotalLength + ourRange.Count;
                if (TotalLength > streamLength)
                    throw new ArgumentException(
                        string.Format("Inner stream is just {0} bytes long, while we should send {1} bytes.",
                            streamLength, TotalLength));
            }
        }

        /// <summary>
        /// Create a string which is valid as value in the Content-Range header.
        /// </summary>
        /// <param name="streamLength">Length of the stream.</param>
        /// <returns><c>bytes X/Y</c></returns>
        public string ToHtmlHeaderValue(int streamLength)
        {
            return string.Format("bytes {0}/{1}", _ranges[0], streamLength);
        }
    }
}