using System;
using System.IO.Pipes;
using System.Linq;
using System.Text;

namespace Griffin.ApplicationServices.AppDomains.Controller
{
    /// <summary>
    /// Represents a connection to a client in the named pipe server.
    /// </summary>
    public class ClientConnection
    {
        private readonly NamedPipeServerStream _pipe;
        private readonly byte[] _readBuffer = new byte[65535];
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private int _restartAttempts = 0;

        public ClientConnection(string pipeName, int instanceCount)
        {
            _pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, instanceCount, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        }

        public object State { get; set; }
        public bool Connected { get { return _pipe.IsConnected; } }

        public void Start()
        {
            _pipe.BeginWaitForConnection(OnAcceptConnection, this);
        }

        private void OnAcceptConnection(IAsyncResult ar)
        {
            try
            {
                _pipe.EndWaitForConnection(ar);
                BeginRead();
                _restartAttempts = 0;
            }
            catch (Exception exception)
            {
                if (_restartAttempts == 10)
                {
                    UnhandledException(this, new UnhandledExceptionEventArgs(exception));
                    return;
                }

                WaitForConnection();
            }
        }

        private void WaitForConnection()
        {
            try
            {
                _restartAttempts++;
                _pipe.BeginWaitForConnection(OnAcceptConnection, null);
            }
            catch (Exception exception)
            {
                UnhandledException(this, new UnhandledExceptionEventArgs(exception));
            }
        }

        public void BeginRead()
        {
            _pipe.BeginRead(_readBuffer, 0, _readBuffer.Length, OnReceive, null);
        }

        protected void Write(string command, params string[] argv)
        {
            try
            {
                var str = string.Format("{0}\x4{1}\n", command, string.Join("\x4", argv));
                var buffer = Encoding.ASCII.GetBytes(str);
                _pipe.Write(buffer, 0, buffer.Length);
            }
            catch (Exception exception)
            {
                UnhandledException(this, new UnhandledExceptionEventArgs(exception));
            }
        }

        private void OnReceive(IAsyncResult ar)
        {
            var bytesRead = _pipe.EndRead(ar);
            var str = Encoding.ASCII.GetString(_readBuffer, 0, bytesRead);
            _stringBuilder.Append(str);

            ProcessInBuffer();

            _pipe.BeginRead(_readBuffer, 0, _readBuffer.Length, OnReceive, null);
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

                        ReceivedCommand(this,
                            new ClientReceivedCommandEventArgs(commandParts[0], commandParts[1], commandParts.Skip(2).ToArray()));
                    }
                    catch (Exception exception)
                    {
                        UnhandledException(this, new UnhandledExceptionEventArgs(exception));
                        var str = Encoding.UTF8.GetBytes(packet);
                        var error = Convert.ToBase64String(Encoding.UTF8.GetBytes(exception.ToString()));
                        var inpacket = Convert.ToBase64String(str);
                        try
                        {
                            Write("error", inpacket, error);
                        }
                        catch (Exception ex)
                        {
                            UnhandledException(this, new UnhandledExceptionEventArgs(ex));
                        }
                    }

                    index = 0;
                }
            }
        }

        public event EventHandler<ClientReceivedCommandEventArgs> ReceivedCommand = delegate { };
        public event EventHandler Disconnected = delegate { };
        public event EventHandler<UnhandledExceptionEventArgs> UnhandledException = delegate { };

        public void Stop()
        {
            _pipe.Close();
        }
    }
}