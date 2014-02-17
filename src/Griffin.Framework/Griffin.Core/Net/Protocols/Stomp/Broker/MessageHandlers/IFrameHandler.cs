namespace Griffin.Net.Protocols.Stomp.Broker.MessageHandlers
{
    interface IFrameHandler
    {
        IFrame Process(IStompClient client, IFrame request);
    }
}
