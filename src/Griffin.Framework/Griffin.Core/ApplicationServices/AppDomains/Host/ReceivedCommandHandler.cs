namespace Griffin.ApplicationServices.AppDomains.Host
{
    /// <summary>
    /// Delegate used when receiving commands
    /// </summary>
    /// <param name="command">Command name</param>
    /// <param name="argv">arguments</param>
    public delegate void ReceivedCommandHandler(string command, string[] argv);
}