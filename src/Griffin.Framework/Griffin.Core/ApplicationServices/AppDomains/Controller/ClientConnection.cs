using System;
using System.IO.Pipes;
using System.Linq;
using System.Text;

namespace Griffin.ApplicationServices.AppDomains.Controller
{
    /// <summary>
    ///     Represents a connection to a client in the named pipe server.
    /// </summary>
    public class ClientConnection
    {
        private readonly NamedPipeServerStream _pipe;
        private readonly byte[] _readBuffer = new byte[65535];
        private readonly StringBuilder _stringBuilder = new StringBuilder();
        private int _restartAttempts;


        /// <summary>
        ///     Initializes a new instance of the <see cref="ClientConnection" /> class.
        /// </summary>
        /// <param name="pipeName">Name of the pipe.</param>
        /// <param name="instanceCount">The instance count.</param>
        /// <exception cref="System.ArgumentNullException">pipeName</exception>
        public ClientConnection(string pipeName, int instanceCount)
        {
            if (pipeName == null) throw new ArgumentNullException("pipeName");
            _pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, instanceCount, PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);
        }

        /// <summary>
        ///     You can assign your own object here to identify the connection
        /// </summary>
        public object UserState { get; set; }

        /// <summary>
        ///     Checks if we are connected. Not thread safe nor 100% reliable as all disconnects such as network failure are not
        ///     automatically detected.
        /// </summary>
        public bool Connected
        {
            get { return _pipe.IsConnected; }
        }

        /// <summary>
        ///     Start receiving information from the remote end point.
        /// </summary>
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

        /// <summary>
        ///     Begin receiving information.
        /// </summary>
        public void BeginRead()
        {
            _pipe.BeginRead(_readBuffer, 0, _readBuffer.Length, OnReceive, null);
        }

        /// <summary>
        ///     Write a command over the connection.
        /// </summary>
        /// <param name="command">Command name</param>
        /// <param name="argv">Argument list</param>
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
                            new ClientReceivedCommandEventArgs(commandParts[0], commandParts[1],
                                commandParts.Skip(2).ToArray()));
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

        /// <summary>
        ///     Received a command from the server.
        /// </summary>
        public event EventHandler<ClientReceivedCommandEventArgs> ReceivedCommand = delegate { };

        /// <summary>
        ///     Detected a disconnect.
        /// </summary>
        public event EventHandler Disconnected = delegate { };

        /// <summary>
        ///     An unexpected exception.
        /// </summary>
        public event EventHandler<UnhandledExceptionEventArgs> UnhandledException = delegate { };

        /// <summary>
        ///     Stop server.
        /// </summary>
        public void Stop()
        {
            _pipe.Close();
        }
    }
}