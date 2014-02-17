using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Griffin.Net.Channels
{
    /// <summary>
    ///     A channel is used to send and receive information over a socket.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Channels should be designed so that they can be reused after a client have disconnected. Hence you have sure
    ///         that the state is cleared when the <c>Cleanup()</c> method is invoked. Buffers etc should still be used,
    ///         but any internal send queue etc should be emptied.
    ///     </para>
    /// </remarks>
    public interface ITcpChannel
    {
        /// <summary>
        ///     Channel got disconnected
        /// </summary>
        DisconnectHandler Disconnected { get; set; }

        /// <summary>
        ///     Channel received a new message
        /// </summary>
        MessageHandler MessageReceived { get; set; }

        /// <summary>
        ///     Channel have sent a message
        /// </summary>
        MessageHandler MessageSent { get; set; }

        /// <summary>
        ///     Gets address of the connected end point.
        /// </summary>
        EndPoint RemoteEndpoint { get; }

        /// <summary>
        /// Identity of this channel
        /// </summary>
        /// <remarks>
        /// Must be unique within a server.
        /// </remarks>
        string ChannelId { get; }

        /// <summary>
        ///     Assign a socket to this channel
        /// </summary>
        /// <param name="socket">Connected socket</param>
        /// <remarks>
        ///     the channel will start receive new messages as soon as you've called assign.
        /// </remarks>
        void Assign(Socket socket);

        /// <summary>
        ///     Cleanup everything so that the channel can be reused.
        /// </summary>
        void Cleanup();

        /// <summary>
        ///     Send a new message
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <remarks>
        ///     <para>
        ///         Outbound messages are enqueued and sent in order.
        ///     </para>
        ///     <para>
        ///         You may enqueue <c>byte[]</c> arrays or <see cref="Stream" />  objects. They will not be serialized but
        ///         MicroMessage framed directly.
        ///     </para>
        /// </remarks>
        void Send(object message);


        /// <summary>
        /// Can be used to store information in the channel so that you can access it at later requests.
        /// </summary>
        /// <remarks>
        /// <para>All data is lost when the channel is closed.</para>
        /// </remarks>
        IChannelData Data { get; }

        /// <summary>
        /// Close channel
        /// </summary>
        void Close();
    }

    public interface IAutoConnectingTcpChannel
    {
        void Start();
        bool IsReady { get; }

    }
}