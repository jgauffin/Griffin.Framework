using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace Griffin.ApplicationServices.AppDomains.Controller
{
    internal class NamedPipeServer
    {
        private ClientConnection[] _clients = new ClientConnection[3];
        private string _pipeName = Guid.NewGuid().ToString("N");

        public string PipeName
        {
            get { return _pipeName; }
        }

        public void Start()
        {
            for (int i = 0; i < 3; i++)
            {
                var clientConnection = new ClientConnection(_pipeName, 3);
                clientConnection.ReceivedCommand += OnClientCommand;
                clientConnection.Disconnected += OnClientDisconnect;
                clientConnection.UnhandledException += OnClientException;
                clientConnection.Start();
            }
            
        }

        public event EventHandler<ClientReceivedCommandEventArgs> ReceivedCommand = delegate { };
        public event EventHandler ClientDisconnected = delegate { };
        public event EventHandler<UnhandledExceptionEventArgs> ClientConnectionFailure = delegate { };

        private void OnClientException(object sender, UnhandledExceptionEventArgs e)
        {
            ClientConnectionFailure(sender, e);
        }

        private void OnClientDisconnect(object sender, EventArgs e)
        {
            ClientDisconnected(sender, e);
        }

        private void OnClientCommand(object sender, ClientReceivedCommandEventArgs e)
        {
            ReceivedCommand(sender, e);
        }

        public void Stop()
        {
            foreach (var client in _clients)
            {
                if (client.Connected)
                    client.Stop();
            }
        }

    

       
    }


    public delegate void ReceivedCommandHandler(string command, string[] argv);
}