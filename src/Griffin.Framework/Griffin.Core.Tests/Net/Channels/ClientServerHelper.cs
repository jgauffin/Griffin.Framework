using System;
using System.Net;
using System.Net.Sockets;

namespace Griffin.Core.Tests.Net.Channels
{
    public class ClientServerHelper : IDisposable
    {
        private ClientServerHelper()
        {
            
        }

        public static ClientServerHelper Create()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var ar = listener.BeginAcceptSocket(null, null);

            var helper = new ClientServerHelper();
            helper.Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            helper.Client.Connect(IPAddress.Loopback, ((IPEndPoint)listener.LocalEndpoint).Port);
            helper.Server = listener.EndAcceptSocket(ar);
            listener.Stop();
            return helper;
        }

        public Socket Client { get; set; }
        public Socket Server { get; set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Client.Dispose();
            Server.Dispose();
        }
    }

    //public class SecureClientServerHelper : IDisposable
    //{
    //    private X509Certificate2 _cert;

    //    private SecureClientServerHelper()
    //    {
    //        _cert = new X509Certificate2(
    //            AppDomain.CurrentDomain.BaseDirectory + "\\cert\\GriffinNetworkingTemp.pfx", "mamma");
    //    }

    //    public static ClientServerHelper Create()
    //    {
    //        var listener = new TcpListener(IPAddress.Loopback, 0);
    //        listener.Start();
    //        var ar = listener.BeginAcceptSocket(null, null);

    //        var helper = new SecureClientServerHelper();
    //        helper.Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    //        helper.Client.Connect(IPAddress.Loopback, ((IPEndPoint)listener.LocalEndpoint).Port);
    //        helper.Server = listener.EndAcceptSocket(ar);
    //        listener.Stop();
    //        return helper;
    //    }

    //    public ITcpChannel Client { get; set; }
    //    public ITcpChannel Server { get; set; }

    //    /// <summary>
    //    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    //    /// </summary>
    //    public void Dispose()
    //    {
    //        Client.Dispose();
    //        Server.Dispose();
    //    }
    //}

}