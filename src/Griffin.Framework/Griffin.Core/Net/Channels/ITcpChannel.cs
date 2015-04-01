using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Griffin.Net.Channels
{
    /// <summary>
    ///     A channel is used to send and receive information over a socket.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Channels should be designed so that they can be reused after a client has disconnected. Hence you can be sure
    ///         that the state is cleared when the <c>Cleanup()</c> method is invoked. Buffers etc may still be being used,
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
        ///     Channel has sent a message
        /// </summary>
        MessageHandler MessageSent { get; set; }

        /// <summary>
        ///     Invoked if the decoder fails to handle an incoming message
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The handler MUST close the connection once a reply has been sent.
        ///     </para>
        /// </remarks>
        ChannelFailureHandler ChannelFailure { get; set; }

        /// <summary>
        ///     Checks if the channel is connected.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        ///     Gets address of the connected end point.
        /// </summary>
        EndPoint RemoteEndpoint { get; }

        /// <summary>
        ///     Identity of this channel
        /// </summary>
        /// <remarks>
        ///     Must be unique within a server.
        /// </remarks>
        string ChannelId { get; }

        /// <summary>
        ///     Can be used to store information in the channel so that you can access it for later requests.
        /// </summary>
        /// <remarks>
        ///     <para>All data is lost when the channel is closed.</para>
        /// </remarks>
        IChannelData Data { get; }

        /// <summary>
        ///     Pre processes incoming bytes before they are passed to the message builder.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Can be used if you for instance use a custom authentication mechanism which needs to process incoming
        ///         bytes instead of deserialized messages.
        ///     </para>
        /// </remarks>
        BufferPreProcessorHandler BufferPreProcessor { get; set; }

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
        ///         sent directly with the transport protocol (like HTTP or MicroMsg).
        ///     </para>
        /// </remarks>
        void Send(object message);

        /// <summary>
        ///     Close channel
        /// </summary>
        void Close();

        /// <summary>
        ///     Close channel asynchronously
        /// </summary>
        /// <returns></returns>
        Task CloseAsync();
    }
}