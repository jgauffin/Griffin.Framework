namespace Griffin.Net.Channels
{
    /// <summary>
    /// Delegate for <see cref="ITcpChannel.BufferPreProcessor"/>.
    /// </summary>
    /// <param name="channel">Channel that have received something</param>
    /// <param name="buffer">Buffer containing the processed bytes</param>
    /// <returns>Number of bytes process by the buffer pre processor (i.e. no one else should process those bytes)</returns>
    public delegate int BufferPreProcessorHandler(ITcpChannel channel, ISocketBuffer buffer);
}