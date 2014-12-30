using System;
using System.IO;
using Griffin.Net.Channels;
using Griffin.Net.Protocols.Stomp.Frames;

namespace Griffin.Net.Protocols.Stomp
{
    /// <summary>
    /// Decode incoming bytes into STOMP frames.
    /// </summary>
    public class StompDecoder : IMessageDecoder
    {
        private readonly HeaderParser _headerParser = new HeaderParser();
        private BasicFrame _frame;
        private Action<object> _messageReceived;
        private bool _isHeaderParsed;
        private int _frameContentBytesLeft = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="StompDecoder"/> class.
        /// </summary>
        public StompDecoder()
        {
            _headerParser.FrameNameParsed = OnFrameName;
            _headerParser.HeaderParsed = OnHeaderName;
            _headerParser.Completed = OnHeaderCompleted;
            _messageReceived = delegate { };
        }

        private int BytesProcessed(int startOffset, int currentOffset)
        {
            return currentOffset - startOffset;
        }
        /// <summary>
        ///     We've received bytes from the socket. Build a message out of them.
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <remarks></remarks>
        public void ProcessReadBytes(ISocketBuffer buffer)
        {
            int receiveBufferOffset = buffer.Offset;
            int bytesLeftInReceiveBuffer = buffer.BytesTransferred;
            while (true)
            {
                if (bytesLeftInReceiveBuffer <= 0)
                    break;

                if (!_isHeaderParsed)
                {
                    var offsetBefore = receiveBufferOffset;
                    receiveBufferOffset = _headerParser.Parse(buffer, receiveBufferOffset);
                    if (!_isHeaderParsed)
                        return;

                    bytesLeftInReceiveBuffer -= receiveBufferOffset - offsetBefore;
                    _frameContentBytesLeft = _frame.ContentLength;
                    if (_frameContentBytesLeft == 0)
                    {
                        // the NULL message delimiter
                        if (bytesLeftInReceiveBuffer == 1)
                            bytesLeftInReceiveBuffer = 0;

                        MessageReceived(_frame);
                        _frame = null;
                        _isHeaderParsed = false;
                        continue;
                    }

                    _frame.Body = new MemoryStream();
                }

                var bytesRead = BytesProcessed(buffer.Offset, receiveBufferOffset);
                var bytesToWrite = Math.Min(_frameContentBytesLeft, buffer.BytesTransferred - bytesRead);
                _frame.Body.Write(buffer.Buffer, receiveBufferOffset, bytesToWrite);
                _frameContentBytesLeft -= bytesToWrite;
                receiveBufferOffset += bytesToWrite;
                bytesLeftInReceiveBuffer -= bytesToWrite;
                bytesRead += bytesToWrite;
                if (_frameContentBytesLeft == 0)
                {
                    // ignore NULL (message delimiter)
                    //TODO: Maybe verify it? ;)
                    var bytesRemaining = buffer.BytesTransferred - bytesRead;
                    if (bytesRemaining == 1)
                    {
                        bytesLeftInReceiveBuffer--;
                        receiveBufferOffset++;
                    }


                    _frame.Body.Position = 0;
                    MessageReceived(_frame);
                    Clear();
                }
            }

        }

        /// <summary>
        /// Clear state to allow this decoder to be reused.
        /// </summary>
        public void Clear()
        {
            _frame = null;
            _isHeaderParsed = false;
            _frameContentBytesLeft = 0;
        }

        /// <summary>
        ///     A message have been received.
        /// </summary>
        /// <remarks>
        ///     Do note that streams are being reused by the decoder, so don't try to close it.
        /// </remarks>
        public Action<object> MessageReceived
        {
            get { return _messageReceived; }
            set
            {
                if (value == null)
                    _messageReceived = m => { };
                else
                    _messageReceived = value;
            }
        }

        private void OnHeaderCompleted()
        {
            _isHeaderParsed = true;
        }

        private void OnFrameName(string name)
        {
            _frame = new BasicFrame(name);
        }

        private void OnHeaderName(string name, string value)
        {
            _frame.AddHeader(name, value);
        }
    }
}