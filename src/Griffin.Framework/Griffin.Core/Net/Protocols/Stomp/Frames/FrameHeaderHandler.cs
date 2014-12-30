namespace Griffin.Net.Protocols.Stomp.Frames
{
    /// <summary>
    /// Delegate used to process header line in a frame.
    /// </summary>
    /// <param name="name">Header name</param>
    /// <param name="value">Header value</param>
    public delegate void FrameHeaderHandler(string name, string value);
}