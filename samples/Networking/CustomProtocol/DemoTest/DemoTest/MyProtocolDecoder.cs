using System;
using System.IO;
using System.Runtime.Serialization.Formatters;
using Griffin.Net;
using Griffin.Net.Channels;
using Griffin.Net.Protocols;
using Newtonsoft.Json;

namespace DemoTest
{
    public class MyProtocolDecoder : IMessageDecoder
    {
        private readonly byte[] _headerBuf = new byte[4];
        private readonly JsonSerializer _serializer;
        private readonly MemoryStream _stream = new MemoryStream();
        private int _bytesLeft;
        private int _headerBytesLeft = 4;
        private int _headerOffset = 0;
        private bool _isHeaderRead;

        public MyProtocolDecoder()
        {
            _serializer = JsonSerializer.Create(new JsonSerializerSettings()
            {
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                ObjectCreationHandling = ObjectCreationHandling.Auto,
                TypeNameHandling = TypeNameHandling.Auto
            });
        }

        public void ProcessReadBytes(ISocketBuffer buffer)
        {
            var offset = buffer.Offset;
            var count = buffer.BytesTransferred;

            // start by reading header.
            if (!_isHeaderRead)
            {
                var headerBytesRead = Math.Min(_headerBytesLeft, count);
                Buffer.BlockCopy(buffer.Buffer, offset, _headerBuf, _headerOffset, headerBytesRead);
                _headerBytesLeft -= headerBytesRead;

                // have not received all bytes left.
                // can occur if we send a lot of messages so that the nagle algorithm merges
                // messages so that the header is in the end of the socket stream.
                if (_headerBytesLeft > 0)
                    return;

                count -= headerBytesRead;
                offset += headerBytesRead;
                _bytesLeft = BitConverter.ToInt32(_headerBuf, 0);
                _isHeaderRead = true;
            }

            var bodyBytesToRead = Math.Min(_bytesLeft, count);
            _stream.Write(buffer.Buffer, offset, bodyBytesToRead);
            _bytesLeft -= bodyBytesToRead;
            if (_bytesLeft == 0)
            {
                _stream.Position = 0;
                var item = _serializer.Deserialize(new StreamReader(_stream), typeof(object));
                MessageReceived(item);
                Clear();

                //TODO: Recursive call to read any more messages
            }
        }

        public void Clear()
        {
            _bytesLeft = 0;
            _headerBytesLeft = 4;
            _headerOffset = 0;
            _isHeaderRead = false;
            _stream.SetLength(0);
            _stream.Position = 0;
        }

        public Action<object> MessageReceived { get; set; }
    }
}