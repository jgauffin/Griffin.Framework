using System.Threading.Tasks;
using DotNetCqs;

namespace Griffin.Cqs.Routing
{
    /// <summary>
    /// Uses a routing rule to determine if this bus can handle the specified request.
    /// </summary>
    public class RoutedRequestReplyBus : IRequestReplyBus, IRoutedBus
    {
        private readonly IRoutingRule _routingRule;
        private readonly IRequestReplyBus _inner;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutedCommandBus"/> class.
        /// </summary>
        /// <param name="routingRule">The routing rule.</param>
        /// <param name="inner">Bus to invoke request on if the rule accepts the request.</param>
        public RoutedRequestReplyBus(IRoutingRule routingRule, IRequestReplyBus inner)
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
        /// Invoke a request and wait for the reply
        /// </summary>
        /// <typeparam name="TReply">Type of reply that we should get for the request.</typeparam>
        /// <param name="request">Request that we want a reply for.</param>
        /// <returns>
        /// Task which will complete once we've got the result (or something failed, like a request wait timeout).
        /// </returns>
        public async Task<TReply> ExecuteAsync<TReply>(Request<TReply> request)
        {
            return await _inner.ExecuteAsync(request);
        }
    }
}