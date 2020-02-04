using System;
using System.IO;

namespace Griffin.Net.Protocols.Http.Messages
{
    /// <summary>
    ///     Represents a HTTP range.
    /// </summary>
    public class Range
    {
        private readonly int _position;
        private int _bytesRemaining;
        private bool _firstRead = true;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Range" /> class.
        /// </summary>
        /// <param name="position">The position to start at in the stream/file.</param>
        /// <param name="count">Number of bytes in this range.</param>
        public Range(int position, int count)
        {
            _position = position;
            _bytesRemaining = count;
            Count = count;
            EndPosition = position + count;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Range" /> class.
        /// </summary>
        /// <param name="range">The range <c>"bytes "</c> should not be included.</param>
        /// <param name="streamLength">Total size of stream/file</param>
        /// <example>
        ///     Last 100 bytes
        ///     <code>
        /// -100
        /// </code>
        ///     Specific range
        ///     <code>
        /// 100-199
        /// </code>
        ///     From index 200 and the rest
        ///     <code>
        /// 200-
        /// </code>
        /// </example>
        public Range(string range, int streamLength)
        {
            var parts = range.Split('-');

            // last bytes
            if (parts[0] == "")
            {
                Count = Parse(parts[1], string.Format("value after the slash ('{0}')", range));
                _position = streamLength - Count;
                EndPosition = _position + Count - 1;
            }
            else if (parts[1] == "")
            {
                _position = Parse(parts[0], string.Format("value before the slash ('{0}')", range));
                Count = streamLength - _position;
                EndPosition = _position + Count - 1;
            }
            else
            {
                _position = Parse(parts[0], string.Format("value before the slash ('{0}')", range));
                EndPosition = Parse(parts[1], string.Format("value after the slash ('{0}')", range));
                Count = EndPosition - _position + 1; // count first and last byte
            }

            _bytesRemaining = Count;
        }

        /// <summary>
        ///     Gets value indicating if everything have been read using the <see cref="Read" /> method.
        /// </summary>
        public bool IsDone
        {
            get { return _bytesRemaining == 0; }
        }

        /// <summary>
        ///     Gets number of bytes to read
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        ///     Gets where to stop read
        /// </summary>
        public int EndPosition { get; private set; }

        /// <summary>
        ///     Gets start position
        /// </summary>
        public int Position
        {
            get { return _position; }
        }

        private int Parse(string value, string name)
        {
            int tmp;
            if (!int.TryParse(value, out tmp))
                throw new FormatException(string.Format("Expected {0} to be an int, got: '{1}'", name, value));

            return tmp;
        }

        /// <summary>
        ///     Reads from the specified stream and puts the content in the specified byte buffer.
        /// </summary>
        /// <param name="source">Read the range from this stream.</param>
        /// <param name="buffer">Buffer to copy stream bytes to.</param>
        /// <param name="offset">The offset in the buffer to start writing.</param>
        /// <param name="count">Number of bytes available to write to in the buffer.</param>
        /// <returns></returns>
        /// <remarks>
        ///     The stream must support seeking since this method will move to our range position before
        ///     start reading. Do note that the move is only made for the first read. Make sure that the position isn't changed
        ///     until
        ///     everything have been read in this range.
        /// </remarks>
        public int Read(Stream source, byte[] buffer, int offset, int count)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (buffer == null) throw new ArgumentNullException("buffer");

            if (_firstRead)
            {
                source.Position = _position;
                _firstRead = false;
            }

            var toRead = Math.Min(_bytesRemaining, count);
            var read = source.Read(buffer, offset, toRead);
            _bytesRemaining -= read;
            return read;
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0}-{1}", _position, EndPosition);
        }
    }
}