using System;
using Griffin.Net.Buffers;

namespace Griffin.Net
{
    /// <summary>
    /// Configuration for <see cref="MessagingServer{TContext}"/>
    /// </summary>
    public class MessagingServerConfiguration<TContext> where TContext : IMiddlewareContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingServerConfiguration{TContext}"/> class.
        /// </summary>
        /// <param name="decoderFactory">Used to create an decoder for every new accepted connection.</param>
        /// <param name="encoderFactory">Used to create an encoder for every new accepted connection.</param>
        /// <exception cref="System.ArgumentNullException">
        /// decoderFactory
        /// or
        /// encoderFactory
        /// </exception>
        public MessagingServerConfiguration()
        {
            BufferPool = new BufferManager(100, 65535);
        }

        public Func<MessagingServerHandlerFactoryContext, IClientHandler<TContext>> HandlerFactory { get; set; }

        /// <summary>
        /// Pool used to allocate buffers for every client
        /// </summary>
        /// <remarks>
        /// Each client requires one buffer (for receiving).
        /// </remarks>
        /// <value>
        /// 100 buffers of size 65535 bytes are allocated per default.
        /// </value>
        public BufferManager BufferPool { get; set; }


    }
}