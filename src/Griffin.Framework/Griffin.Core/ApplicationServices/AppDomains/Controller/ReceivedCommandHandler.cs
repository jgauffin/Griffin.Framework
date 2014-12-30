namespace Griffin.ApplicationServices.AppDomains.Controller
{
    /// <summary>
    /// Delegate used to process incoming commands
    /// </summary>
    /// <param name="command">Name of the command to execute</param>
    /// <param name="argv">Arguments for the command</param>
    public delegate void ReceivedCommandHandler(string command, string[] argv);
}