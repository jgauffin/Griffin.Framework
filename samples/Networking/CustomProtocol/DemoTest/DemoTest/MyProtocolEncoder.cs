using System;
using System.IO;
using System.Runtime.Serialization.Formatters;
using Griffin;
using Griffin.Net;
using Griffin.Net.Channels;
using Griffin.Net.Protocols;
using Newtonsoft.Json;

namespace DemoTest
{
    public class MyProtocolEncoder : IMessageEncoder
    {
        private readonly MemoryStream _memoryStream = new MemoryStream(65535);
        private readonly JsonSerializer _serializer = new JsonSerializer();
        private int _bytesLeft;
        private int _offset;

        public MyProtocolEncoder()
        {
            _serializer = JsonSerializer.Create(new JsonSerializerSettings()
            {
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                ObjectCreationHandling = ObjectCreationHandling.Auto,
                TypeNameHandling = TypeNameHandling.Auto
            });
        }

        public void Prepare(object message)
        {
            // Clear the memory stream
            _memoryStream.SetLength(0);

            // start at position 4 so that we can insert the length header.
            _memoryStream.Position = 4;
            var writer = new StreamWriter(_memoryStream);
            _serializer.Serialize(writer, message, typeof(object));
            writer.Flush();

            // insert length header.
            // BitConverter2 exists in a library and uses an existing buffer instead of allocating a new one.
            BitConverter2.GetBytes((int)_memoryStream.Length - 4, _memoryStream.GetBuffer(), 0);

            // and save how much we should send
            _bytesLeft = (int)_memoryStream.Length;
            _offset = 0;
        }


        public void Send(ISocketBuffer buffer)
        {
            // Continue from where the last send operation stopped.
            buffer.SetBuffer(_memoryStream.GetBuffer(), _offset, _bytesLeft);
        }

        public bool OnSendCompleted(int bytesTransferred)
        {
            // a send operation completed, move forward in our buffer.
            _offset += bytesTransferred;
            _bytesLeft -= bytesTransferred;
            return _bytesLeft <= 0;
        }

        public void Clear()
        {
            _bytesLeft = 4;
            _memoryStream.SetLength(0);
            _memoryStream.Position = 0;
        }
    }
}