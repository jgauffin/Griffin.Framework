using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using DotNetCqs;

namespace Griffin.Cqs.Routing
{
    /// <summary>
    ///     A services that routes objects for all types of bus's.
    /// </summary>
    public class RoutedCqsService : IEventBus, ICommandBus, IRequestReplyBus, IQueryBus
    {
        private readonly List<RoutedCommandBus> _commandRoutes = new List<RoutedCommandBus>();
        private readonly List<RoutedEventBus> _eventRoutes = new List<RoutedEventBus>();
        private readonly List<RoutedQueryBus> _queryRoutes = new List<RoutedQueryBus>();
        private readonly List<RoutedRequestReplyBus> _requestRoutes = new List<RoutedRequestReplyBus>();

        /// <summary>
        ///     Request that a command should be executed.
        /// </summary>
        /// <typeparam name="T">Type of command to execute.</typeparam>
        /// <param name="command">Command to execute</param>
        /// <returns>
        ///     Task which completes once the command has been delivered (and NOT when it has been executed).
        /// </returns>
        /// <exception cref="CqsHandlerMissingException">Missing a route</exception>
        /// <remarks>
        ///     The actual execution of an command can be done anywhere at any time. Do not expect the command to be executed just
        ///     because this method returns. That just means
        ///     that the command have been successfully delivered (to a queue or another process etc) for execution.
        /// </remarks>
        public async Task ExecuteAsync<T>(T command) where T : Command
        {
            foreach (var route in _commandRoutes)
            {
                if (route.Match(command))
                {
                    await route.ExecuteAsync(command);
                    return;
                }
            }

            throw new CqsHandlerMissingException(typeof (T));
        }

        /// <summary>
        ///     Publish a new application event.
        /// </summary>
        /// <typeparam name="TApplicationEvent">Type of event to publish.</typeparam>
        /// <param name="e">Event to publish, must be serializable.</param>
        /// <returns>
        ///     Task triggered once the event has been delivered.
        /// </returns>
        /// <exception cref="CqsHandlerMissingException">Missing a route</exception>
        public async Task PublishAsync<TApplicationEvent>(TApplicationEvent e)
            where TApplicationEvent : ApplicationEvent
        {
            foreach (var route in _eventRoutes)
            {
                if (route.Match(e))
                {
                    await route.PublishAsync(e);
                    return;
                }
            }

            throw new CqsHandlerMissingException(typeof (TApplicationEvent));
        }

        /// <summary>
        ///     Invoke a query and wait for the result
        /// </summary>
        /// <typeparam name="TResult">Type of result that the query will return</typeparam>
        /// <param name="query">Query to execute.</param>
        /// <returns>
        ///     Task which will complete once we've got the result (or something failed, like a query wait timeout).
        /// </returns>
        /// <exception cref="CqsHandlerMissingException">Missing a route</exception>
        public async Task<TResult> QueryAsync<TResult>(Query<TResult> query)
        {
            foreach (var route in _queryRoutes)
            {
                if (route.Match(query))
                {
                    return await route.QueryAsync(query);
                }
            }

            throw new CqsHandlerMissingException(query.GetType());
        }

        /// <summary>
        ///     Invoke a request and wait for the reply
        /// </summary>
        /// <typeparam name="TReply">Type of reply that we should get for the request.</typeparam>
        /// <param name="request">Request that we want a reply for.</param>
        /// <returns>
        ///     Task which will complete once we've got the result (or something failed, like a request wait timeout).
        /// </returns>
        /// <exception cref="CqsHandlerMissingException">Missing a route</exception>
        public async Task<TReply> ExecuteAsync<TReply>(Request<TReply> request)
        {
            foreach (var route in _requestRoutes)
            {
                if (route.Match(request))
                {
                    return await route.ExecuteAsync(request);
                }
            }

            throw new CqsHandlerMissingException(request.GetType());
        }

        /// <summary>
        ///     Routes commands that are declared in the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="destination">The destination.</param>
        public void RouteCommands(Assembly assembly, ICommandBus destination)
        {
            _commandRoutes.Add(new RoutedCommandBus(new AssemblyRule(assembly), destination));
        }

        /// <summary>
        ///     Route commands using a custom rule.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <param name="destination">The destination to invoke if the rule accepts the CQS object.</param>
        /// <exception cref="System.ArgumentNullException">
        ///     rule
        ///     or
        ///     destination
        /// </exception>
        public void RouteCommands(IRoutingRule rule, ICommandBus destination)
        {
            if (rule == null) throw new ArgumentNullException("rule");
            if (destination == null) throw new ArgumentNullException("destination");
            _commandRoutes.Add(new RoutedCommandBus(rule, destination));
        }

        /// <summary>
        ///     Routes events that are declared in the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="destination">The destination.</param>
        public void RouteEvents(Assembly assembly, IEventBus destination)
        {
            _eventRoutes.Add(new RoutedEventBus(new AssemblyRule(assembly), destination));
        }

        /// <summary>
        ///     Route events using a custom rule.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <param name="destination">The destination to invoke if the rule accepts the CQS object.</param>
        /// <exception cref="System.ArgumentNullException">
        ///     rule
        ///     or
        ///     destination
        /// </exception>
        public void RouteEvents(IRoutingRule rule, IEventBus destination)
        {
            if (rule == null) throw new ArgumentNullException("rule");
            if (destination == null) throw new ArgumentNullException("destination");
            _eventRoutes.Add(new RoutedEventBus(rule, destination));
        }

        /// <summary>
        ///     Routes queries that are declared in the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="destination">The destination.</param>
        public void RouteQueries(Assembly assembly, IQueryBus destination)
        {
            _queryRoutes.Add(new RoutedQueryBus(new AssemblyRule(assembly), destination));
        }

        /// <summary>
        ///     Route queries using a custom rule.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <param name="destination">The destination to invoke if the rule accepts the CQS object.</param>
        /// <exception cref="System.ArgumentNullException">
        ///     rule
        ///     or
        ///     destination
        /// </exception>
        public void RouteQueries(IRoutingRule rule, IQueryBus destination)
        {
            if (rule == null) throw new ArgumentNullException("rule");
            if (destination == null) throw new ArgumentNullException("destination");
            _queryRoutes.Add(new RoutedQueryBus(rule, destination));
        }

        /// <summary>
        ///     Routes requests that are declared in the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="destination">The destination.</param>
        public void RouteRequests(Assembly assembly, IRequestReplyBus destination)
        {
            _requestRoutes.Add(new RoutedRequestReplyBus(new AssemblyRule(assembly), destination));
        }

        /// <summary>
        ///     Route requests using a custom rule.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <param name="destination">The destination to invoke if the rule accepts the CQS object.</param>
        /// <exception cref="System.ArgumentNullException">
        ///     rule
        ///     or
        ///     destination
        /// </exception>
        public void RouteRequests(IRoutingRule rule, IRequestReplyBus destination)
        {
            if (rule == null) throw new ArgumentNullException("rule");
            if (destination == null) throw new ArgumentNullException("destination");
            _requestRoutes.Add(new RoutedRequestReplyBus(rule, destination));
        }
    }
}