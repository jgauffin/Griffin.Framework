using System.Threading.Tasks;
using DotNetCqs;

namespace Griffin.Cqs.Routing
{
    /// <summary>
    /// Uses a routing rule to determine if this bus can handle the specified command
    /// </summary>
    public class RoutedCommandBus : ICommandBus, IRoutedBus
    {
        private readonly IRoutingRule _routingRule;
        private readonly ICommandBus _inner;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutedCommandBus"/> class.
        /// </summary>
        /// <param name="routingRule">The routing rule.</param>
        /// <param name="inner">Bus to invoke command on if the rule accepts the command.</param>
        public RoutedCommandBus(IRoutingRule routingRule, ICommandBus inner)
        {
            _routingRule = routingRule;
            _inner = inner;
        }

        /// <summary>
        /// Match object against a rule
        /// </summary>
        /// <param name="cqsObject">The CQS object.</param>
        /// <returns>
        ///   <c>true</c> if this bus can handle the specified object; otherwise <c>false</c>.
        /// </returns>
        public bool Match(object cqsObject)
        {
            return _routingRule.Match(cqsObject);
        }

        /// <summary>
        /// Request that a command should be executed.
        /// </summary>
        /// <typeparam name="T">Type of command to execute.</typeparam>
        /// <param name="command">Command to execute</param>
        /// <returns>
        /// Task which completes once the command has been delivered (and NOT when it has been executed).
        /// </returns>
        /// <remarks>
        /// The actual execution of an command can be done anywhere at any time. Do not expect the command to be executed just because this method returns. That just means
        /// that the command have been successfully delivered (to a queue or another process etc) for execution.
        /// </remarks>
        public async Task ExecuteAsync<T>(T command) where T : Command
        {
            await _inner.ExecuteAsync(command);
        }
    }
}