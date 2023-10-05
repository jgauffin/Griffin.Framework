using System.Security.Claims;
using System.Threading.Tasks;
using DotNetCqs;

namespace Griffin.Cqs.Routing
{
    /// <summary>
    /// Uses a routing rule to determine if this bus can handle the specified command
    /// </summary>
    public class RoutedMessageBus : IMessageBus, IRoutedBus
    {
        private readonly IRoutingRule _routingRule;
        private readonly IMessageBus _inner;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutedMessageBus"/> class.
        /// </summary>
        /// <param name="routingRule">The routing rule.</param>
        /// <param name="inner">Bus to invoke command on if the rule accepts the command.</param>
        public RoutedMessageBus(IRoutingRule routingRule, IMessageBus inner)
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

        /// <inheritdoc />
        public Task SendAsync(ClaimsPrincipal principal, object message)
        {
            return _inner.SendAsync(principal, message);
        }

        /// <inheritdoc />
        public Task SendAsync(ClaimsPrincipal principal, Message message)
        {
            return _inner.SendAsync(principal, message);
        }

        /// <inheritdoc />
        public Task SendAsync(Message message)
        {
            return _inner.SendAsync(message);
        }

        /// <inheritdoc />
        public Task SendAsync(object message)
        {
            return _inner.SendAsync(message);
        }
    }
}