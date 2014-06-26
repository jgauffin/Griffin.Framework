using System;
using System.IO;
using Griffin.Net.Channels;

namespace Griffin.Net.Protocols.Stomp
{
    public class StompEncoder : IMessageEncoder
    {
        private int _bytesToSend;
        private IFrame _frame;
        private bool _isHeaderSent;
        private int _offset;
        private byte[] _buffer = new byte[65535];
        private MemoryStream _stream;
        private int _totalAmountToSend;
        private StreamWriter _writer;

        public StompEncoder()
        {
            _stream = new MemoryStream(_buffer);
            _stream.SetLength(0);
            _writer = new StreamWriter(_stream);
        }

        /// <summary>
        ///     Are about to send a new message
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <remarks>
        ///     Can be used to prepare the next message. for instance serialize it etc.
        /// </remarks>
        /// <exception cref="NotSupportedException">Message is of a type that the encoder cannot handle.</exception>
        public void Prepare(object message)
        {
            if (!(message is IFrame))
                throw new NotSupportedException("Only supports IFrame derived classes");

            _frame = (IFrame) message;
        }

        /// <summary>
        ///     Buffer structure used for socket send operations.
        /// </summary>
        /// <param name="buffer">
        ///     Do note that there are not buffer attached to the structure, you have to assign one yourself using
        ///     <see cref="ISocketBuffer.SetBuffer(int,int)" />. This choice was made
        ///     to prevent unnecessary copy operations.
        /// </param>
        public void Send(ISocketBuffer buffer)
        {
            if (_bytesToSend > 0)
            {
                buffer.SetBuffer(_buffer, _offset, _bytesToSend, _bytesToSend);
                return;
            }
            if (_isHeaderSent)
            {
                var bytes = Math.Min(_totalAmountToSend, _buffer.Length);
                _frame.Body.Read(_buffer, 0, bytes);
                _bytesToSend = bytes;

                // include null char as message delimter
                if (bytes < _buffer.Length)
                {
                    buffer.Buffer[_bytesToSend] = 0;
                    bytes++;
                }

                buffer.SetBuffer(_buffer, 0, bytes, bytes);
                return;
            }

            if (_frame.Name == "NoOp")
            {
                _bytesToSend = 0;
                buffer.Buffer[0] = 10;
                buffer.SetBuffer(0, 1);
                return;
            }

            _writer.Write("{0}\n", _frame.Name);
            foreach (var header in _frame.Headers)
            {
                _writer.Write("{0}:{1}\n", header.Key, header.Value);
            }

            _writer.Write("\n");
            _writer.Flush();
            _isHeaderSent = true;

            if (_frame.Body == null || _frame.ContentLength == 0)
            {
                _stream.Write(new byte[] {0}, 0, 1);
                _bytesToSend = (int) _stream.Length;
                _totalAmountToSend = _bytesToSend;
                buffer.SetBuffer(_buffer, 0, (int) _stream.Length);
                return;
            }

            var bytesLeft = _buffer.Length - _stream.Length;
            var bytesToSend = Math.Min(_frame.ContentLength, (int) bytesLeft);
            var offset = (int) _stream.Position;
            _frame.Body.Read(_buffer, offset, bytesToSend);
            _bytesToSend = (int) _stream.Length + bytesToSend;
            _totalAmountToSend = (int) _stream.Length + _frame.ContentLength + 1;

            // everything is done in the first run
            // so add the message delimiter.
            if (_totalAmountToSend < buffer.Count)
            {
                _totalAmountToSend++;
                _bytesToSend++;
                _buffer[_bytesToSend] = 0;
            }

            buffer.SetBuffer(_buffer, 0, _totalAmountToSend);
        }

        /// <summary>
        ///     The previous <see cref="IMessageEncoder.Send" /> has just completed.
        /// </summary>
        /// <param name="bytesTransferred"></param>
        /// <remarks><c>true</c> if the message have been sent successfully; otherwise <c>false</c>.</remarks>
        public bool OnSendCompleted(int bytesTransferred)
        {
            _totalAmountToSend -= bytesTransferred;
            _bytesToSend -= bytesTransferred;
            _offset += bytesTransferred;

            if (_bytesToSend <= 0)
                _offset = 0;

            return _bytesToSend <= 0;
        }

        /// <summary>
        ///     Remove everything used for the last message
        /// </summary>
        public void Clear()
        {
            _bytesToSend = 0;
            _frame = null;
            _isHeaderSent = false;
        }
    }
}