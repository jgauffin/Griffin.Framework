using System;
using System.IO.Pipes;
using System.Linq;
using System.Text;

namespace Griffin.ApplicationServices.AppDomains.Host
{
    internal class CommunicationChannel
    {
        private readonly byte[] _readBuffer = new byte[65535];
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private NamedPipeClientStream _pipe;

        public ReceivedCommandHandler ReceivedCommand { get; set; }

        public void Start(string pipeName, string id)
        {
            _pipe = new NamedPipeClientStream(pipeName);
            _pipe.Connect();
            _pipe.BeginRead(_readBuffer, 0, _readBuffer.Length, OnRead, null);

            Write("hello", id);
        }

        public void Stop()
        {
           Write("goodbye");
        }

        private void OnRead(IAsyncResult ar)
        {
            var bytesRead = _pipe.EndRead(ar);
            var str = Encoding.ASCII.GetString(_readBuffer, 0, bytesRead);
            _stringBuilder.Append(str);

            ProcessInBuffer();

            _pipe.BeginRead(_readBuffer, 0, _readBuffer.Length, OnRead, null);
        }

        private void ProcessInBuffer()
        {
            var index = 0;
            while (index < _readBuffer.Length)
            {
                if (_stringBuilder[index] == '\n')
                {
                    var packet = "";
                    try
                    {
                        packet = _stringBuilder.ToString(0, index);
                        _stringBuilder.Remove(0, index + 1);
                        var commandParts = packet.Split('\x4');
                        ReceivedCommand(commandParts[0], commandParts.Skip(1).ToArray());
                    }
                    catch (Exception exception)
                    {
                        var str = Encoding.UTF8.GetBytes(packet);
                        var error = Convert.ToBase64String(Encoding.UTF8.GetBytes(exception.ToString()));
                        var inpacket = Convert.ToBase64String(str);
                        try
                        {
                            Write("error", inpacket, error);
                        }
                        catch
                        {
                            //todo: Notify some other way?
                        }
                    }

                    index = 0;
                }
            }
        }

        public void Write(string command, params string[] argv)
        {
            try
            {
                var str = string.Format("{0}\x4{1}\n", command, string.Join("\x4", argv));
                var buffer = Encoding.ASCII.GetBytes(str);
                _pipe.Write(buffer, 0, buffer.Length);
            }
            catch (Exception exception)
            {
                //TODO: notify
            }
        }

        public void Send(string command, params string[] argv)
        {
            Write(command, argv);
        }
    }
}