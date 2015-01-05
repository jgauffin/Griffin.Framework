namespace Griffin.Net.Protocols.Stomp.Broker.MessageHandlers
{
    /// <summary>
    /// Used to process incoming frames
    /// </summary>
    interface IFrameHandler
    {
        /// <summary>
        /// Process an inbound frame.
        /// </summary>
        /// <param name="client">Connection that received the frame</param>
        /// <param name="request">Inbound frame to process</param>
        /// <returns>Frame to send back; <c>null</c> if no message should be returned;</returns>
        /// <exception cref="BadRequestException">Frame was not well formed or contains invalid information.</exception>
        IFrame Process(IStompClient client, IFrame request);
    }
}
