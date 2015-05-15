using System.Threading.Tasks;
using DotNetCqs;

namespace Griffin.Cqs.Routing
{
    /// <summary>
    /// Uses a routing rule to determine if this bus can handle the specified command
    /// </summary>
    public class RoutedEventBus : IEventBus, IRoutedBus
    {
        private readonly IRoutingRule _routingRule;
        private readonly IEventBus _inner;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutedCommandBus"/> class.
        /// </summary>
        /// <param name="routingRule">The routing rule.</param>
        /// <param name="inner">Bus to invoke command on if the rule accepts the command.</param>
        public RoutedEventBus(IRoutingRule routingRule, IEventBus inner)
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
        /// Publish a new application event.
        /// </summary>
        /// <typeparam name="TApplicationEvent">Type of event to publish.</typeparam>
        /// <param name="e">Event to publish, must be serializable.</param>
        /// <returns>
        /// Task triggered once the event has been delivered.
        /// </returns>
        public async Task PublishAsync<TApplicationEvent>(TApplicationEvent e) where TApplicationEvent : ApplicationEvent
        {
            await _inner.PublishAsync(e);
        }
    }
}