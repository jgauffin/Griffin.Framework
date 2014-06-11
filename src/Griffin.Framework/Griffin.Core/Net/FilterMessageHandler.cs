using Griffin.Net.Channels;

namespace Griffin.Net
{
    /// <summary>
    /// Used to be able to determine if a certain message should be handled or not
    /// </summary>
    /// <param name="channel">Channel that received the message</param>
    /// <param name="message">Message to be processed.</param>
    /// <returns>Result</returns>
    public delegate ClientFilterResult FilterMessageHandler(ITcpChannel channel, object message);
}