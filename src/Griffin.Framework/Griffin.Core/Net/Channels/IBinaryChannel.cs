using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Griffin.Net.Channels
{
    /// <summary>
    ///     Representing bare bones communication where we shuffle bytes.
    /// </summary>
    public interface IBinaryChannel : IOutboundBinaryChannel, IInboundBinaryChannel
    {
        /// <summary>
        ///     You can use channel data to store connection specific information (compare it to a HTTP session).
        /// </summary>
        IChannelData ChannelData { get; }

        /// <summary>
        ///     Last known state of the communication interface (updated each time an IO operation is done).
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        ///     Indicates if the channel is open or not.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The reliability of this flag varies depending on the communication technology. The connection
        ///         might be down due to network failures etc. hence it tells what the channel sees as the current truth,
        ///         but that can change with the next IO operation which would challenge that truth.
        ///     </para>
        /// </remarks>
        bool IsOpen { get; }

        /// <summary>
        ///     Amount of bytes which can be included in a single send operation.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         If the queued number of bytes (by using <c>SendMore()</c>) exceed this number of bytes the <c>SendMore()</c>
        ///         method will internally invoke <c>Send()</c> to
        ///         force a send operation.
        ///     </para>
        /// </remarks>
        int MaxBytesPerWriteOperation { get; set; }

        /// <summary>
        ///     Who we are talking to.
        /// </summary>
        EndPoint RemoteEndpoint { get; }

        /// <summary>
        ///     Channel data is an dictionary which means a lookup every time. This token can be used as an alternative.
        /// </summary>
        object UserToken { get; set; }

        /// <summary>
        ///     Channel have been closed (by either side).
        /// </summary>
        event EventHandler ChannelClosed;

        /// <summary>
        ///     Close channel.
        /// </summary>
        Task CloseAsync();

        /// <summary>
        /// Assign a socket to this channel.
        /// </summary>
        /// <param name="socket">A connected socket.</param>
        /// <exception cref="InvalidOperationException">This channel already have an connected socket. Cleanup before assigning a new one.</exception>
        void Assign(Socket socket);

        /// <summary>
        ///     Open channel, requires that connection information have been specified in another method.
        /// </summary>
        /// <exception cref="InvalidOperationException">Connection information have not yet been specified.</exception>
        /// <exception cref="NotSupportedException">
        ///     Channel is part of a server (which will initiate the channel by accepting new
        ///     connections)
        /// </exception>
        Task OpenAsync();

        /// <summary>
        ///     Open channel, requires that connection information have been specified in another method.
        /// </summary>
        /// <param name="endpoint">Remote endpoint.</param>
        /// <exception cref="InvalidOperationException">Connection information have not yet been specified.</exception>
        /// <exception cref="NotSupportedException">
        ///     Channel is part of a server (which will initiate the channel by accepting new
        ///     connections)
        /// </exception>
        Task OpenAsync(EndPoint endpoint);

        ///// <summary>
        /////     Enqueue something for delivery (will not do an actual send operation but enqueue data for delivery).
        ///// </summary>
        ///// <param name="buffer">Buffer to send.</param>
        ///// <param name="offset">Offset in buffer.</param>
        ///// <param name="count">Amount fo bytes to send, starting at offset.</param>
        ///// <param name="deliverIfChannelIsFree">
        /////     deliver if possible, otherwise enqueue (data will be sent when the pending write
        /////     operation completes).
        ///// </param>
        ///// <remarks>
        /////     <para>
        /////         Everything will be sent if the channel got no pending write operations, otherwise data will be queued.
        /////     </para>
        ///// </remarks>
        //Task SendMoreAsync(byte[] buffer, int offset, int count, bool deliverIfChannelIsFree);

        /// <summary>
        /// Reset all internal state so that the channel can be reused.
        /// </summary>
        void Reset();

    }
}