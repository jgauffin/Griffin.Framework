using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using DotNetCqs;

namespace Griffin.Cqs.Routing
{
    /// <summary>
    ///     A services that routes objects for all types of bus's.
    /// </summary>
    public class RoutedCqsService : IMessageBus, IQueryBus
    {
        private readonly List<RoutedMessageBus> _messageRoutes = new List<RoutedMessageBus>();
        private readonly List<RoutedQueryBus> _queryRoutes = new List<RoutedQueryBus>();
        
        public async Task<TResult> QueryAsync<TResult>(ClaimsPrincipal principal, Query<TResult> query)
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
        public void RouteMessages(Assembly assembly, IMessageBus destination)
        {
            _messageRoutes.Add(new RoutedMessageBus(new AssemblyRule(assembly), destination));
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
        public void RouteMessages(IRoutingRule rule, IMessageBus destination)
        {
            if (rule == null) throw new ArgumentNullException("rule");
            if (destination == null) throw new ArgumentNullException("destination");
            _messageRoutes.Add(new RoutedMessageBus(rule, destination));
        }

        public async Task SendAsync(ClaimsPrincipal principal, object message)
        {
            foreach (var route in _messageRoutes)
            {
                if (route.Match(message))
                {
                    await route.SendAsync(message);
                }
            }

            throw new CqsHandlerMissingException(message.GetType());
        }

        public async Task SendAsync(ClaimsPrincipal principal, Message message)
        {
            foreach (var route in _messageRoutes)
            {
                if (route.Match(message.Body))
                {
                    await route.SendAsync(message);
                }
            }

            throw new CqsHandlerMissingException(message.GetType());
        }

        public async Task SendAsync(Message message)
        {
            foreach (var route in _messageRoutes)
            {
                if (route.Match(message.Body))
                {
                    await route.SendAsync(message.Body);
                }
            }

            throw new CqsHandlerMissingException(message.Body.GetType());
        }

        public async Task SendAsync(object message)
        {
            foreach (var route in _messageRoutes)
            {
                if (route.Match(message))
                {
                    await route.SendAsync(message);
                }
            }

            throw new CqsHandlerMissingException(message.GetType());
        }
    }
}