using System.Threading.Tasks;
using DotNetCqs;

namespace Griffin.Cqs.Routing
{
    /// <summary>
    /// Uses a routing rule to determine if this bus can handle the specified query
    /// </summary>
    public class RoutedQueryBus : IQueryBus, IRoutedBus
    {
        private readonly IRoutingRule _routingRule;
        private readonly IQueryBus _inner;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutedCommandBus"/> class.
        /// </summary>
        /// <param name="routingRule">The routing rule.</param>
        /// <param name="inner">Bus to invoke command on if the rule accepts the command.</param>
        public RoutedQueryBus(IRoutingRule routingRule, IQueryBus inner)
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
        /// Invoke a query and wait for the result
        /// </summary>
        /// <typeparam name="TResult">Type of result that the query will return</typeparam>
        /// <param name="query">Query to execute.</param>
        /// <returns>
        /// Task which will complete once we've got the result (or something failed, like a query wait timeout).
        /// </returns>
        public async Task<TResult> QueryAsync<TResult>(Query<TResult> query)
        {
            return await _inner.QueryAsync(query);
        }
    }
}